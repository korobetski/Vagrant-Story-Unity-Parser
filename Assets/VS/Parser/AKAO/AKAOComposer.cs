using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        //public static readonly ushort[] delta_time_table = { 0xC0, 0x60, 0x30, 0x18, 0x0C, 0x6, 0x3, 0x20, 0x10, 0x8, 0x4, 0x0, 0xA0A0, 0xA0A0 };
        public static readonly ushort[] delta_time_table = { 0xC0, 0x60, 0x30, 0x18, 0x0C, 0x6, 0x3, 0x20, 0x10, 0x8, 0x5, 0x0, 0xA0A0, 0xA0A0 };

        private BinaryReader buffer;
        private string name;
        private long start;
        private long end;
        private uint numTrack;
        private uint numInstr;
        private AKAOTrack[] tracks;

        List<uint> progIDs;


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
                if (track != null)
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

            int repeatIndex = -1;
            int repeatNumber = 0;
            List<long> repeaterStartPositions = new List<long>();
            List<long> repeaterEndPositions = new List<long>();
            List<long> condLoops = new List<long>();

            progIDs = new List<uint>();

            bool DrumOn = false;

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
                }
                else
                {
                    curTrack = tracks[tracks.Length - 1]; // using the last track instead
                }

                if (!DrumOn)
                {
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
                    channel = 9;
                }


                byte STATUS_BYTE = buffer.ReadByte();
                int i, k;

                if (STATUS_BYTE <= 0x9A)
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
                            curTrack.AddEvent(new EvNoteOff(STATUS_BYTE, channel, prevKey, delta));
                            delta = 0;
                            playingNote = false;
                        }
                        

                        uint relativeKey = (uint)i;
                        uint baseKey = octave * 12;
                        uint key = baseKey + relativeKey;
                        curTrack.AddEvent(new EvNoteOn(STATUS_BYTE, channel, key, velocity, delta));
                        delta = delta_time_table[k];
                        prevKey = key;
                        playingNote = true;
                    }
                    else if (STATUS_BYTE < 0x8F) // Tie
                    {
                        ushort duration = delta_time_table[k];
                        delta += duration;
                        curTrack.AddEvent(new EvTieTime(STATUS_BYTE, duration));
                    }
                    else // Rest
                    {
                        if (playingNote == true)
                        {
                            curTrack.AddEvent(new EvNoteOff(STATUS_BYTE, channel, prevKey, delta));
                            delta = 0;
                            playingNote = false;
                        }

                        ushort duration = delta_time_table[k];
                        delta += (ushort)duration;
                        curTrack.AddEvent(new EvRest(STATUS_BYTE, delta));
                    }
                }
                else if ((STATUS_BYTE >= 0xF0) && (STATUS_BYTE <= 0xFB)) // Alternate Note On ?
                {
                    
                    if (playingNote)
                    {
                        curTrack.AddEvent(new EvNoteOff(STATUS_BYTE, channel, prevKey, delta));
                        delta = 0;
                        playingNote = false;
                    }
                    

                    uint relativeKey = (uint)STATUS_BYTE - 0xF0;
                    uint baseKey = octave * 12;
                    uint key = baseKey + relativeKey;
                    uint time = buffer.ReadByte();

                    curTrack.AddEvent(new EvNoteOn(STATUS_BYTE, channel, key, velocity, delta));
                    delta = (ushort)time;
                    prevKey = key;
                    playingNote = true;
                }
                else
                {
                    switch (STATUS_BYTE)
                    {
                        case 0xA0: // End Track ??

                            if (playingNote)
                            {
                                curTrack.AddEvent(new EvNoteOff(STATUS_BYTE, channel, prevKey, delta));
                                delta = 0;
                                playingNote = false;
                            }
                            // We don't want an empty track
                            if (curTrack.Events.Count > 3)
                            {
                                curTrack.AddEvent(new EvEndTrack(STATUS_BYTE, cTrackId, delta));
                                cTrackId++;
                                if (cTrackId < tracks.Length)
                                {
                                    curTrack = tracks[cTrackId];
                                }
                                delta = 0;
                            }

                            break;
                        case 0xA1:// Program Change, using General Midi fallback instead of samples ?
                            byte prog = buffer.ReadByte();
                            if (!progIDs.Contains(prog)) progIDs.Add(prog);
                            curTrack.AddEvent(new EvBank(STATUS_BYTE, channel, 0, delta));
                            curTrack.AddEvent(new EvProgramChange(STATUS_BYTE, channel, (byte)(prog + numInstr))); //  + numInstr
                            if (curTrack.name == "AKAOTrack")
                            {
                                curTrack.name = string.Concat("Track ", SMF.INSTRUMENTS[prog]);
                                curTrack.AddEvent(new EvTrackName(curTrack.name));
                            }
                            delta = 0;
                            break;
                        case 0xA2: // Pause ?
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE, buffer.ReadByte()));
                            break;
                        case 0xA3:// Volume
                            uint volume = buffer.ReadByte();
                            double val = Math.Round(Math.Sqrt((volume / 127.0f)) * 127.0f);
                            velocity = (byte)val;
                            curTrack.AddEvent(new EvVolume(STATUS_BYTE, channel, velocity, delta));
                            delta = 0;
                            break;
                        case 0xA4:// Portamento
                            byte[] b = buffer.ReadBytes(2);
                            curTrack.AddEvent(new EvPortamento(STATUS_BYTE, channel, b[0], b[1]));
                            break;
                        case 0xA5:// Octave
                            octave = buffer.ReadByte();
                            curTrack.AddEvent(new EvOctave(STATUS_BYTE, octave));
                            break;
                        case 0xA6:// Octave ++
                            octave++;
                            curTrack.AddEvent(new EvOctaveUp(STATUS_BYTE));
                            break;
                        case 0xA7:// Octave --
                            octave--;
                            curTrack.AddEvent(new EvOctaveDown(STATUS_BYTE));
                            break;
                        case 0xA8:// Expression
                            uint expression = buffer.ReadByte();
                            curTrack.AddEvent(new EvExpr(STATUS_BYTE, channel, expression));
                            break;
                        case 0xA9:// Expression Slide
                            uint duration = buffer.ReadByte();
                            expression = buffer.ReadByte();
                            curTrack.AddEvent(new EvExprSlide(STATUS_BYTE, duration, expression));
                            break;
                        case 0xAA:// Pan
                            int pan = buffer.ReadByte();
                            curTrack.AddEvent(new EvPan(STATUS_BYTE, channel, pan, delta));
                            delta = 0;
                            break;
                        case 0xAB:// Pan Fade
                            duration = buffer.ReadByte();
                            pan = buffer.ReadByte();
                            curTrack.AddEvent(new EvPanSlide(STATUS_BYTE, channel, duration, pan));
                            break;
                        case 0xAC: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE, buffer.ReadByte()));
                            break;
                        case 0xAD: // Attack
                            int attack = buffer.ReadByte();
                            curTrack.AddEvent(new EvAttack(STATUS_BYTE, attack));
                            break;
                        case 0xAE: // Decay
                            int decay = buffer.ReadByte();
                            curTrack.AddEvent(new EvDecay(STATUS_BYTE, decay));
                            break;
                        case 0xAF: // Sustain
                            int sustain = buffer.ReadByte();
                            curTrack.AddEvent(new EvSustain(STATUS_BYTE, sustain));
                            break;
                        case 0xB0: // Decay + Sustain
                            decay = buffer.ReadByte();
                            sustain = buffer.ReadByte();
                            curTrack.AddEvent(new EvDecay(STATUS_BYTE, decay));
                            curTrack.AddEvent(new EvSustain(STATUS_BYTE, sustain));
                            break;
                        case 0xB1: // Sustain release
                            duration = buffer.ReadByte();
                            curTrack.AddEvent(new EvSustainRelease(STATUS_BYTE, duration));
                            break;
                        case 0xB2: // Release
                            duration = buffer.ReadByte();
                            curTrack.AddEvent(new EvRelease(STATUS_BYTE, duration));
                            break;
                        case 0xB3: // Reset ADSR (Attack-Decay-Sustain-Release)
                            curTrack.AddEvent(new EvResetADSR(STATUS_BYTE));
                            break;
                        // LFO (low-frequency oscillators) Pitch bend
                        case 0xB4: // LFO Pitch bend Range
                            b = buffer.ReadBytes(3);
                            curTrack.AddEvent(new EvLFOPitchRange(STATUS_BYTE, b[0], b[1], b[2]));
                            break;
                        case 0xB5: // LFO Pitch bend Depth
                            int depth = buffer.ReadByte();
                            curTrack.AddEvent(new EvLFOPitchDepth(STATUS_BYTE, depth));
                            break;
                        case 0xB6: // LFO Pitch bend Off
                            curTrack.AddEvent(new EvLFOPitchOff(STATUS_BYTE));
                            break;
                        case 0xB7: // LFO Pitch bend ??
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE, buffer.ReadByte()));
                            break;
                        // LFO (low-frequency oscillators) Expression
                        case 0xB8: // LFO Expression Range
                            b = buffer.ReadBytes(3);
                            curTrack.AddEvent(new EvLFOExprRange(STATUS_BYTE, b[0], b[1], b[2]));
                            break;
                        case 0xB9: // LFO Expression Depth
                            depth = buffer.ReadByte();
                            curTrack.AddEvent(new EvLFOExprDepth(STATUS_BYTE, depth));
                            break;
                        case 0xBA: // LFO Expression Off
                            curTrack.AddEvent(new EvLFOExprOff(STATUS_BYTE));
                            break;
                        case 0xBB: // LFO Expression ??
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE, buffer.ReadByte()));
                            break;
                        // LFO (low-frequency oscillators) Panpot
                        case 0xBC: // LFO Panpot Range
                            b = buffer.ReadBytes(2);
                            curTrack.AddEvent(new EvLFOPanpotRange(STATUS_BYTE, b[0], b[1]));
                            break;
                        case 0xBD: // LFO Panpot Depth
                            depth = buffer.ReadByte();
                            curTrack.AddEvent(new EvLFOPanpotDepth(STATUS_BYTE, depth));
                            break;
                        case 0xBE: // LFO Panpot Off
                            curTrack.AddEvent(new EvLFOPanpotOff(STATUS_BYTE));
                            break;
                        case 0xBF: // LFO Panpot ??
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE, buffer.ReadByte()));
                            break;
                        case 0xC0: // Transpose
                            sbyte transpose = buffer.ReadSByte();
                            curTrack.AddEvent(new EvTranspose(STATUS_BYTE, transpose));
                            break;
                        case 0xC1: // Transpose Move
                            byte trans = buffer.ReadByte();
                            curTrack.AddEvent(new EvTransposeMove(STATUS_BYTE, trans));
                            break;

                        case 0xC2: // Reverb On
                            curTrack.AddEvent(new EvReverbOn(STATUS_BYTE, channel, delta));
                            delta = 0;
                            break;
                        case 0xC3: // Reverb Off
                            curTrack.AddEvent(new EvReverbOff(STATUS_BYTE, channel, delta));
                            delta = 0;
                            break;

                        case 0xC4: // Noise On
                            curTrack.AddEvent(new EvNoiseOn(STATUS_BYTE));
                            break;
                        case 0xC5: // Noise Off
                            curTrack.AddEvent(new EvNoiseOff(STATUS_BYTE));
                            break;

                        case 0xC6: // FM (Frequency Modulation) On
                            curTrack.AddEvent(new EvFMOn(STATUS_BYTE));
                            break;
                        case 0xC7: // FM (Frequency Modulation) Off
                            curTrack.AddEvent(new EvFMOff(STATUS_BYTE));
                            break;

                        case 0xC8: // Repeat Start
                            //repeatBegin = buffer.BaseStream.Position;
                            repeaterStartPositions.Add(buffer.BaseStream.Position);
                            repeatIndex++;
                            curTrack.AddEvent(new EvRepeatStart(STATUS_BYTE));
                            break;
                        case 0xC9: // Repeat End
                            int loopId = buffer.ReadByte();

                            if (!repeaterEndPositions.Contains(buffer.BaseStream.Position))
                            {
                                repeaterEndPositions.Add(buffer.BaseStream.Position);
                                repeatNumber = loopId;
                            }

                            if (repeatNumber >= 2 && repeaterStartPositions.Count > repeatIndex)
                            {
                                buffer.BaseStream.Position = repeaterStartPositions[repeatIndex];
                                repeatNumber--;
                            }
                            else
                            {
                                if (repeatIndex > 0) // We want to keep the level 0 repeat start
                                {
                                    repeaterStartPositions.RemoveAt(repeatIndex);
                                    repeatIndex--;
                                }

                                if (UseDebug)
                                {
                                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  #######  Repeats Ends"));
                                }
                            }
                            curTrack.AddEvent(new EvRepeatEnd(STATUS_BYTE, repeatIndex, loopId));
                            break;
                        case 0xCA: // Repeat End
                            loopId = 2;
                            if (!repeaterEndPositions.Contains(buffer.BaseStream.Position))
                            {
                                repeaterEndPositions.Add(buffer.BaseStream.Position);
                                repeatNumber = loopId;
                            }

                            if (repeatNumber >= 2 && repeaterStartPositions.Count > repeatIndex)
                            {
                                buffer.BaseStream.Position = repeaterStartPositions[repeatIndex];
                                repeatNumber--;
                            }
                            else
                            {
                                if (repeatIndex > 0) // We want to keep the level 0 repeat start
                                {
                                    repeaterStartPositions.RemoveAt(repeatIndex);
                                    repeatIndex--;
                                }
                                if (UseDebug)
                                {
                                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  #######  Repeats Ends"));
                                }
                            }
                            curTrack.AddEvent(new EvRepeatEnd(STATUS_BYTE, repeatIndex));
                            break;
                        case 0xCB: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xCC: // Slur On
                            curTrack.AddEvent(new EvSlurOn(STATUS_BYTE));
                            break;
                        case 0xCD: // Slur Off
                            curTrack.AddEvent(new EvSlurOff(STATUS_BYTE));
                            break;
                        case 0xCE: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xCF: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xD0: // Note Off
                            curTrack.AddEvent(new EvNoteOff(STATUS_BYTE, channel, prevKey, delta));
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
                            curTrack.AddEvent(new EvPitchBend(STATUS_BYTE, channel, buffer.ReadSByte()));
                            break;
                        case 0xD9: // Pitch Bend Move
                            curTrack.AddEvent(new EvPitchBendMove(STATUS_BYTE, buffer.ReadByte()));
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
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE, buffer.ReadByte()));
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
                        case 0xFC: // Tie ??
                            duration = buffer.ReadByte();
                            delta += (byte)duration;
                            curTrack.AddEvent(new EvTieTime(STATUS_BYTE, duration));
                            break;
                        case 0xFD: // Rest ??
                            duration = buffer.ReadByte();
                            if (playingNote)
                            {
                                curTrack.AddEvent(new EvNoteOff(STATUS_BYTE, channel, prevKey, delta));
                                delta = 0;
                                playingNote = false;
                            }
                            delta += (ushort)duration;
                            curTrack.AddEvent(new EvRest(STATUS_BYTE, duration));
                            break;
                        case 0xFE: // Meta Event
                            byte Meta = buffer.ReadByte();
                            switch (Meta)
                            {
                                case 0x00: // Tempo
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvTempo(STATUS_BYTE, b[0], b[1], delta));
                                    delta = 0;
                                    break;
                                case 0x01: // Tempo Slide
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvTempoSlide(STATUS_BYTE, b[0], b[1], delta));
                                    break;
                                case 0x02: // Reverb Level
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvReverbLevel(STATUS_BYTE, channel, b[0], b[1], delta));
                                    break;
                                case 0x03: // Reverb Fade
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvReverbFade(STATUS_BYTE, b[0], b[1]));
                                    break;
                                case 0x04: // Drum kit On
                                    DrumOn = true;
                                    curTrack.AddEvent(new EvDrumKitOn(STATUS_BYTE));
                                    break;
                                case 0x05: // Drum kit Off
                                    DrumOn = false;
                                    curTrack.AddEvent(new EvDrumKitOff(STATUS_BYTE));
                                    break;
                                case 0x06: // Perma Loop
                                    long dest = buffer.BaseStream.Position + buffer.ReadInt16();
                                    long loopLen = buffer.BaseStream.Position - beginOffset;
                                    curTrack.AddEvent(new EvPermaLoop(STATUS_BYTE, cTrackId, dest));

                                    /*
                                    if (condLoops.Contains(dest) == false)
                                    {
                                        condLoops.Add(dest);
                                        repeaterEndPositions = new List<long>();
                                        buffer.BaseStream.Position = dest;
                                    }
                                    else
                                    {
                                        // -----------------------------------------------------------
                                        curTrack.AddEvent(new EvEndTrack(STATUS_BYTE, cTrackId, delta));
                                        cTrackId++;
                                        if (cTrackId < tracks.Length)
                                        {
                                            curTrack = tracks[cTrackId];
                                        }
                                        delta = 0;
                                    }
                                    */

                                    // -----------------------------------------------------------

                                    if (playingNote)
                                    {
                                        curTrack.AddEvent(new EvNoteOff(STATUS_BYTE, channel, prevKey, delta));
                                        delta = 0;
                                        playingNote = false;
                                    }
                                    curTrack.AddEvent(new EvEndTrack(STATUS_BYTE, cTrackId, delta));
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

                                    if (UseDebug)
                                    {
                                        Debug.LogWarning(string.Concat("Perma Loop break : ", buffer.BaseStream.Position, "   -|-   ", dest, "  cond : ", cond));
                                    }
                                    curTrack.AddEvent(new EvPermaLoopBreak(STATUS_BYTE, cTrackId, cond, dest));
                                    /*
                                    if (dest < buffer.BaseStream.Position)
                                    {
                                        // we loop one time
                                        if (condLoops.Contains(dest) == false)
                                        {
                                            condLoops.Add(dest);
                                            buffer.BaseStream.Position = dest;
                                        }
                                        else
                                        {

                                        }
                                    }
                                    else
                                    {

                                    }
                                    */
                                    break;
                                case 0x09: // Repeat Break
                                    b = buffer.ReadBytes(3);
                                    curTrack.AddEvent(new EvRepeatBreak(STATUS_BYTE, b));
                                    break;
                                case 0x0E: // call subroutine  Recurent in MUSIC034
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvUnknown(STATUS_BYTE, b));
                                    break;
                                case 0x0F: // return from subroutine
                                    curTrack.AddEvent(new EvUnknown(STATUS_BYTE, Meta));
                                    break;
                                case 0x10: // Unknown
                                    curTrack.AddEvent(new EvUnknown(STATUS_BYTE, Meta));
                                    break;
                                case 0x14: // Bank Change
                                    byte bank = buffer.ReadByte();
                                    if (!progIDs.Contains(bank)) progIDs.Add(bank);
                                    curTrack.AddEvent(new EvBank(STATUS_BYTE, channel, 1, delta));
                                    curTrack.AddEvent(new EvProgramChange(STATUS_BYTE, channel, bank));
                                    if (curTrack.name == "AKAOTrack")
                                    {
                                        curTrack.name = string.Concat("Track ", SMF.INSTRUMENTS[bank]);
                                        curTrack.AddEvent(new EvTrackName(curTrack.name));
                                    }
                                    delta = 0;
                                    break;
                                case 0x15: // Time Signature
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvTimeSign(STATUS_BYTE, b[0], b[1]));
                                    break;
                                case 0x16: // Marker
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvMarker(STATUS_BYTE, b[0], b[1]));
                                    break;
                                case 0x1C: // Unknown
                                    curTrack.AddEvent(new EvUnknown(STATUS_BYTE, buffer.ReadByte()));
                                    break;
                                case 0x1F: // Unknown Recurent in MUSIC020
                                    curTrack.AddEvent(new EvUnknown(STATUS_BYTE, Meta));
                                    break;
                                default:
                                    curTrack.AddEvent(new EvUnknown(STATUS_BYTE, Meta));
                                    break;
                            }
                            break;
                        case 0xFF: // End Track Padding
                            //curTrack.AddEvent(new EvEndTrack(cTrackId, delta, true));
                            break;
                        default:
                            Debug.LogWarning("Unknonw instruction in " + name + " at " + buffer.BaseStream.Position + "  ->  " + (byte)STATUS_BYTE);
                            //curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                    }
                }
            }
        }

        private class AKAOTrack
        {
            public string name = "AKAOTrack";
            public List<byte> programs;
            public bool drumTrack = false;
            public byte trackPan = 64;

            private List<AKAOEvent> events;

            public AKAOTrack()
            {
                events = new List<AKAOEvent>();
                programs = new List<byte>();
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

                if (events.Count > 0)
                {
                    ev.absTime = events[events.Count - 1].absTime + ev.deltaTime;
                } else
                {
                    ev.absTime = ev.deltaTime;
                }

                if (ev.GetType() == typeof(EvPan))
                {
                    trackPan = (byte)ev.midiArg2;
                }
                if (ev.GetType() == typeof(EvProgramChange))
                {
                    programs.Add((byte)ev.midiArg1);
                }
                if (ev.GetType() == typeof(EvBank))
                {
                    programs.Add((byte)ev.midiArg2);
                }
                if (ev.GetType() == typeof(EvDrumKitOn))
                {
                    drumTrack = true;
                }
                events.Add(ev);
            }
        }



        private class AKAOEvent
        {
            internal uint deltaTime = 0x00;
            internal uint absTime = 0x00;
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

            public static int CompareByAbsTime(AKAOEvent x, AKAOEvent y)
            {
                return x.absTime.CompareTo(y.absTime);
            }
        }



        #region MIDI Events
        private class EvTimeSign : AKAOEvent
        {
            private uint _num;
            private double _denom;
            private byte _clocks;
            private byte _quart;

            public EvTimeSign(byte STATUS_BYTE, uint num, uint denom)
            {
                _num = num;
                _denom = Math.Round(Math.Log((double)(denom / 0.69314718055994530941723212145818)));
                _clocks = 0x20;
                _quart = 0x08;
                /*
                deltaTime = 0x00;
                midiStatusByte = 0xFF;
                midiArg1 = 0x58;
                midiArg2 = 0x04;
                tail = new byte[] { (byte)_num, (byte)_denom, _clocks, _quart };
                */

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvTimeSign : ", num, ", ", denom));
                }
            }
        }

        private class EvMarker : AKAOEvent
        {
            private byte v1;
            private byte v2;

            public EvMarker(byte STATUS_BYTE, byte v1, byte v2)
            {
                this.v1 = v1;
                this.v2 = v2;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvMarker : ", v1, ", ", v2));
                }
            }
        }
        private class EvVolume : AKAOEvent
        {
            private uint volume;

            public EvVolume(byte STATUS_BYTE, uint channel, uint volume, uint delta = 0x00)
            {
                this.volume = volume;

                /*
                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x07;
                midiArg2 = (byte)volume;
                */
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvVolume : ", volume, "   channel : ", channel, "    delta : ", delta));
                }
            }
        }
        private class EvPan : AKAOEvent
        {
            private int pan;

            public EvPan(byte STATUS_BYTE, uint channel, int pan, uint delta = 0x00)
            {
                this.pan = pan;

                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x0A;
                midiArg2 = (byte)pan;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvPan : ", pan, "   channel : ", channel, "    delta : ", delta));
                }
            }
        }


        private class EvBank : AKAOEvent
        {

            public EvBank(byte STATUS_BYTE, uint channel, byte bank, ushort delta = 0x00)
            {
                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = (byte)0x00; // 0x00 MSB (Coarse) Bank select | 0x20 LSB (Fine) Bank select
                midiArg2 = bank;
            }
        }

        private class EvProgramChange : AKAOEvent
        {
            /*
             * http://midi.teragonaudio.com/tutr/rolmidi.htm
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
            public EvProgramChange(byte STATUS_BYTE, uint channel, byte prog, uint delta = 0x00)
            {
                deltaTime = delta;
                midiStatusByte = (byte)(0xC0 + channel);
                midiArg1 = (byte)prog;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogError(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvProgramChange : ", prog, "  ", SMF.INSTRUMENTS[prog], "   channel : ", channel, "    delta : ", delta));
                }
            }
        }
        private class EvReverbOn : AKAOEvent
        {
            public EvReverbOn(byte STATUS_BYTE, uint channel, uint delta = 0x00)
            {
                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x5B;
                midiArg2 = 0x40; // 127(max value) or 40(default value)
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvReverbOn"));
                }
            }
        }
        private class EvReverbOff : AKAOEvent
        {
            public EvReverbOff(byte STATUS_BYTE, uint channel, uint delta = 0x00)
            {
                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x5B;
                midiArg2 = 0; // reset reverb to 0
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvReverbOff"));
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

            public EvReverbLevel(byte STATUS_BYTE, uint channel, byte v1, byte v2, uint delta = 0x00)
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
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvReverbLevel : ", v1, ", ", v2, "   channel : ", channel, "    delta : ", delta));
                }
            }
        }

        private class EvTempo : AKAOEvent
        {
            private double tempo;

            public EvTempo(byte STATUS_BYTE, byte val1, byte val2, uint t)
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
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvTempo : ", microSecs, "    delta : ", t));
                }
            }
        }
        private class EvExpr : AKAOEvent
        {
            private uint _expression;

            public EvExpr(byte STATUS_BYTE, uint channel, uint expression, uint delta = 0x00)
            {
                _expression = expression;
                double val = Math.Round(Math.Sqrt((_expression / 127.0f)) * 127.0f);

                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x0B;
                midiArg2 = (byte)val;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvExpr : ", val, "   channel : ", channel, "    delta : ", delta));
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

            public EvNoteOn(byte STATUS_BYTE, uint channel, uint key, uint velocity, uint t = 0x00)
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
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvNoteOn : ", key, "   velocity : ", velocity, "   channel : ", channel, "    delta : ", t));
                }
            }
        }
        private class EvNoteOff : AKAOEvent
        {
            //"8nH + 2 Bytes"; // 1000	MIDI channel [0 - 15]	Key Number [0 - 127]	Velocity [0 - 127]
            private uint key;

            public EvNoteOff(byte STATUS_BYTE, uint channel, uint key, uint t)
            {
                //Debug.Log("EvNoteOff : "+channel+" k : "+key+"  t : "+t);
                this.key = key;

                deltaTime = t;
                midiStatusByte = (byte)(0x80 + channel);
                midiArg1 = (byte)key;
                midiArg2 = 0x40; // Standard velocity

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvNoteOff : ", key, "   channel : ", channel, "    delta : ", t));
                }
            }
        }

        private class EvPortamento : AKAOEvent
        {
            public EvPortamento(byte STATUS_BYTE, uint channel, byte duration, byte step)
            {
                deltaTime = 0x00;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x41;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvPortamento :    channel : ", channel, "  duration :", duration, "  step : ", step));
                }
            }
        }


        private class EvExprSlide : AKAOEvent
        {
            private uint duration;
            private uint expression;

            public EvExprSlide(byte STATUS_BYTE, uint duration, uint expression)
            {
                this.duration = duration;
                this.expression = expression;
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvExprSlide : ", duration, ", ", expression));
                }
            }
        }

        private class EvPitchBend : AKAOEvent
        {
            private byte _msb;
            private byte _lsb;

            public EvPitchBend(byte STATUS_BYTE, uint channel, sbyte value)
            {
                _msb = (byte)(0x40 + value);
                _lsb = 0x00;
                deltaTime = 0x00;
                midiStatusByte = (byte)(0xE0 + channel);
                midiArg1 = _lsb;
                midiArg2 = _msb;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvPitchBend : ", value));
                }
            }
        }


        private class EvTrackName : AKAOEvent
        {

            public EvTrackName(string trackName)
            {
                
                deltaTime = 0x00;
                midiStatusByte = 0xFF;
                midiArg1 = 0x03;

                byte[] bytes = Encoding.ASCII.GetBytes(trackName);
                VLQ nameVlq = new VLQ((uint)bytes.Length);
                List<byte> sizeNDatas = new List<byte>();
                sizeNDatas.AddRange(nameVlq.Bytes);
                sizeNDatas.AddRange(bytes);
                tail = sizeNDatas.ToArray();
                
            }
        }

    private class EvEndTrack : AKAOEvent
        {
            public EvEndTrack(byte STATUS_BYTE, uint trackId, uint delta = 0x00, bool bigEnd = false)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvEndTrack : " + trackId, "    EOF : ", bigEnd));
                    Debug.Log("|------------------------------------------------------------------------------------------------");

                    /*
                    if (!bigEnd)
                    {
                        deltaTime = delta;
                        midiStatusByte = 0xFF;
                        midiArg1 = 0x2F;
                        midiArg2 = 0x00;
                    }
                    */
                }

            }
        }
        #endregion







        private class EvUnknown : AKAOEvent
        {
            private byte v;

            public EvUnknown(byte STATUS_BYTE)
            {

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogError(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvUnknown : ", STATUS_BYTE));
                }
            }

            public EvUnknown(byte STATUS_BYTE, byte v)
            {
                this.v = v;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogError(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvUnknown : ", STATUS_BYTE, ", ", v));
                }
            }

            public EvUnknown(byte STATUS_BYTE, byte[] bytes)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogError(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvUnknown : ", BitConverter.ToString(bytes)));
                }
            }
        }

        private class EvPanSlide : AKAOEvent
        {
            private uint duration;
            private int pan;

            public EvPanSlide(byte STATUS_BYTE, uint channel, uint duration, int pan)
            {
                this.duration = duration;
                this.pan = pan;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvPanSlide : ", duration, ", ", pan));
                }
            }
        }

        private class EvAttack : AKAOEvent
        {
            private int attack;

            public EvAttack(byte STATUS_BYTE, int attack)
            {
                this.attack = attack;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvAttack : ", attack));
                }
            }
        }

        private class EvDecay : AKAOEvent
        {
            private int decay;

            public EvDecay(byte STATUS_BYTE, int decay)
            {
                this.decay = decay;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvDecay : ", decay));
                }
            }
        }

        private class EvSustain : AKAOEvent
        {
            private int sustain;

            public EvSustain(byte STATUS_BYTE, int sustain)
            {
                this.sustain = sustain;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvSustain : ", sustain));
                }
            }
        }

        private class EvSustainRelease : AKAOEvent
        {
            private uint duration;

            public EvSustainRelease(byte STATUS_BYTE, uint duration)
            {
                this.duration = duration;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvSustainRelease : ", duration));
                }
            }
        }

        private class EvRelease : AKAOEvent
        {
            private uint duration;

            public EvRelease(byte STATUS_BYTE, uint duration)
            {
                this.duration = duration;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvRelease : ", duration));
                }
            }
        }

        private class EvResetADSR : AKAOEvent
        {
            public EvResetADSR(byte STATUS_BYTE)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvResetADSR "));
                }
            }
        }

        private class EvLFOPitchRange : AKAOEvent
        {
            private byte v1;
            private byte v2;
            private byte v3;

            public EvLFOPitchRange(byte STATUS_BYTE, byte v1, byte v2, byte v3)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvLFOPitchRange :  ", v1, ", ", v2, ", ", v3));
                }
            }
        }

        private class EvLFOPitchDepth : AKAOEvent
        {
            private int depth;

            public EvLFOPitchDepth(byte STATUS_BYTE, int depth)
            {
                this.depth = depth;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvLFOPitchDepth : ", depth));
                }
            }
        }

        private class EvLFOPitchOff : AKAOEvent
        {
            public EvLFOPitchOff(byte STATUS_BYTE)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvLFOPitchOff : "));
                }
            }
        }

        private class EvLFOExprRange : AKAOEvent
        {
            private byte v1;
            private byte v2;
            private byte v3;

            public EvLFOExprRange(byte STATUS_BYTE, byte v1, byte v2, byte v3)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvLFOExprRange :  ", v1, ", ", v2, ", ", v3));
                }
            }
        }

        private class EvLFOExprDepth : AKAOEvent
        {
            private int depth;

            public EvLFOExprDepth(byte STATUS_BYTE, int depth)
            {
                this.depth = depth;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvLFOExprDepth : ", depth));
                }
            }
        }

        private class EvLFOExprOff : AKAOEvent
        {
            public EvLFOExprOff(byte STATUS_BYTE)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvLFOExprOff : "));
                }
            }
        }

        private class EvLFOPanpotRange : AKAOEvent
        {
            private byte v1;
            private byte v2;

            public EvLFOPanpotRange(byte STATUS_BYTE, byte v1, byte v2)
            {
                this.v1 = v1;
                this.v2 = v2;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvLFOPanpotRange :  ", v1, ", ", v2));
                }
            }
        }

        private class EvLFOPanpotDepth : AKAOEvent
        {
            private int depth;

            public EvLFOPanpotDepth(byte STATUS_BYTE, int depth)
            {
                this.depth = depth;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvLFOPanpotDepth : ", depth));
                }
            }
        }

        private class EvLFOPanpotOff : AKAOEvent
        {

            public EvLFOPanpotOff(byte STATUS_BYTE)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvLFOPanpotOff"));
                }
            }
        }

        private class EvTranspose : AKAOEvent
        {
            private int transpose;

            public EvTranspose(byte STATUS_BYTE, int transpose)
            {
                this.transpose = transpose;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvTranspose : ", transpose));
                }
            }
        }

        private class EvTransposeMove : AKAOEvent
        {
            private int transpose;

            public EvTransposeMove(byte STATUS_BYTE, int transpose)
            {
                this.transpose = transpose;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvTransposeMove : ", transpose));
                }
            }
        }


        private class EvNoiseOn : AKAOEvent
        {
            public EvNoiseOn(byte STATUS_BYTE)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvNoiseOn"));
                }
            }
        }

        private class EvNoiseOff : AKAOEvent
        {
            public EvNoiseOff(byte STATUS_BYTE)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvNoiseOff"));
                }
            }
        }

        private class EvFMOn : AKAOEvent
        {
            public EvFMOn(byte STATUS_BYTE)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvFMOn"));
                }
            }
        }

        private class EvFMOff : AKAOEvent
        {
            public EvFMOff(byte STATUS_BYTE)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvFMOff"));
                }
            }
        }

        private class EvSlurOn : AKAOEvent
        {
            public EvSlurOn(byte STATUS_BYTE)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvSlurOn"));
                }
            }
        }

        private class EvSlurOff : AKAOEvent
        {
            public EvSlurOff(byte STATUS_BYTE)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvSlurOff"));
                }
            }
        }

        private class EvPitchBendMove : AKAOEvent
        {
            private uint value;

            public EvPitchBendMove(byte STATUS_BYTE, uint value)
            {
                this.value = value;
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvPitchBendMove : ", value));
                }
            }
        }

        private class EvTempoSlide : AKAOEvent
        {
            public EvTempoSlide(byte STATUS_BYTE)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvTempoSlide"));
                }
            }

            public EvTempoSlide(byte STATUS_BYTE, byte v1, byte v2, ushort delta)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvTempoSlide"));
                }
            }
        }


        private class EvReverbFade : AKAOEvent
        {
            private byte v1;
            private byte v2;

            public EvReverbFade(byte STATUS_BYTE, byte v1, byte v2)
            {
                this.v1 = v1;
                this.v2 = v2;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvReverbFade :  ", v1, ", ", v2));
                }
            }
        }





        #region Non MIDI EVENTS

        private class EvTieTime : AKAOEvent
        {
            private uint value;

            public EvTieTime(byte STATUS_BYTE, uint value)
            {
                this.value = value;
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvTieTime : " + value));
                }
            }
        }
        private class EvRest : AKAOEvent
        {
            private uint duration;

            public EvRest(byte STATUS_BYTE, uint duration)
            {
                this.duration = duration;
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvRest : " + duration));
                }
            }
        }
        private class EvOctave : AKAOEvent
        {
            private uint _octave;

            public EvOctave(byte STATUS_BYTE, uint octave)
            {
                _octave = octave;
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvOctave : ", octave));
                }
            }
        }

        private class EvOctaveUp : AKAOEvent
        {
            public EvOctaveUp(byte STATUS_BYTE)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvOctaveUp"));
                }
            }
        }

        private class EvOctaveDown : AKAOEvent
        {
            public EvOctaveDown(byte STATUS_BYTE)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvOctaveDown"));
                }
            }
        }


        private class EvRepeatStart : AKAOEvent
        {
            public EvRepeatStart(byte STATUS_BYTE)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvRepeatStart"));
                    Debug.Log(string.Concat("<<<<-----------------------------------------VV---------------------------------------------->>>>"));
                }
            }
        }
        private class EvRepeatEnd : AKAOEvent
        {
            private int loopId;

            public EvRepeatEnd(byte STATUS_BYTE, long offset)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvRepeatEnd  to : ", offset));
                }
            }

            public EvRepeatEnd(byte STATUS_BYTE, long offset, int loopId)
            {
                this.loopId = loopId;
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvRepeatEnd  to : ", offset, "   for ", loopId, "  times"));
                }
            }
        }
        private class EvDrumKitOn : AKAOEvent
        {

            public EvDrumKitOn(byte STATUS_BYTE)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvDrumKitOn"));
                }
            }
        }

        private class EvDrumKitOff : AKAOEvent
        {

            public EvDrumKitOff(byte STATUS_BYTE)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvDrumKitOff"));
                }
            }
        }

        private class EvPermaLoopBreak : AKAOEvent
        {
            private uint cTrackId;
            private byte cond;
            private long dest;

            public EvPermaLoopBreak(byte STATUS_BYTE, uint cTrackId, byte cond, long dest)
            {
                this.cTrackId = cTrackId;
                this.cond = cond;
                this.dest = dest;


                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvPermaLoopBreak : ", dest, "   condition : ", cond));
                }
            }
        }

        private class EvPermaLoop : AKAOEvent
        {
            private uint cTrackId;
            private long dest;

            public EvPermaLoop(byte STATUS_BYTE, uint cTrackId, long dest)
            {
                this.cTrackId = cTrackId;
                this.dest = dest;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvPermaLoop : ", dest));
                }
            }
        }

        private class EvRepeatBreak : AKAOEvent
        {
            public EvRepeatBreak(byte STATUS_BYTE, byte[] b)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvRepeatBreak : ", BitConverter.ToString(b)));
                }
            }
        }
        #endregion
    }
}
