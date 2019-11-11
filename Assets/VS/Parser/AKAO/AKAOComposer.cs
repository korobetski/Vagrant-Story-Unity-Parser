using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Format;
using VS.Utils;

//Minoru Akao
//https://github.com/vgmtrans/vgmtrans/blob/master/src/main/formats/AkaoSeq.cpp
// Akao in MUSIC folder contains music instructions like a Midi file.
namespace VS.Parser.Akao
{
    public class AKAOComposer
    {
        public static readonly ushort[] delta_time_table = { 0xC0, 0x60, 0x30, 0x18, 0x0C, 0x6, 0x3, 0x20, 0x10, 0x8, 0x4, 0x0, 0xA0A0, 0xA0A0 };

        private BinaryReader buffer;
        private string name;
        private long start;
        private long end;
        private uint numTrack;
        private uint numInstr;
        private AKAOTrack[] tracks;


        private uint velocity = 127;
        private int quarterNote = 0x30;

        public static bool UseDebug = false;

        public AKAOComposer(BinaryReader buffer, long start, long end, uint NI, uint NT, string name, bool UseDebug = false)
        {
            this.buffer = buffer;
            AKAOComposer.UseDebug = UseDebug;
            numTrack = NT;
            numInstr = NI;
            this.name = name;

            this.start = start;
            this.end = end;

            buffer.BaseStream.Position = start;
            SetTracks();
        }

        public void OutputMidiFile()
        {
            List<byte> midiByte = new List<byte>();
            midiByte.AddRange(new byte[] { 0x4D, 0x54, 0x68, 0x64 }); // MThd Header
            midiByte.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x06 }); // Chunck length
            midiByte.AddRange(new byte[] { 0x00, 0x01 }); // Format Midi 1
            midiByte.Add((byte)(((byte)(tracks.Length) & 0xFF00) >> 8)); //num tracks hi
            midiByte.Add((byte)((byte)(tracks.Length) & 0x00FF)); //num tracks lo
            midiByte.Add((byte)((quarterNote & 0xFF00) >> 8)); //Per Quarter Note hi
            midiByte.Add((byte)(quarterNote & 0x00FF)); //Per Quarter Note lo
            foreach (AKAOTrack track in tracks)
            {
                midiByte.AddRange(new byte[] { 0x4D, 0x54, 0x72, 0x6B }); // MTrk Header
                List<byte> tb = new List<byte>();
                foreach (AKAOEvent ev in track.Events)
                {
                    List<byte> evb = ev.GetMidiBytes();
                    if (evb != null)
                    {
                        tb.AddRange(evb);
                    }
                }
                midiByte.AddRange(new byte[] { (byte)((tb.Count + 4 & 0xFF000000) >> 24), (byte)((tb.Count + 4 & 0x00FF0000) >> 16), (byte)((tb.Count + 4 & 0x0000FF00) >> 8), (byte)(tb.Count + 4 & 0x000000FF) }); // Chunck length
                midiByte.AddRange(tb); // Track datas
                midiByte.AddRange(new byte[] { 0x00, 0xFF, 0x2F, 0x00 }); // End Track
            }

