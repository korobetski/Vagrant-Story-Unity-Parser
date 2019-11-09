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

        public static uint timeDebug = 0;

        public AKAOComposer(BinaryReader buffer, long start, long end, uint NI, uint NT, string name)
        {
            this.buffer = buffer;

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


            //Debug.Log("## TRACK : " + cTrackId + "   -----------------------------------------------------------------------");
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

                //Debug.Log(timeDebug+"    STATUS_BYTE : " + STATUS_BYTE);
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
                            timeDebug += delta;
                            curTrack.AddEvent(new EvNoteOff(channel, prevKey, delta));
                            delta = 0;
                            playingNote = false;
                        }

                        uint relativeKey = (uint)i;
                        uint baseKey = octave * 12;
                        uint key = baseKey + relativeKey;
                        timeDebug += delta;
                        curTrack.AddEvent(new EvNoteOn(channel, key, velocity, delta));
                        delta = delta_time_table[k];
                        prevKey = key;
                        playingNote = true;
                    }
                    else if (STATUS_BYTE < 0x8F) // Tie
                    {
                        uint duration = delta_time_table[k];
                        delta += (ushort)duration;
                        curTrack.AddEvent(new EvTieTime(duration));
                    }
                    else // Rest
                    {
                        if (playingNote)
                        {
                            timeDebug += delta;
                            curTrack.AddEvent(new EvNoteOff(channel, prevKey, delta));
                            delta = 0;
                            playingNote = false;
                        }

                        uint duration = delta_time_table[k];
                        delta += (ushort)duration;
                        curTrack.AddEvent(new EvTieTime(delta));
                    }
                }
                else if ((STATUS_BYTE >= 0xF0) && (STATUS_BYTE <= 0xFB)) // Alternate Note On ?
                {
                    if (playingNote)
                    {
                        timeDebug += delta;
                        curTrack.AddEvent(new EvNoteOff(channel, prevKey, delta));
                        delta = 0;
                        playingNote = false;
                    }
                    uint relativeKey = (uint)STATUS_BYTE - 0xF0;
                    uint baseKey = octave * 12;
                    uint key = baseKey + relativeKey;
                    uint time = buffer.ReadByte();
                    timeDebug += delta;
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
                            curTrack.AddEvent(new EvEndTrack());
                            timeDebug = 0;
                            delta = 0;
                            break;
                        case 0xA1:// Program Change
                            //articulations[articulationId]
                            timeDebug += delta;
                            curTrack.AddEvent(new EvProgramChange(channel, (byte)(buffer.ReadByte() + numInstr), delta));
                            delta = 0;
                            break;
                        case 0xA2: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xA3:// Volume
                            uint volume = buffer.ReadByte();
                            timeDebug += delta;
                            curTrack.AddEvent(new EvVolume(channel, volume, delta));
                            delta = 0;
                            break;
                        case 0xA4:// Portamento
                            curTrack.AddEvent(new EvPortamento(channel));
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
                            timeDebug += delta;
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
                            byte[] b = buffer.ReadBytes(3);
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
                            b = buffer.ReadBytes(3);
                            curTrack.AddEvent(new EvLFOPanpotRange(b[0], b[1], b[2]));
                            break;
                        case 0xBD: // LFO Panpot Depth
                            depth = buffer.ReadByte();
                            curTrack.AddEvent(new EvLFOPanpotDepth(depth));
                            break;
                        case 0xBE: // LFO Panpot Off
                            curTrack.AddEvent(new EvLFOPanpotOff());
                            break;
                        case 0xBF: // LFO Panpot ??
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
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
                            timeDebug += delta;
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
                            uint value = buffer.ReadByte();
                            uint fullValue = (uint)(value * 64.503937007874015748031496062992f);
                            fullValue += 0x2000;
                            uint high = fullValue & 0x7F;
                            uint low = (fullValue & 0x3F80) << 7;
                            curTrack.AddEvent(new EvPitchBend(channel, low, high));
                            break;
                        case 0xD9: // Pitch Bend Move
                            value = buffer.ReadByte();
                            curTrack.AddEvent(new EvPitchBendMove(value));
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
                            value = buffer.ReadByte();
                            delta += (ushort)value;
                            curTrack.AddEvent(new EvTieTime(value));
                            break;
                        case 0xFD: // Rest
                            duration = buffer.ReadByte();
                            if (playingNote)
                            {
                                timeDebug += delta;
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
                                    timeDebug += delta;
                                    curTrack.AddEvent(new EvTempo(b[0], b[1], delta));
                                    break;
                                case 0x01: // Tempo Slide
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvTempoSlide());
                                    break;
                                case 0x02: // Reverb Level
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvReverbLevel(b[0], b[1]));
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
                                case 0x06: // End Track
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvEndTrack());
                                    cTrackId++;
                                    if (cTrackId < tracks.Length)
                                    {
                                        curTrack = tracks[cTrackId];
                                    }
                                    timeDebug = 0;
                                    delta = 0;
                                    //Debug.Log("## TRACK : " + cTrackId + "   -----------------------------------------------------------------------");
                                    break;
                                case 0x07: // End Track
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvEndTrack());
                                    cTrackId++;
                                    if (cTrackId < tracks.Length)
                                    {
                                        curTrack = tracks[cTrackId];
                                    }
                                    timeDebug = 0;
                                    delta = 0;
                                    //Debug.Log("## TRACK : " + cTrackId + "   -----------------------------------------------------------------------");
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
                                    curTrack.AddEvent(new EvProgramChange(channel, buffer.ReadByte()));
                                    delta = 0;
                                    break;
                                case 0x15: // Time Signature
                                    uint num = buffer.ReadByte();
                                    uint denom = buffer.ReadByte();
                                    //curTrack.AddEvent(new EvTimeSign(num, denom));
                                    break;
                                case 0x16: // Maker
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvMaker(b[0], b[1]));
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
                            curTrack.AddEvent(new EvEndTrack());
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
                //Debug.Log("     AddEvent : " +ev);
                events.Add(ev);
            }
        }



        private class AKAOEvent
        {
            internal ushort deltaTime = 0x00;
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


        /*
         * Important Events to implement
         * EvTimeSign
         * EvMaker
         * EvVolume
         * EvPan
         * EvProgramChange
         * EvReverbOn
         * EvReverbOff
         * EvReverbLevel
         * EvTempo
         * EvExpr
         * EvNoteOn
         * EvNoteOff
         * EvRepeatStart
         * EvRepeatEnd
         * EvTie
         * EvEndTrack
         * EvPitchBend
         * EvLFOPanpotDepth
         * EvLFOPanpotRange
         * EvPortamento
         * EvRelease
         * EvDrumKitOn
         * EvAttack
         * EvSustainRelease
         * EvDecay
         * EvLFOPitchDepth
         * EvSlurOn
         * EvLFOExprOff
         * EvFMOn
         * EvLFOExprRange
         * EvDecay
         * EvSustain
         * EvNoiseOn
         * EvTransposeMove
        */

        private class EvTimeSign : AKAOEvent
        {
            private uint _num;
            private uint _denom;
            private byte clocks = 0x24;
            private byte quart = 0x08;

            public EvTimeSign(uint num, uint denom)
            {
                _num = num;
                _denom = denom;

                deltaTime = 0x00;
                midiStatusByte = 0xFF;
                midiArg1 = 0x58;
                midiArg2 = 0x04;
                tail = new byte[] { (byte)num, (byte)(denom / 0.69314718055994530941723212145818), clocks, quart };

            }
        }

        private class EvMaker : AKAOEvent
        {
            private byte v1;
            private byte v2;

            public EvMaker(byte v1, byte v2)
            {
                this.v1 = v1;
                this.v2 = v2;
            }
        }
        private class EvVolume : AKAOEvent
        {
            private uint volume;

            public EvVolume(uint channel, uint volume, ushort delta = 0x00)
            {
                this.volume = volume;
                double val = Math.Round(Math.Sqrt((volume / 127.0f)) * 127.0f);


                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x07;
                midiArg2 = (byte)val;
            }
        }
        private class EvPan : AKAOEvent
        {
            private int pan;

            public EvPan(uint channel, int pan, ushort delta = 0x00)
            {
                this.pan = pan;

                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x0A;
                midiArg2 = (byte)pan;
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
            public EvProgramChange(uint channel, byte articulationId, ushort delta = 0x00)
            {
                deltaTime = delta;
                midiStatusByte = (byte)(0xC0 + channel);
                //Debug.Log("EvProgramChange : art -> "+ articulationId);
                midiArg1 = (byte)articulationId;
            }
        }
        private class EvReverbOn : AKAOEvent
        {
            public EvReverbOn()
            {

            }
        }
        private class EvReverbLevel : AKAOEvent
        {
            private byte v1;
            private byte v2;

            public EvReverbLevel(byte v1, byte v2)
            {
                this.v1 = v1;
                this.v2 = v2;
            }
        }

        private class EvTempo : AKAOEvent
        {
            private long tempo;

            public EvTempo(byte val1, byte val2, ushort t)
            {
                tempo = (long)(((val2 << 8) + val1) / 218.4555555555555555555555555);
                uint microSecs = (UInt32)Math.Round((double)60000000 / tempo);

                deltaTime = t;
                midiStatusByte = 0xFF;
                midiArg1 = (byte)0x51;
                midiArg2 = (byte)0x03;
                tail = new byte[] { (byte)((microSecs & 0xFF0000) >> 16), (byte)((microSecs & 0x00FF00) >> 8), (byte)(microSecs & 0x0000FF) };

            }
        }
        private class EvExpr : AKAOEvent
        {
            private uint _expression;

            public EvExpr(uint channel, uint expression, ushort delta = 0x00)
            {
                _expression = expression;
                double val = Math.Round(Math.Sqrt((_expression / 127.0f)) * 127.0f);

                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x0B;
                midiArg2 = (byte)val;
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

            public EvNoteOn(uint channel, uint key, uint velocity, ushort t = 0x00)
            {
                //Debug.Log("EvNoteOn : " + channel + " k : " + key +" vel : "+ velocity + "  t : " + t);
                this.key = key;
                this.velocity = velocity;

                deltaTime = t;
                midiStatusByte = (byte)(0x90 + channel);
                midiArg1 = (byte)key;
                midiArg2 = (byte)velocity;
            }
        }
        private class EvNoteOff : AKAOEvent
        {
            //"8nH + 2 Bytes"; // 1000	MIDI channel [0 - 15]	Key Number [0 - 127]	Velocity [0 - 127]
            private uint key;

            public EvNoteOff(uint channel, uint key, ushort t)
            {
                //Debug.Log("EvNoteOff : "+channel+" k : "+key+"  t : "+t);
                this.key = key;

                deltaTime = t;
                midiStatusByte = (byte)(0x80 + channel);
                midiArg1 = (byte)key;
                midiArg2 = 0x40; // Standard velocity
            }
        }


        private class EvRepeatStart : AKAOEvent
        {
            public EvRepeatStart()
            {

            }
        }
        private class EvRepeatEnd : AKAOEvent
        {
            private int loopId;

            public EvRepeatEnd()
            {

            }

            public EvRepeatEnd(int loopId)
            {
                this.loopId = loopId;

            }
        }
        private class EvEndTrack : AKAOEvent
        {
            public EvEndTrack()
            {

            }
        }








        private class EvUnknown : AKAOEvent
        {
            private byte v;

            public EvUnknown(byte value)
            {

            }

            public EvUnknown(byte value, byte v)
            {
                this.v = v;
            }
        }

        private class EvPortamento : AKAOEvent
        {
            public EvPortamento(uint channel)
            {
                deltaTime = 0x00;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x41;
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
            }
        }

        private class EvAttack : AKAOEvent
        {
            private int attack;

            public EvAttack(int attack)
            {
                this.attack = attack;
            }
        }

        private class EvDecay : AKAOEvent
        {
            private int decay;

            public EvDecay(int decay)
            {
                this.decay = decay;
            }
        }

        private class EvSustain : AKAOEvent
        {
            private int sustain;

            public EvSustain(int sustain)
            {
                this.sustain = sustain;
            }
        }

        private class EvSustainRelease : AKAOEvent
        {
            private uint duration;

            public EvSustainRelease(uint duration)
            {
                this.duration = duration;
            }
        }

        private class EvRelease : AKAOEvent
        {
            private uint duration;

            public EvRelease(uint duration)
            {
                this.duration = duration;
            }
        }

        private class EvResetADSR : AKAOEvent
        {
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
            }
        }

        private class EvLFOPitchDepth : AKAOEvent
        {
            private int depth;

            public EvLFOPitchDepth(int depth)
            {
                this.depth = depth;
            }
        }

        private class EvLFOPitchOff : AKAOEvent
        {
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
            }
        }

        private class EvLFOExprDepth : AKAOEvent
        {
            private int depth;

            public EvLFOExprDepth(int depth)
            {
                this.depth = depth;
            }
        }

        private class EvLFOExprOff : AKAOEvent
        {
        }

        private class EvLFOPanpotRange : AKAOEvent
        {
            private byte v1;
            private byte v2;
            private byte v3;

            public EvLFOPanpotRange(byte v1, byte v2, byte v3)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;
            }
        }

        private class EvLFOPanpotDepth : AKAOEvent
        {
            private int depth;

            public EvLFOPanpotDepth(int depth)
            {
                this.depth = depth;
            }
        }

        private class EvLFOPanpotOff : AKAOEvent
        {
        }

        private class EvTranspose : AKAOEvent
        {
            private int transpose;

            public EvTranspose(int transpose)
            {
                this.transpose = transpose;
            }
        }

        private class EvTransposeMove : AKAOEvent
        {
            private int transpose;

            public EvTransposeMove(int transpose)
            {
                this.transpose = transpose;
            }
        }

        private class EvReverbOff : AKAOEvent
        {
        }

        private class EvNoiseOn : AKAOEvent
        {
        }

        private class EvNoiseOff : AKAOEvent
        {
        }

        private class EvFMOn : AKAOEvent
        {
        }

        private class EvFMOff : AKAOEvent
        {
        }

        private class EvSlurOn : AKAOEvent
        {
        }

        private class EvSlurOff : AKAOEvent
        {
        }

        private class EvPitchBend : AKAOEvent
        {
            private uint low;
            private uint high;

            public EvPitchBend(uint channel, uint low, uint high)
            {
                this.low = low;
                this.high = high;
                /*
                deltaTime = 0x00;
                midiStatusByte = (byte)(0xE0 + channel);
                midiArg1 = (byte)low;
                midiArg2 = (byte)high;
                */
            }
        }

        private class EvPitchBendMove : AKAOEvent
        {
            private uint value;

            public EvPitchBendMove(uint value)
            {
                this.value = value;
            }
        }

        private class EvTieTime : AKAOEvent
        {
            private uint value;

            public EvTieTime(uint value)
            {
                //Debug.Log("EvTieTime : "+value);
                this.value = value;
            }
        }

        private class EvTempoSlide : AKAOEvent
        {
        }


        private class EvReverbFade : AKAOEvent
        {
            private byte v1;
            private byte v2;

            public EvReverbFade(byte v1, byte v2)
            {
                this.v1 = v1;
                this.v2 = v2;
            }
        }

        private class EvDrumKitOn : AKAOEvent
        {
        }

        private class EvDrumKitOff : AKAOEvent
        {
        }

        private class EvOctave : AKAOEvent
        {
            private uint octave;

            public EvOctave(uint octave)
            {
                this.octave = octave;
            }
        }

        private class EvOctaveUp : AKAOEvent
        {
        }

        private class EvOctaveDown : AKAOEvent
        {
        }

        private class EvRest : AKAOEvent
        {
            private uint duration;

            public EvRest(uint duration)
            {
                this.duration = duration;
            }
        }
    }

}