            ToolBox.DirExNorCreate("Assets/Resources/Sounds/");
            using (FileStream fs = File.Create("Assets/Resources/Sounds/" + name + ".mid"))
            {
                for (int i = 0; i < midiByte.Count; i++)
                {
                    fs.WriteByte(midiByte[i]);
                }
                fs.Close();
            }
        }


        private void SetTracks()
        {
            long beginOffset = buffer.BaseStream.Position;
            //Debug.Log("SetTracks : "+ numTrack);
            tracks = new AKAOTrack[numTrack];
            for (uint i = 0; i < numTrack; i++)
            {
                tracks[i] = new AKAOTrack();
            }

            uint cTrackId = 0;
            bool playingNote = false;
            uint prevKey = 0;
            ushort delta = 0;
            uint channel = 0;
            uint octave = 0;

            long repeatBegin = 0;
            int repeatNumber = 0;
            List<long> repeaterEndPositions = new List<long>();
            List<long> condLoops = new List<long>();


            if (AKAOComposer.UseDebug)
            {
                Debug.Log("## TRACK : " + cTrackId + "   -----------------------------------------------------------------------");
            }

            while (buffer.BaseStream.Position < end)
            {
                AKAOTrack curTrack;
                if (cTrackId < tracks.Length)
                {
                    curTrack = tracks[cTrackId];
                    channel = cTrackId % 0xF;
                    if (channel > 8)
                    {
                        channel++;
                    }
                    if (channel == 16)
                    {
                        channel = 0;
                    }
                }
                else
                {
                    curTrack = tracks[tracks.Length - 1]; // using the last track instead
                    channel = cTrackId % 0xF;
                    if (channel > 8)
                    {
                        channel++;
                    }
                    if (channel == 16)
                    {
                        channel = 0;
                    }
                }
                byte STATUS_BYTE = buffer.ReadByte();
                int i, k;

                //Debug.Log("##    STATUS_BYTE : " + STATUS_BYTE);
                if (STATUS_BYTE <= 0x9F)
                {
                    i = STATUS_BYTE / 11;
                    k = i * 2;
                    k += i;
                    k *= 4;
                    k -= i;
                    k = STATUS_BYTE - k;

                    if (STATUS_BYTE < 0x83) // Note On
                    {
                        if (playingNote)
                        {
                            curTrack.AddEvent(new EvNoteOff(channel, prevKey, delta));
                            delta = 0;
                            playingNote = false;
                        }

                        uint relativeKey = (uint)i;
                        uint baseKey = octave * 12;
                        uint key = baseKey + relativeKey;
                        curTrack.AddEvent(new EvNoteOn(channel, key, velocity, delta));
                        delta = delta_time_table[k];
                        prevKey = key;
                        playingNote = true;
                    }
                    else if (STATUS_BYTE < 0x8F) // Tie
                    {
                        ushort duration = delta_time_table[k];
                        delta += duration;
                        curTrack.AddEvent(new EvTieTime(duration));
                    }
                    else // Rest
                    {
                        if (playingNote == true)
                        {
                            curTrack.AddEvent(new EvNoteOff(channel, prevKey, delta));
                            delta = 0;
                            playingNote = false;
                        }

                        ushort duration = delta_time_table[k];
                        delta += (ushort)duration;
                        curTrack.AddEvent(new EvRest(delta));
                    }
                }
                else if ((STATUS_BYTE >= 0xF0) && (STATUS_BYTE <= 0xFB)) // Alternate Note On ?
                {
                    if (playingNote)
                    {
                        curTrack.AddEvent(new EvNoteOff(channel, prevKey, delta));
                        delta = 0;
                        playingNote = false;
                    }
                    uint relativeKey = (uint)STATUS_BYTE - 0xF0;
                    uint baseKey = octave * 12;
                    uint key = baseKey + relativeKey;
                    uint time = buffer.ReadByte();

                    curTrack.AddEvent(new EvNoteOn(channel, key, velocity, delta));
                    delta = (ushort)time;
                    prevKey = key;
                    playingNote = true;
                }
                else
                {
                    switch (STATUS_BYTE)
                    {
                        case 0xA0:
                            /*
                            curTrack.AddEvent(new EvEndTrack(cTrackId));
                            cTrackId++;
                            if (cTrackId < tracks.Length)
                            {
                                curTrack = tracks[cTrackId];
                            }
                            delta = 0;
                            */
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xA1:// Program Change
                            curTrack.AddEvent(new EvProgramChange(channel, (byte)(buffer.ReadByte() + numInstr), delta));
                            delta = 0;
                            break;
                        case 0xA2: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xA3:// Volume
                            uint volume = buffer.ReadByte();
                            curTrack.AddEvent(new EvVolume(channel, volume, delta));
                            delta = 0;
                            break;
                        case 0xA4:// Portamento
                            byte[] b = buffer.ReadBytes(2);
                            curTrack.AddEvent(new EvPortamento(channel, b[0], b[1]));
                            break;
                        case 0xA5:// Octave
                            octave = buffer.ReadByte();
                            curTrack.AddEvent(new EvOctave(octave));
                            break;
                        case 0xA6:// Octave ++
                            octave++;
                            curTrack.AddEvent(new EvOctaveUp());
                            break;
                        case 0xA7:// Octave --
                            octave--;
                            curTrack.AddEvent(new EvOctaveDown());
                            break;
                        case 0xA8:// Expression
                            uint expression = buffer.ReadByte();
                            curTrack.AddEvent(new EvExpr(channel, expression, delta));
                            delta = 0;
                            break;
                        case 0xA9:// Expression Slide
                            uint duration = buffer.ReadByte();
                            expression = buffer.ReadByte();
                            curTrack.AddEvent(new EvExprSlide(duration, expression));
                            break;
                        case 0xAA:// Pan
                            int pan = buffer.ReadByte();
                            curTrack.AddEvent(new EvPan(channel, pan, delta));
                            delta = 0;
                            break;
                        case 0xAB:// Pan Fade
                            duration = buffer.ReadByte();
                            pan = buffer.ReadByte();
                            curTrack.AddEvent(new EvPanSlide(channel, duration, pan));
                            break;
                        case 0xAC: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xAD: // Attack
                            int attack = buffer.ReadByte();
                            curTrack.AddEvent(new EvAttack(attack));
                            break;
                        case 0xAE: // Decay
                            int decay = buffer.ReadByte();
                            curTrack.AddEvent(new EvDecay(decay));
                            break;
                        case 0xAF: // Sustain
                            int sustain = buffer.ReadByte();
                            curTrack.AddEvent(new EvSustain(sustain));
                            break;
                        case 0xB0: // Decay + Sustain
                            decay = buffer.ReadByte();
                            sustain = buffer.ReadByte();
                            curTrack.AddEvent(new EvDecay(decay));
                            curTrack.AddEvent(new EvSustain(sustain));
                            break;
                        case 0xB1: // Sustain release
                            duration = buffer.ReadByte();
                            curTrack.AddEvent(new EvSustainRelease(duration));
                            break;
                        case 0xB2: // Release
                            duration = buffer.ReadByte();
                            curTrack.AddEvent(new EvRelease(duration));
                            break;
                        case 0xB3: // Reset ADSR (Attack-Decay-Sustain-Release)
                            curTrack.AddEvent(new EvResetADSR());
                            break;
                        // LFO (low-frequency oscillators) Pitch bend
                        case 0xB4: // LFO Pitch bend Range
                            b = buffer.ReadBytes(3);
                            curTrack.AddEvent(new EvLFOPitchRange(b[0], b[1], b[2]));
                            break;
                        case 0xB5: // LFO Pitch bend Depth
                            int depth = buffer.ReadByte();
                            curTrack.AddEvent(new EvLFOPitchDepth(depth));
                            break;
                        case 0xB6: // LFO Pitch bend Off
                            curTrack.AddEvent(new EvLFOPitchOff());
                            break;
                        case 0xB7: // LFO Pitch bend ??
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        // LFO (low-frequency oscillators) Expression
                        case 0xB8: // LFO Expression Range
                            b = buffer.ReadBytes(3);
                            curTrack.AddEvent(new EvLFOExprRange(b[0], b[1], b[2]));
                            break;
                        case 0xB9: // LFO Expression Depth
                            depth = buffer.ReadByte();
                            curTrack.AddEvent(new EvLFOExprDepth(depth));
                            break;
                        case 0xBA: // LFO Expression Off
                            curTrack.AddEvent(new EvLFOExprOff());
                            break;
                        case 0xBB: // LFO Expression ??
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        // LFO (low-frequency oscillators) Panpot
                        case 0xBC: // LFO Panpot Range
                            b = buffer.ReadBytes(2);
                            curTrack.AddEvent(new EvLFOPanpotRange(b[0], b[1]));
                            break;
                        case 0xBD: // LFO Panpot Depth
                            depth = buffer.ReadByte();
                            curTrack.AddEvent(new EvLFOPanpotDepth(depth));
                            break;
                        case 0xBE: // LFO Panpot Off
                            curTrack.AddEvent(new EvLFOPanpotOff());
                            break;
                        case 0xBF: // LFO Panpot ??
                            curTrack.AddEvent(new EvUnknown(buffer.ReadByte()));
                            break;
                        case 0xC0: // Transpose
                            int transpose = buffer.ReadByte();
                            curTrack.AddEvent(new EvTranspose(transpose));
                            break;
                        case 0xC1: // Transpose Move
                            transpose = buffer.ReadByte();
                            curTrack.AddEvent(new EvTransposeMove(transpose));
                            break;
                        case 0xC2: // Reverb On
                            curTrack.AddEvent(new EvReverbOn());
                            break;
                        case 0xC3: // Reverb Off
                            curTrack.AddEvent(new EvReverbOff());
                            break;
                        case 0xC4: // Noise On
                            curTrack.AddEvent(new EvNoiseOn());
                            break;
                        case 0xC5: // Noise Off
                            curTrack.AddEvent(new EvNoiseOff());
                            break;
                        case 0xC6: // FM (Frequency Modulation) On
                            curTrack.AddEvent(new EvFMOn());
                            break;
                        case 0xC7: // FM (Frequency Modulation) Off
                            curTrack.AddEvent(new EvFMOff());
                            break;
                        case 0xC8: // Repeat Start
                            repeatBegin = buffer.BaseStream.Position;
                            curTrack.AddEvent(new EvRepeatStart());
                            break;
                        case 0xC9: // Repeat End
                            int loopId = buffer.ReadByte();
                            if (!repeaterEndPositions.Contains(buffer.BaseStream.Position))
                            {
                                repeaterEndPositions.Add(buffer.BaseStream.Position);
                                repeatNumber = loopId;
                            }

                            if (repeatNumber >= 2 && repeatBegin != 0)
                            {
                                buffer.BaseStream.Position = repeatBegin;
                                repeatNumber--;
                            }
                            curTrack.AddEvent(new EvRepeatEnd(loopId));
                            break;
                        case 0xCA: // Repeat End
                            loopId = 2;
                            if (!repeaterEndPositions.Contains(buffer.BaseStream.Position))
                            {
                                repeaterEndPositions.Add(buffer.BaseStream.Position);
                                repeatNumber = loopId;
                            }
                            if (repeatNumber >= 2 && repeatBegin != 0)
                            {
                                buffer.BaseStream.Position = repeatBegin;
                                repeatNumber--;
                            }
                            curTrack.AddEvent(new EvRepeatEnd());
                            break;
                        case 0xCB: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xCC: // Slur On
                            curTrack.AddEvent(new EvSlurOn());
                            break;
                        case 0xCD: // Slur Off
                            curTrack.AddEvent(new EvSlurOff());
                            break;
                        case 0xCE: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xCF: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xD0: // Note Off
                            curTrack.AddEvent(new EvNoteOff(channel, prevKey, delta));
                            delta = 0;
                            playingNote = false;
                            break;
                        case 0xD1: // Desactivate Notes ?
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xD2: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE, buffer.ReadByte()));
                            break;
                        case 0xD3: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE, buffer.ReadByte()));
                            break;
                        case 0xD4: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xD5: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xD6: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xD7: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xD8: // Pitch Bend
                            curTrack.AddEvent(new EvPitchBend(channel, buffer.ReadSByte()));
                            break;
                        case 0xD9: // Pitch Bend Move
                            curTrack.AddEvent(new EvPitchBendMove(buffer.ReadByte()));
                            break;
                        case 0xDA: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE, buffer.ReadByte()));
                            break;
                        case 0xDB: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xDC: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE, buffer.ReadByte()));
                            break;
                        case 0xDD: // LFO Pitch Bend times
                            b = buffer.ReadBytes(2);
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xDE: // LFO Expression times
                            b = buffer.ReadBytes(2);
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xDF: // LFO Panpot times
                            b = buffer.ReadBytes(2);
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xE0: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xE1: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xE2: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xE3: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xE4: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xE5: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xE6: // LFO Expression times
                            b = buffer.ReadBytes(2);
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xE7: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xE8: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xE9: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xEA: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xEB: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xEC: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xED: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xEE: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xEF: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xFC: // Tie
                            duration = buffer.ReadByte();
                            delta += (byte)duration;
                            curTrack.AddEvent(new EvTieTime(duration));
                            break;
                        case 0xFD: // Rest
                            duration = buffer.ReadByte();
                            if (playingNote)
                            {
                                curTrack.AddEvent(new EvNoteOff(channel, prevKey, delta));
                                delta = 0;
                                playingNote = false;
                            }
                            delta += (ushort)duration;
                            curTrack.AddEvent(new EvRest(duration));
                            break;
                        case 0xFE: // Meta Event
                            byte Meta = buffer.ReadByte();
                            switch (Meta)
                            {
                                case 0x00: // Tempo
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvTempo(b[0], b[1], delta));
                                    delta = 0;
                                    break;
                                case 0x01: // Tempo Slide
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvTempoSlide());
                                    break;
                                case 0x02: // Reverb Level
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvReverbLevel(channel, b[0], b[1], delta));
                                    break;
                                case 0x03: // Reverb Fade
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvReverbFade(b[0], b[1]));
                                    break;
                                case 0x04: // Drum kit On
                                    channel = 10;
                                    curTrack.AddEvent(new EvDrumKitOn());
                                    break;
                                case 0x05: // Drum kit Off
                                    curTrack.AddEvent(new EvDrumKitOff());
                                    break;
                                case 0x06: // Perma Loop
                                    long dest = buffer.BaseStream.Position + buffer.ReadInt16();
                                    long loopLen = buffer.BaseStream.Position - beginOffset;
                                    //buffer.BaseStream.Position = dest;
                                    curTrack.AddEvent(new EvPermaLoop(cTrackId, dest));
                                    // -----------------------------------------------------------
                                    curTrack.AddEvent(new EvEndTrack(cTrackId));
                                    cTrackId++;
                                    if (cTrackId < tracks.Length)
                                    {
                                        curTrack = tracks[cTrackId];
                                    }
                                    delta = 0;

                                    break;
                                case 0x07: // Perma Loop break with conditional.
                                    byte cond = buffer.ReadByte();
                                    dest = buffer.BaseStream.Position + buffer.ReadInt16();
                                    loopLen = buffer.BaseStream.Position - beginOffset;

                                    //Debug.Log(string.Concat("Perma Loop break : ", buffer.BaseStream.Position, " -|- ", dest));
                                    if (!condLoops.Contains(dest))
                                    {
                                        condLoops.Add(dest);
                                        //buffer.BaseStream.Position = dest;
                                    }
                                    else
                                    {
                                        // skip
                                    }
                                    curTrack.AddEvent(new EvPermaLoopBreak(cTrackId, cond, dest));
                                    break;
                                case 0x09: // Repeat Break
                                    b = buffer.ReadBytes(3);
                                    curTrack.AddEvent(new EvRepeatEnd());
                                    break;
                                case 0x0E: // call subroutine
                                    curTrack.AddEvent(new EvUnknown(STATUS_BYTE, Meta));
                                    break;
                                case 0x0F: // return from subroutine
                                    curTrack.AddEvent(new EvUnknown(STATUS_BYTE, Meta));
                                    break;
                                case 0x10: // Unknown
                                    curTrack.AddEvent(new EvUnknown(STATUS_BYTE, Meta));
                                    break;
                                case 0x14: // Program Change
                                    //curTrack.AddEvent(new EvProgramChange(channel, (byte)(buffer.ReadByte() + instruments.Length)));
                                    curTrack.AddEvent(new EvProgramChange(channel, buffer.ReadByte(), delta));
                                    delta = 0;
                                    break;
                                case 0x15: // Time Signature
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvTimeSign(b[0], b[1]));
                                    break;
                                case 0x16: // Marker
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvMarker(b[0], b[1]));
                                    break;
                                case 0x1C: // Unknown
                                    curTrack.AddEvent(new EvUnknown(STATUS_BYTE, buffer.ReadByte()));
                                    break;
                                default:
                                    curTrack.AddEvent(new EvUnknown(STATUS_BYTE, Meta));
                                    break;
                            }
                            break;
                        case 0xFF: // End Track Padding
                            curTrack.AddEvent(new EvEndTrack(cTrackId, true));
                            break;
                        default:
                            Debug.Log("Unknonw instruction in " + name + " at " + buffer.BaseStream.Position + "  ->  " + (byte)STATUS_BYTE);
                            //curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                    }
                }
            }
        }

        private class AKAOTrack
        {
            private List<AKAOEvent> events;

            public AKAOTrack()
            {
                events = new List<AKAOEvent>();
            }

            public List<AKAOEvent> Events
            {
                get => events;
            }

            public void AddEvent(AKAOEvent ev)
            {
                if (events == null)
                {
                    events = new List<AKAOEvent>();
                }
                events.Add(ev);
            }
        }



        private class AKAOEvent
        {
            internal uint deltaTime = 0x00;
            internal byte midiStatusByte;
            internal byte? midiArg1;
            internal byte? midiArg2;
            internal byte[] tail;

            internal List<byte> GetMidiBytes()
            {
                List<byte> midiBytes = new List<byte>();
                VLQ time = new VLQ(deltaTime);
                midiBytes.AddRange(time.Bytes);
                midiBytes.Add(midiStatusByte);
                if (midiArg1 != null)
                {
                    midiBytes.Add((byte)midiArg1);
                }

                if (midiArg2 != null)
                {
                    midiBytes.Add((byte)midiArg2);
                }

                if (tail != null && tail.Length > 0)
                {
                    midiBytes.AddRange(tail);
                }

                if (midiStatusByte != 0)
                {
                    return midiBytes;
                }
                else
                {
                    return null;
                }
            }
        }




        #region MIDI Events
        private class EvTimeSign : AKAOEvent
        {
            private uint _num;
            private double _denom;
            private byte _clocks = 0x20;
            private byte _quart = 0x08;

            public EvTimeSign(uint num, uint denom)
            {
                _num = num;
                _denom = Math.Round(Math.Log((double)(denom / 0.69314718055994530941723212145818)));
                /*
                deltaTime = 0x00;
                midiStatusByte = 0xFF;
                midiArg1 = 0x58;
                midiArg2 = 0x04;
                tail = new byte[] { (byte)_num, (byte)_denom, _clocks, _quart };
                */

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvTimeSign : ", num, ", ", denom));
                }
            }
        }

        private class EvMarker : AKAOEvent
        {
            private byte v1;
            private byte v2;

            public EvMarker(byte v1, byte v2)
            {
                this.v1 = v1;
                this.v2 = v2;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvMarker : ", v1, ", ", v2));
                }
            }
        }
        private class EvVolume : AKAOEvent
        {
            private uint volume;

            public EvVolume(uint channel, uint volume, uint delta = 0x00)
            {
                this.volume = volume;
                double val = Math.Round(Math.Sqrt((volume / 127.0f)) * 127.0f);


                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x07;
                midiArg2 = (byte)val;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvVolume : ", volume, "   channel : ", channel, "    delta : ", delta));
                }
            }
        }
        private class EvPan : AKAOEvent
        {
            private int pan;

            public EvPan(uint channel, int pan, uint delta = 0x00)
            {
                this.pan = pan;

                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x0A;
                midiArg2 = (byte)pan;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvPan : ", pan, "   channel : ", channel, "    delta : ", delta));
                }
            }
        }

        private class EvProgramChange : AKAOEvent
        {
            /*
            General MIDI Sound Set Groupings: (all channels except 10)
            Prog #      Instrument Group        Prog #      Instrument Group
            1-8         Piano                   65-72       Reed
            9-16        Chromatic Percussion    73-80       Pipe
            17-24       Organ                   81-88       Synth Lead
            25-32       Guitar                  89-96       Synth Pad
            33-40       Bass                    97-104      Synth Effects
            41-48       Strings                 105-112     Ethnic
            49-56       Ensemble                113-120     Percussive
            57-64       Brass                   121-128     Sound Effects
            */
            public EvProgramChange(uint channel, byte articulationId, uint delta = 0x00)
            {
                deltaTime = delta;
                midiStatusByte = (byte)(0xC0 + channel);
                midiArg1 = (byte)articulationId;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("|| EvProgramChange : ", articulationId, "   channel : ", channel, "    delta : ", delta));
                }
            }
        }
        private class EvReverbOn : AKAOEvent
        {
            public EvReverbOn()
            {
                // Maybe we should send a midi event to set the current channel's reverb value to 127 (max value) or 40 (default value)
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvReverbOn"));
                }
            }
        }
        private class EvReverbOff : AKAOEvent
        {
            public EvReverbOff()
            {
                // Maybe we should send a midi event to set the current channel's reverb value to 0
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvReverbOff"));
                }
            }
        }

        private class EvReverbLevel : AKAOEvent
        {
            private byte _v1;
            private byte _v2;

            /*
             * Effect 1 (Reverb Send Level) (Controller Number 91)
             * Status           2nd bytes               3rd byte
             * BnH              5BH                     vvH
             * n = MIDI channel number: 0H–FH (ch.1–ch.16)
             * vv = Control value :     00H–7FH (0–127), Initial Value = 28H (40)
             * *    This message adjusts the Reverb Send Level of each Part.
             */

            public EvReverbLevel(uint channel, byte v1, byte v2, uint delta = 0x00)
            {
                _v1 = v1; // maybe the channel
                _v2 = v2; // maybe the reverb value



                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x5B;
                //midiArg2 = 0x28; // set to default value
                midiArg2 = _v2;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvReverbLevel : ", v1, ", ", v2, "   channel : ", channel, "    delta : ", delta));
                }
            }
        }

        private class EvTempo : AKAOEvent
        {
            private double tempo;

            public EvTempo(byte val1, byte val2, uint t)
            {
                tempo = ((val2 << 8) + val1) / 218.4555555555555555555555555;
                uint microSecs = (uint)Math.Round(60000000 / tempo);



                deltaTime = t;
                midiStatusByte = 0xFF;
                midiArg1 = 0x51;
                midiArg2 = 0x03;
                tail = new byte[] { (byte)((microSecs & 0xFF0000) >> 16), (byte)((microSecs & 0x00FF00) >> 8), (byte)(microSecs & 0x0000FF) };

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvTempo : ", microSecs, "    delta : ", t));
                }
            }
        }
        private class EvExpr : AKAOEvent
        {
            private uint _expression;

            public EvExpr(uint channel, uint expression, uint delta = 0x00)
            {
                _expression = expression;
                double val = Math.Round(Math.Sqrt((_expression / 127.0f)) * 127.0f);

                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x0B;
                midiArg2 = (byte)val;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvExpr : ", val, "   channel : ", channel, "    delta : ", delta));
                }
            }
        }
        private class EvNoteOn : AKAOEvent
        {
            //"9nH + 2 Bytes"; // 1001	MIDI channel [0 - 15]	Key Number [0 - 127]	Velocity [0 - 127]
            private uint key;
            /*
            0        1      64      127
            off ppp p pp mp mf f ff fff
            */
            private uint velocity;

            public EvNoteOn(uint channel, uint key, uint velocity, uint t = 0x00)
            {
                //Debug.Log("EvNoteOn : " + channel + " k : " + key +" vel : "+ velocity + "  t : " + t);
                this.key = key;
                this.velocity = velocity;

                deltaTime = t;
                midiStatusByte = (byte)(0x90 + channel);
                midiArg1 = (byte)key;
                midiArg2 = (byte)velocity;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvNoteOn : ", key, "   velocity : ", velocity, "   channel : ", channel, "    delta : ", t));
                }
            }
        }
        private class EvNoteOff : AKAOEvent
        {
            //"8nH + 2 Bytes"; // 1000	MIDI channel [0 - 15]	Key Number [0 - 127]	Velocity [0 - 127]
            private uint key;

            public EvNoteOff(uint channel, uint key, uint t)
            {
                //Debug.Log("EvNoteOff : "+channel+" k : "+key+"  t : "+t);
                this.key = key;

                deltaTime = t;
                midiStatusByte = (byte)(0x80 + channel);
                midiArg1 = (byte)key;
                midiArg2 = 0x40; // Standard velocity

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvNoteOff : ", key, "   channel : ", channel, "    delta : ", t));
                }
            }
        }

        private class EvPortamento : AKAOEvent
        {
            public EvPortamento(uint channel, byte duration, byte step)
            {
                deltaTime = 0x00;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x41;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvPortamento :    channel : ", channel, "  duration :", duration, "  step : ", step));
                }
            }
        }


        private class EvExprSlide : AKAOEvent
        {
            private uint duration;
            private uint expression;

            public EvExprSlide(uint duration, uint expression)
            {
                this.duration = duration;
                this.expression = expression;
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvExprSlide : ", duration, ", ", expression));
                }
            }
        }

        private class EvPitchBend : AKAOEvent
        {
            private byte _msb;
            private byte _lsb;

            public EvPitchBend(uint channel, sbyte value)
            {
                _msb = (byte)(0x40 + value);
                _lsb = 0x00;
                deltaTime = 0x00;
                midiStatusByte = (byte)(0xE0 + channel);
                midiArg1 = _lsb;
                midiArg2 = _msb;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvPitchBend : ", value));
                }
            }
        }
        #endregion







        private class EvUnknown : AKAOEvent
        {
            private byte v;

            public EvUnknown(byte value)
            {

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvUnknown : ", value));
                }
            }

            public EvUnknown(byte value, byte v)
            {
                this.v = v;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvUnknown : ", value, ", ", v));
                }
            }
        }

        private class EvPanSlide : AKAOEvent
        {
            private uint duration;
            private int pan;

            public EvPanSlide(uint channel, uint duration, int pan)
            {
                this.duration = duration;
                this.pan = pan;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvPanSlide : ", duration, ", ", pan));
                }
            }
        }

        private class EvAttack : AKAOEvent
        {
            private int attack;

            public EvAttack(int attack)
            {
                this.attack = attack;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvAttack : ", attack));
                }
            }
        }

        private class EvDecay : AKAOEvent
        {
            private int decay;

            public EvDecay(int decay)
            {
                this.decay = decay;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvDecay : ", decay));
                }
            }
        }

        private class EvSustain : AKAOEvent
        {
            private int sustain;

            public EvSustain(int sustain)
            {
                this.sustain = sustain;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvSustain : ", sustain));
                }
            }
        }

        private class EvSustainRelease : AKAOEvent
        {
            private uint duration;

            public EvSustainRelease(uint duration)
            {
                this.duration = duration;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvSustainRelease : ", duration));
                }
            }
        }

        private class EvRelease : AKAOEvent
        {
            private uint duration;

            public EvRelease(uint duration)
            {
                this.duration = duration;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvRelease : ", duration));
                }
            }
        }

        private class EvResetADSR : AKAOEvent
        {
            public EvResetADSR()
            {


                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvResetADSR "));
                }
            }
        }

        private class EvLFOPitchRange : AKAOEvent
        {
            private byte v1;
            private byte v2;
            private byte v3;

            public EvLFOPitchRange(byte v1, byte v2, byte v3)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvLFOPitchRange :  ", v1, ", ", v2, ", ", v3));
                }
            }
        }

        private class EvLFOPitchDepth : AKAOEvent
        {
            private int depth;

            public EvLFOPitchDepth(int depth)
            {
                this.depth = depth;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvLFOPitchDepth : ", depth));
                }
            }
        }

        private class EvLFOPitchOff : AKAOEvent
        {
            public EvLFOPitchOff()
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvLFOPitchOff : "));
                }
            }
        }

        private class EvLFOExprRange : AKAOEvent
        {
            private byte v1;
            private byte v2;
            private byte v3;

            public EvLFOExprRange(byte v1, byte v2, byte v3)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvLFOExprRange :  ", v1, ", ", v2, ", ", v3));
                }
            }
        }

        private class EvLFOExprDepth : AKAOEvent
        {
            private int depth;

            public EvLFOExprDepth(int depth)
            {
                this.depth = depth;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvLFOExprDepth : ", depth));
                }
            }
        }

        private class EvLFOExprOff : AKAOEvent
        {
            public EvLFOExprOff()
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvLFOExprOff : "));
                }
            }
        }

        private class EvLFOPanpotRange : AKAOEvent
        {
            private byte v1;
            private byte v2;

            public EvLFOPanpotRange(byte v1, byte v2)
            {
                this.v1 = v1;
                this.v2 = v2;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvLFOPanpotRange :  ", v1, ", ", v2));
                }
            }
        }

        private class EvLFOPanpotDepth : AKAOEvent
        {
            private int depth;

            public EvLFOPanpotDepth(int depth)
            {
                this.depth = depth;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvLFOPanpotDepth : ", depth));
                }
            }
        }

        private class EvLFOPanpotOff : AKAOEvent
        {

            public EvLFOPanpotOff()
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvLFOPanpotOff"));
                }
            }
        }

        private class EvTranspose : AKAOEvent
        {
            private int transpose;

            public EvTranspose(int transpose)
            {
                this.transpose = transpose;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvTranspose : ", transpose));
                }
            }
        }

        private class EvTransposeMove : AKAOEvent
        {
            private int transpose;

            public EvTransposeMove(int transpose)
            {
                this.transpose = transpose;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvTransposeMove : ", transpose));
                }
            }
        }


        private class EvNoiseOn : AKAOEvent
        {
            public EvNoiseOn()
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvNoiseOn"));
                }
            }
        }

        private class EvNoiseOff : AKAOEvent
        {
            public EvNoiseOff()
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvNoiseOff"));
                }
            }
        }

        private class EvFMOn : AKAOEvent
        {
            public EvFMOn()
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvFMOn"));
                }
            }
        }

        private class EvFMOff : AKAOEvent
        {
            public EvFMOff()
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvFMOff"));
                }
            }
        }

        private class EvSlurOn : AKAOEvent
        {
            public EvSlurOn()
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvSlurOn"));
                }
            }
        }

        private class EvSlurOff : AKAOEvent
        {
            public EvSlurOff()
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvSlurOff"));
                }
            }
        }

        private class EvPitchBendMove : AKAOEvent
        {
            private uint value;

            public EvPitchBendMove(uint value)
            {
                this.value = value;
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvPitchBendMove : ", value));
                }
            }
        }

        private class EvTempoSlide : AKAOEvent
        {
            public EvTempoSlide()
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvTempoSlide"));
                }
            }
        }


        private class EvReverbFade : AKAOEvent
        {
            private byte v1;
            private byte v2;

            public EvReverbFade(byte v1, byte v2)
            {
                this.v1 = v1;
                this.v2 = v2;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvReverbFade :  ", v1, ", ", v2));
                }
            }
        }





        #region Non MIDI EVENTS

        private class EvEndTrack : AKAOEvent
        {
            public EvEndTrack(uint trackId, bool bigEnd = false)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvEndTrack : " + trackId, "    EOF : ", bigEnd));
                    Debug.Log("|------------------------------------------------------------------------------------------------");
                }

            }
        }
        private class EvTieTime : AKAOEvent
        {
            private uint value;

            public EvTieTime(uint value)
            {
                this.value = value;
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log("EvTieTime : " + value);
                }
            }
        }
        private class EvRest : AKAOEvent
        {
            private uint duration;

            public EvRest(uint duration)
            {
                this.duration = duration;
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log("EvRest : " + duration);
                }
            }
        }
        private class EvOctave : AKAOEvent
        {
            private uint _octave;

            public EvOctave(uint octave)
            {
                _octave = octave;
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvOctave : ", octave));
                }
            }
        }

        private class EvOctaveUp : AKAOEvent
        {
            public EvOctaveUp()
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log("EvOctaveUp");
                }
            }
        }

        private class EvOctaveDown : AKAOEvent
        {
            public EvOctaveDown()
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log("EvOctaveDown");
                }
            }
        }


        private class EvRepeatStart : AKAOEvent
        {
            public EvRepeatStart()
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log("EvRepeatStart");
                }
            }
        }
        private class EvRepeatEnd : AKAOEvent
        {
            private int loopId;

            public EvRepeatEnd()
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log("EvRepeatEnd");
                }
            }

            public EvRepeatEnd(int loopId)
            {
                this.loopId = loopId;
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvRepeatEnd : ", loopId));
                }
            }
        }
        private class EvDrumKitOn : AKAOEvent
        {

            public EvDrumKitOn()
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning("######-------------------------------------------------###########");
                    Debug.LogWarning("EvDrumKitOn");
                    Debug.LogWarning("######-------------------------------------------------###########");
                }
            }
        }

        private class EvDrumKitOff : AKAOEvent
        {

            public EvDrumKitOff()
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning("######-------------------------------------------------###########");
                    Debug.LogWarning("EvDrumKitOff");
                    Debug.LogWarning("######-------------------------------------------------###########");
                }
            }
        }

        private class EvPermaLoopBreak : AKAOEvent
        {
            private uint cTrackId;
            private byte cond;
            private long dest;

            public EvPermaLoopBreak(uint cTrackId, byte cond, long dest)
            {
                this.cTrackId = cTrackId;
                this.cond = cond;
                this.dest = dest;


                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvPermaLoopBreak : ", dest, "   condition : ", cond));
                }
            }
        }

        private class EvPermaLoop : AKAOEvent
        {
            private uint cTrackId;
            private long dest;

            public EvPermaLoop(uint cTrackId, long dest)
            {
                this.cTrackId = cTrackId;
                this.dest = dest;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("EvPermaLoop : ", dest));
                }
            }
        }
        #endregion
    }

}
