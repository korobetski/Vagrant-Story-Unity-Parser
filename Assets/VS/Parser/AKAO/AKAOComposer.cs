using AudioSynthesis.Bank;
using AudioSynthesis.Sequencer;
using AudioSynthesis.Synthesis;
using AudioSynthesis.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityMidi;
using VS.Format;
using VS.Utils;

//Minoru Akao
//https://github.com/vgmtrans/vgmtrans/blob/master/src/main/formats/AkaoSeq.cpp
//https://github.com/vgmtrans/vgmtrans/blob/7b48bd60de62ade22ed8c471a354e04c62b7df3f/src/main/formats/AkaoSeq.cpp 
// Akao in MUSIC folder contains musical instructions like a Midi file but also some synthetizer controls.
// Two different instruments cannot play in the same channel at the same time


// MUSIC020 -> OST Graylands Incident Climax part ~8' (there is a little mistake to correct)
// MUSIC021 -> OST Graylands Incident Climax part 10'25
// MUSIC022 -> OST Temple of Kiltia
// MUSIC023 -> OST Snowfly Forest
// MUSIC024 -> Unfinished music
// MUSIC026 -> OST Part of Graylands Incident Climax ()
// MUSIC027 -> OST Part of Graylands Incident Climax (At ~1:50)
// MUSIC028 -> Not in OST
// MUSIC029 -> OST Catacombs
// MUSIC030 -> OST Sanctum
// MUSIC031 -> OST Dullahan
// MUSIC032 -> OST Minotaur
// MUSIC033 -> OST Iron Crab
// MUSIC034 -> OST Tieger & Neesa
// MUSIC035 -> OST Kali
// MUSIC036 -> OST Banquet of Transmigration
// MUSIC037 -> OST Lizardman
// MUSIC038 -> OST Nightmare
// MUSIC039 -> OST Grotesque Creature
// MUSIC040 -> OST Factory
// MUSIC041 -> OST Great Cathedral
// MUSIC043 -> OST Abandoned Mines B1
// MUSIC044 -> OST Snowfly Forest
// MUSIC045 -> OST Wyvern
// MUSIC046 -> OST Golem
// MUSIC047 -> OST Ogre
// MUSIC048 -> OST Dark Element
// MUSIC049 -> OST Ifrit
// MUSIC180 -> OST Joshua (another version ?)
namespace VS.Parser.Akao
{
    public class AKAOComposer
    {
        public static readonly ushort[] delta_time_table = { 0xC0, 0x60, 0x30, 0x18, 0x0C, 0x6, 0x3, 0x20, 0x10, 0x8, 0x4, 0x0, 0xA0A0, 0xA0A0 };
        public static bool UseDebug = false;
        public static bool UseDebugFull = false;

        public List<uint> A1Calls;
        public List<uint> progIDs;
        public bool DrumKitOn = false;


        private BinaryReader buffer;
        private string name;
        private long start;
        private long end;
        private uint numTrack;
        private ushort[] tracksPtr;
        private uint numInstr;
        private AKAOTrack[] tracks;
        private uint velocity = 127;
        private int quarterNote = 0x30;





        public AKAOComposer(BinaryReader buffer, long start, long end, uint NI, uint NT, ushort[] tracksPtr, string name, bool UseDebug = false)
        {
            this.buffer = buffer;
            AKAOComposer.UseDebug = UseDebug;
            numTrack = NT;
            this.tracksPtr = tracksPtr;
            numInstr = NI;
            this.name = name;

            this.start = start;
            this.end = end;

            A1Calls = new List<uint>();

            buffer.BaseStream.Position = start;

            SetTracks();
        }

        public void Synthetize(bool bMid, bool bWav, SF2 soundfont = null)
        {
            SyncTracksChannels();
            //MergePolyTracks();

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
                    track.OrderByAbsTime();

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
            if (bMid)
            {
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

            if ((File.Exists("Assets/Resources/Sounds/SF2/" + name + ".sf2") || soundfont != null) && bWav)
            {
                // so we try to build a sampled audio file.
                int channel = 2;
                int sampleRate = 44100;
                int bufferSize = 1024;
                Synthesizer synthesizer = new Synthesizer(sampleRate, channel, bufferSize, 1);
                MidiFileSequencer sequencer = new MidiFileSequencer(synthesizer);
                if (soundfont != null)
                {
                    MemoryStream sf2MemStr = soundfont.MemorySave();
                    sf2MemStr.Close();
                    synthesizer.LoadBank(new PatchBank(new MemoryAsset(name + ".sf2", sf2MemStr.GetBuffer())));
                }
                else if (File.Exists("Assets/Resources/Sounds/SF2/" + name + ".sf2"))
                {
                    synthesizer.LoadBank(new PatchBank(new AssetResouce("Resources/Sounds/SF2/" + name + ".sf2")));
                }
                if (File.Exists("Resources/Sounds/" + name + ".mid"))
                {
                    sequencer.LoadMidi(new AssetResouce("Resources/Sounds/" + name + ".mid"));
                }
                else
                {
                    sequencer.LoadMidi(new MemoryAsset(name + ".mid", midiByte.ToArray()));
                }
                ToolBox.DirExNorCreate("Assets/Resources/Sounds/WAV/");
                File.Create("Assets/Resources/Sounds/WAV/" + name + ".tmp").Close();
                File.Create("Assets/Resources/Sounds/WAV/" + name + ".wav").Close();
                AssetResouce temp = new AssetResouce("Resources/Sounds/WAV/" + name + ".tmp");

                using (WaveFileWriter wavManager = new WaveFileWriter(sampleRate, 2, 16, temp, new AssetResouce("Resources/Sounds/WAV/" + name + ".wav")))
                {
                    sequencer.Play();
                    while (sequencer.CurrentTime < sequencer.EndTime)
                    {
                        int newMSize = (int)(synthesizer.MicroBufferSize * sequencer.PlaySpeed);
                        bool reachEnd = (sequencer.CurrentTime + newMSize >= sequencer.EndTime) ? true : false;
                        sequencer.FillMidiEventQueue();
                        synthesizer.GetNext();
                        wavManager.Write(synthesizer.WorkingBuffer);
                        if (reachEnd)
                        {
                            break;
                        }
                    }
                    wavManager.Close();
                }
            }
        }

        private void MergePolyTracks()
        {
            // here we are trying to get all tracks of the same instrument and pan in the same Midi track (15 +1)
            AKAOTrack[] MidiTracks = new AKAOTrack[16];
            uint lastEmpty = 0;
            for (uint i = 0; i < numTrack; i++)
            {
                AKAOTrack trk = tracks[i];
                if(trk.programs.Count == 1)
                {
                    if (trk.drumTrack)
                    {
                        if (MidiTracks[9] == null) MidiTracks[9] = new AKAOTrack();
                        MidiTracks[9].InsertAllEvents(trk.Events);
                    } else
                    {
                        bool instrTrackFound = false;
                        if (lastEmpty > 0)
                        {
                            // check for the same instrument track if exist
                            for (uint j = 0; j < lastEmpty; j++)
                            {
                                if (j == 9)
                                {
                                    j++;
                                    if (j == lastEmpty) break;
                                }
                                if (MidiTracks[j].mainProg == trk.mainProg && MidiTracks[j].trackPan == trk.trackPan)
                                {
                                    instrTrackFound = true;
                                    MidiTracks[j].InsertAllEvents(trk.Events);
                                    break;
                                }
                            }

                        }
                        // if not we put events in a new track
                        if (!instrTrackFound)
                        {
                            if (lastEmpty == 9) lastEmpty++;
                            MidiTracks[lastEmpty] = new AKAOTrack();
                            MidiTracks[lastEmpty].InsertAllEvents(trk.Events);
                            MidiTracks[lastEmpty].mainProg = trk.mainProg;
                            MidiTracks[lastEmpty].trackPan = trk.trackPan;
                            lastEmpty++;
                            if (lastEmpty == 9) lastEmpty++;
                        }
                    }
                } else
                {
                    for (int k = 0; k < trk.programs.Count; k++)
                    {
                        bool instrTrackFound = false;
                        if (lastEmpty > 0)
                        {
                            // check for the same instrument track if exist
                            for (uint j = 0; j < lastEmpty; j++)
                            {
                                if (j == 9)
                                {
                                    j++;
                                    if (j == lastEmpty) break;
                                }
                                if (MidiTracks[j].mainProg == trk.programs[k] && MidiTracks[j].trackPan == trk.trackPan)
                                {
                                    instrTrackFound = true;
                                    MidiTracks[j].InsertAllEvents(trk.GetInstrumentEvents(trk.programs[k]));
                                    break;
                                }
                            }

                        }
                        // if not we put events in a new track
                        if (!instrTrackFound)
                        {
                            if (lastEmpty == 9) lastEmpty++;
                            MidiTracks[lastEmpty] = new AKAOTrack();
                            MidiTracks[lastEmpty].InsertAllEvents(trk.GetInstrumentEvents(trk.programs[k]));
                            MidiTracks[lastEmpty].mainProg = trk.programs[k];
                            MidiTracks[lastEmpty].trackPan = trk.trackPan;
                            lastEmpty++;
                            if (lastEmpty == 9) lastEmpty++;
                        }

                    }
                }
            }

            for (int i = 0; i < lastEmpty; i++)
            {
                if (MidiTracks[i] != null)
                {
                    MidiTracks[i].ResetChannelTo((byte)i);
                }
            }

            tracks = MidiTracks;
        }

        private void SyncTracksChannels()
        {
            uint[] chanStat = new uint[16];
            for (int i = 0; i < 16; i++)
            {
                chanStat[i] = 0;
            }

            for (uint i = 0; i < numTrack; i++)
            {
                AKAOTrack trk = tracks[i];
                if (trk.drumTrack)
                {
                    chanStat[9]++;
                    trk.ResetChannelTo(9);
                }
                else
                {
                    if (i > 0)
                    {
                        // we check if a track already use the same programs
                        bool chk = false;
                        for (uint j = 0; j < i; j++)
                        {
                            AKAOTrack ntrk = tracks[j];
                            if (trk.programs.Except(ntrk.programs).Count() == 0 && ntrk.programs.Except(trk.programs).Count() == 0/* && chanStat[ntrk.channel] < 4*/)
                            {
                                chanStat[ntrk.channel]++;
                                trk.ResetChannelTo(ntrk.channel);
                                chk = true;
                                break;
                            }
                        }

                        if (!chk)
                        {
                            // we must find an empty chan
                            for (int j = 0; j < 16; j++)
                            {
                                if (chanStat[j] == 0 && j != 9)
                                {
                                    chanStat[j]++;
                                    trk.ResetChannelTo((byte)j);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        chanStat[tracks[0].channel]++;
                    }
                }
            }

            // if free channels left, we set bigger tracks in
            // we consider that events count determine the importance of the track

            tracks = tracks.OrderByDescending(o => o.Events.Count).ToArray();
            List<AKAOTrack> swiTracks = new List<AKAOTrack>();
            for (int j = 0; j < 16; j++)
            {
                if (chanStat[j] == 0 && j != 9)
                {
                    for (uint i = 0; i < numTrack; i++)
                    {
                        if (chanStat[tracks[i].channel] > 1 && !swiTracks.Contains(tracks[i]) && !tracks[i].drumTrack)
                        {
                            chanStat[tracks[i].channel]--;
                            tracks[i].ResetChannelTo((byte)j);
                            chanStat[j]++;
                            swiTracks.Add(tracks[i]);
                            break;
                        }
                    }
                }
            }

        }

        private void SetTracks()
        {
            long beginOffset = buffer.BaseStream.Position;
            tracks = new AKAOTrack[numTrack];

            bool playingNote = false;
            uint prevKey = 0;
            ushort delta = 0;
            ushort deltaplane = 0;
            byte channel = 0;
            uint octave = 0;
            byte currentExpr = 0;

            int repeatIndex = -1;
            int repeatNumber = 0;
            List<long> repeaterStartPositions = new List<long>();
            List<long> repeaterEndPositions = new List<long>();
            List<long> condLoops = new List<long>();

            progIDs = new List<uint>();

            for (uint t = 0; t < numTrack; t++)
            {

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning("## TRACK : " + t + "   -----------------------------------------------------------------------");
                }

                tracks[t] = new AKAOTrack();
                AKAOTrack curTrack = tracks[t];

                deltaplane = 0;
                delta = 0;
                channel = 0;
                curTrack.channel = channel;
                long trackEnd = (t < numTrack - 1) ? tracksPtr[t + 1] : end;

                while (buffer.BaseStream.Position < trackEnd)
                {
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
                        if (deltaplane > 0)
                        {
                            delta = deltaplane;
                            deltaplane = 0;
                        }
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

                        curTrack.AddEvent(new EvNoteOn(STATUS_BYTE, channel, key, velocity, delta, true));
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
                                curTrack.AddEvent(new EvEndTrack(STATUS_BYTE, t, delta));
                                break;
                            case 0xA1:// Program Change (articulation)
                                byte prog = buffer.ReadByte();
                                if (!A1Calls.Contains(prog))
                                {
                                    A1Calls.Add(prog);
                                }

                                if (!progIDs.Contains(prog))
                                {
                                    progIDs.Add(prog);
                                }

                                if (curTrack.LastProgram() != prog)
                                {
                                    curTrack.AddEvent(new EvProgramChange(STATUS_BYTE, channel, prog, delta));
                                }

                                if (curTrack.name == "AKAOTrack")
                                {
                                    curTrack.name = string.Concat("Track ", SMF.GetName(prog));
                                    curTrack.AddEvent(new EvTrackName(curTrack.name));
                                }
                                delta = 0;
                                break;
                            case 0xA2: // Delta plane
                                byte time = buffer.ReadByte();
                                deltaplane = time;
                                curTrack.AddEvent(new EvDeltaplane(STATUS_BYTE, time));
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
                                byte expression = buffer.ReadByte();
                                // maybe we shouldn't reset delta
                                currentExpr = expression;
                                curTrack.AddEvent(new EvExpr(STATUS_BYTE, channel, expression, delta));
                                delta = 0;
                                break;
                            case 0xA9:// Expression Slide MUSIC050
                                uint duration = buffer.ReadByte();
                                expression = buffer.ReadByte();
                                /*
                                curTrack.AddEvent(new EvExprSlide(STATUS_BYTE, channel, expression, delta));
                                delta = 0;
                                */
                                // Todo : make a linear interpolation of expression

                                int interpolations = Mathf.RoundToInt(duration / 12);
                                uint baseTime = curTrack.Events[curTrack.Events.Count - 1].absTime + delta;
                                for (int it = 0; it < interpolations; it++)
                                {
                                    byte linExpr = (byte)(currentExpr + ((expression - currentExpr) / interpolations) * it);
                                    curTrack.InsertEvent(new EvExprSlide(STATUS_BYTE, channel, linExpr), (uint)(baseTime + it * 12));
                                }

                                break;
                            case 0xAA:// Pan
                                curTrack.AddEvent(new EvPan(STATUS_BYTE, channel, buffer.ReadByte(), delta));
                                delta = 0;
                                break;
                            case 0xAB:// Pan Fade
                                curTrack.AddEvent(new EvPanSlide(STATUS_BYTE, channel, buffer.ReadByte(), buffer.ReadByte()));
                                break;
                            case 0xAC: // Unknown
                                curTrack.AddEvent(new EvUnknown(STATUS_BYTE, buffer.ReadByte()));
                                break;
                            case 0xAD: // Attack
                                curTrack.AddEvent(new EvAttack(STATUS_BYTE, channel, buffer.ReadByte(), delta));
                                delta = 0;
                                break;
                            case 0xAE: // Decay
                                byte decay = buffer.ReadByte();
                                curTrack.AddEvent(new EvDecay(STATUS_BYTE, channel, decay, delta));
                                delta = 0;
                                break;
                            case 0xAF: // Sustain
                                byte sustain = buffer.ReadByte();
                                curTrack.AddEvent(new EvSustain(STATUS_BYTE, channel, sustain, delta));
                                break;
                            case 0xB0: // Decay + Sustain
                                decay = buffer.ReadByte();
                                sustain = buffer.ReadByte();
                                curTrack.AddEvent(new EvDecay(STATUS_BYTE, channel, decay, delta));
                                delta = 0;
                                curTrack.AddEvent(new EvSustain(STATUS_BYTE, channel, sustain, delta));
                                break;
                            case 0xB1: // Sustain ?
                                curTrack.AddEvent(new EvSustainRelease(STATUS_BYTE, channel, buffer.ReadByte(), delta));
                                //delta = 0;
                                break;
                            case 0xB2: // Release
                                curTrack.AddEvent(new EvRelease(STATUS_BYTE, channel, buffer.ReadByte(), delta));
                                delta = 0;
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
                            case 0xBB: // ADSR_SUSTAIN_MODE occur when AKAO MUSIC unk3 is 255 or -1 (MUSIC074, 75, 76, 77, ...) (0xA2-0xE1-0xBB)
                                b = buffer.ReadBytes(2);
                                curTrack.AddEvent(new EvUnknown(STATUS_BYTE, b));
                                break;
                            // LFO (low-frequency oscillators) Panpot
                            case 0xBC: // LFO Panpot Range
                                b = buffer.ReadBytes(2);
                                curTrack.AddEvent(new EvLFOPanpotRange(STATUS_BYTE, channel, b[0], b[1], delta));
                                delta = 0;
                                break;
                            case 0xBD: // LFO Panpot Depth
                                curTrack.AddEvent(new EvLFOPanpotDepth(STATUS_BYTE, channel, buffer.ReadByte(), delta));
                                delta = 0;
                                break;
                            case 0xBE: // LFO Panpot On / Off ?
                                curTrack.AddEvent(new EvLFOPanpotOff(STATUS_BYTE, channel, delta));
                                delta = 0;
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
                                curTrack.AddEvent(new EvRepeatStart(STATUS_BYTE, buffer.BaseStream.Position));
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
                                }
                                curTrack.AddEvent(new EvRepeatEnd(STATUS_BYTE, repeatIndex));
                                break;
                            case 0xCB: // RESET_VOICE_EFFECTS
                                curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                                break;
                            case 0xCC: // Slur On
                                curTrack.AddEvent(new EvSlurOn(STATUS_BYTE));
                                break;
                            case 0xCD: // Slur Off
                                curTrack.AddEvent(new EvSlurOff(STATUS_BYTE));
                                break;
                            case 0xCE: // NOISE_ON_DELAY_TOGGLE
                                curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                                break;
                            case 0xCF: // NOISE_DELAY_TOGGLE;
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
                            case 0xDC: // ?? (MUSIC 088 to 091) when unk3 is 16383
                                curTrack.AddEvent(new EvUnknown(STATUS_BYTE, buffer.ReadByte()));
                                break;
                            case 0xDD: // ?? when AKAO MUSIC unk3 is 255 or -1
                                b = buffer.ReadBytes(2);
                                curTrack.AddEvent(new EvUnknown(STATUS_BYTE, b));
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
                            case 0xE1: // Unknown       (2x in MUSIC058 set to 44 | 8x in MUSIC074 set to 64)
                                curTrack.AddEvent(new EvUnknown(STATUS_BYTE, buffer.ReadByte()));
                                break;
                            case 0xE2: // Unknown       (6x in MUSIC074)
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
                                        DrumKitOn = true;
                                        curTrack.drumTrack = true;
                                        curTrack.AddEvent(new EvDrumKitOn(STATUS_BYTE));
                                        curTrack.AddEvent(new EvBank(STATUS_BYTE, channel, 128, delta));
                                        delta = 0;
                                        curTrack.AddEvent(new EvProgramChange(STATUS_BYTE, channel, 0, delta));
                                        //curTrack.ResetChannelTo(9);
                                        break;
                                    case 0x05: // Drum kit Off
                                        curTrack.AddEvent(new EvDrumKitOff(STATUS_BYTE));
                                        //curTrack.AddEvent(new EvBank(STATUS_BYTE, channel, 0, delta));
                                        //delta = 0;
                                        break;
                                    case 0x06: // Perma Loop
                                        long dest = buffer.BaseStream.Position + buffer.ReadInt16();
                                        long loopLen = buffer.BaseStream.Position - beginOffset;
                                        if (trackEnd == buffer.BaseStream.Position)
                                        {
                                            // this is the end of the track, we turn off if a note is playing
                                            if (playingNote)
                                            {
                                                curTrack.AddEvent(new EvNoteOff(STATUS_BYTE, channel, prevKey, delta));
                                                delta = 0;
                                                playingNote = false;
                                            }
                                        }
                                        curTrack.AddEvent(new EvPermaLoop(STATUS_BYTE, t, dest));
                                        // TODO add a midi loop point

                                        break;
                                    case 0x07: // Perma Loop break with conditional.
                                        byte cond = buffer.ReadByte();
                                        dest = buffer.BaseStream.Position + buffer.ReadInt16();
                                        loopLen = buffer.BaseStream.Position - beginOffset;
                                        curTrack.AddEvent(new EvPermaLoopBreak(STATUS_BYTE, t, cond, dest));
                                        break;
                                    case 0x09: // Repeat Break
                                        b = buffer.ReadBytes(3);
                                        curTrack.AddEvent(new EvRepeatBreak(STATUS_BYTE, b));
                                        break;
                                    case 0x0E: // call subroutine Recurent in MUSIC034 (total festa 200 calls...), 123, ...
                                               // for some reason there is several calls of this instruction for some 0x0F below before the track ends
                                               // maybe its work like a if / else statment or a kind of sysex message
                                        b = buffer.ReadBytes(2);
                                        curTrack.AddEvent(new EvSubRoutine(STATUS_BYTE, b));
                                        break;
                                    case 0x0F: // return from subroutine

                                        if (trackEnd == buffer.BaseStream.Position)
                                        {
                                            // this is the end of the track, we turn off if a note is playing
                                            if (playingNote)
                                            {
                                                curTrack.AddEvent(new EvNoteOff(STATUS_BYTE, channel, prevKey, delta));
                                                delta = 0;
                                                playingNote = false;
                                            }
                                        }
                                        curTrack.AddEvent(new EvEndSubRoutine(STATUS_BYTE, Meta));
                                        break;
                                    case 0x10: // Unknown
                                        curTrack.AddEvent(new EvUnknown(STATUS_BYTE, Meta));
                                        break;
                                    case 0x14: // Program Change (articulation target)
                                        byte art = buffer.ReadByte();
                                        if (!progIDs.Contains(art))
                                        {
                                            progIDs.Add(art);
                                        }

                                        if (curTrack.LastProgram() != art)
                                        {
                                            curTrack.AddEvent(new EvProgramChange(STATUS_BYTE, channel, art, delta));
                                        }

                                        if (curTrack.name == "AKAOTrack")
                                        {
                                            curTrack.name = string.Concat("Track ", SMF.INSTRUMENTS[art]);
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
                                    case 0x1F: // Unknown Recurent in MUSIC004, MUSIC005, MUSIC016, MUSIC017, MUSIC018, MUSIC020, MUSIC025, MUSIC026
                                               // often on a side of a Rest Event
                                        curTrack.AddEvent(new EvUnknown(STATUS_BYTE, Meta));
                                        break;
                                    default:
                                        curTrack.AddEvent(new EvUnknown(STATUS_BYTE, Meta));
                                        break;
                                }
                                break;
                            case 0xFF: // End Track Padding
                                break;
                            default:
                                Debug.LogWarning("Unknonw instruction in " + name + " at " + buffer.BaseStream.Position + "  ->  " + (byte)STATUS_BYTE);
                                //curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                                break;
                        }
                    }
                }
            }
        }

        private class AKAOTrack
        {
            public string name = "AKAOTrack";
            public byte channel = 0;
            public short mainProg = -1;
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


            public byte? LastProgram()
            {
                if (programs.Count > 0)
                {
                    return programs[programs.Count - 1];
                }
                else
                {
                    return null;
                }
            }


            public void ResetChannelTo(byte newChan)
            {
                channel = newChan;
                if (events != null && events.Count > 0)
                {
                    for (int i = 0; i < events.Count; i++)
                    {
                        AKAOEvent lEv = events[i];
                        if (!lEv.meta && lEv.midiStatusByte != 0)
                        {
                            lEv.midiStatusByte -= lEv.channel;
                            lEv.channel = newChan;
                            lEv.midiStatusByte += lEv.channel;
                        }
                    }
                }
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
                }
                else
                {
                    ev.absTime = ev.deltaTime;
                }

                if (ev.GetType() == typeof(EvPan))
                {
                    trackPan = (byte)ev.midiArg2;
                }
                if (ev.GetType() == typeof(EvProgramChange))
                {
                    if (mainProg == -1)
                    {
                        mainProg = (short)ev.midiArg1;
                    }

                    programs.Add((byte)ev.midiArg1);
                }
                if (ev.GetType() == typeof(EvDrumKitOn))
                {
                    drumTrack = true;
                }
                events.Add(ev);
            }

            public void InsertEvent(AKAOEvent ev, uint atTime)
            {
                if (events == null)
                {
                    events = new List<AKAOEvent>();
                }


                ev.absTime = atTime;

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

                // add event to the end of the list and then reorder the list in a proper way
                events.Add(ev);
                OrderByAbsTime();
            }

            internal void InsertAllEvents(List<AKAOEvent> iev)
            {
                for (int i = 0; i < iev.Count; i++)
                {
                    AKAOEvent lEv = iev[i];
                    if (events == null)
                    {
                        events = new List<AKAOEvent>();
                    }
                    if (lEv.GetType() == typeof(EvPan))
                    {
                        trackPan = (byte)lEv.midiArg2;
                    }
                    if (lEv.GetType() == typeof(EvProgramChange))
                    {
                        programs.Add((byte)lEv.midiArg1);
                    }
                    if (lEv.GetType() == typeof(EvBank))
                    {
                        programs.Add((byte)lEv.midiArg2);
                    }
                    if (lEv.GetType() == typeof(EvDrumKitOn))
                    {
                        drumTrack = true;
                    }
                    events.Add(lEv);
                }
                OrderByAbsTime();
            }

            internal List<AKAOEvent> GetInstrumentEvents(uint instrumentId)
            {
                List<AKAOEvent> instrEvents = new List<AKAOEvent>();
                bool inInstrEvents = false;

                if (events != null && events.Count > 1)
                {
                    for (int i = 0; i < events.Count; i++)
                    {
                        AKAOEvent lEv = events[i];
                        if (lEv.GetType() == typeof(EvProgramChange))
                        {
                            if (lEv.midiArg1 == instrumentId)
                            {
                                inInstrEvents = true;
                                instrEvents.Add(lEv);
                            } else
                            {
                                inInstrEvents = false;
                            }
                        } else
                        {
                            if (inInstrEvents)
                            {
                                instrEvents.Add(lEv);
                            }
                        }
                    }
                }

                return instrEvents;
            }


            private void RecomputeDeltas()
            {
                if (events != null && events.Count > 1)
                {
                    for (int i = 0; i < events.Count; i++)
                    {
                        AKAOEvent lEv = events[i];
                        if (i == 0)
                        {
                            lEv.deltaTime = lEv.absTime;
                        }
                        else
                        {
                            AKAOEvent pEv = events[i - 1];
                            lEv.deltaTime = lEv.absTime - pEv.absTime;
                        }
                    }
                }
            }

            internal void OrderByAbsTime()
            {
                events = events.OrderBy(o => o.absTime).ToList();
                RecomputeDeltas();
            }

            internal AKAOEvent LastEvent()
            {
                if (events != null && events.Count > 0)
                {
                    return events[events.Count - 1];
                }
                else
                {
                    return null;
                }
            }
        }



        private class AKAOEvent
        {
            internal bool meta = false;
            internal byte channel;
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


        #region MIDI Voice Events

        // A logarithmic scale is recommended for velocity.
        private class EvNoteOff : AKAOEvent
        {
            //"8nH + 2 Bytes"; // 1000	MIDI channel [0 - 15]	Key Number [0 - 127]	Velocity [0 - 127]
            public EvNoteOff(byte STATUS_BYTE, byte channel, uint key, uint t)
            {
                this.channel = channel;
                deltaTime = t;
                midiStatusByte = (byte)(0x80 + channel);
                midiArg1 = (byte)key;
                midiArg2 = 0x40; // Standard velocity
                if (UseDebugFull)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvNoteOff : ", key, "   channel : ", channel, "    delta : ", t));
                }
            }
        }
        private class EvNoteOn : AKAOEvent
        {
            //"9nH + 2 Bytes"; // 1001	MIDI channel [0 - 15]	Key Number [0 - 127]	Velocity [0 - 127]
            /*
            0        1      64      127
            off ppp p pp mp mf f ff fff
            */
            public EvNoteOn(byte STATUS_BYTE, byte channel, uint key, uint velocity, uint t = 0x00, bool alt = false)
            {
                this.channel = channel;
                deltaTime = t;
                midiStatusByte = (byte)(0x90 + channel);
                midiArg1 = (byte)key;
                midiArg2 = (byte)velocity;
                if (UseDebugFull)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvNoteOn ", alt ? "(Alt)" : "", " : ", key, "   velocity : ", velocity, "   channel : ", channel, "    delta : ", t));
                }
            }
        }
        private class EvAfterTouch : AKAOEvent
        {
            public EvAfterTouch(byte STATUS_BYTE, byte channel, uint key, uint value, uint t = 0x00)
            {
                this.channel = channel;
                deltaTime = t;
                midiStatusByte = (byte)(0xA0 + channel);
                midiArg1 = (byte)key;
                midiArg2 = (byte)value;
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvAfterTouch : ", key, "   value : ", value, "   channel : ", channel, "    delta : ", t));
                }
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
            public EvProgramChange(byte STATUS_BYTE, byte channel, byte prog, uint delta = 0x00)
            {
                this.channel = channel;
                deltaTime = delta;
                midiStatusByte = (byte)(0xC0 + channel);
                midiArg1 = (byte)prog;

                if (AKAOComposer.UseDebug)
                {
                    string progName = "" + prog;
                    if (prog < SMF.INSTRUMENTS.Length)
                    {
                        progName = SMF.INSTRUMENTS[prog];
                    }
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvProgramChange : ", prog, "  ", progName, "   channel : ", channel, "    delta : ", delta));
                }
            }
        }
        private class EvChannelPressure : AKAOEvent
        {
            public EvChannelPressure(byte STATUS_BYTE, byte channel, byte value, uint delta = 0x00)
            {
                this.channel = channel;
                deltaTime = delta;
                midiStatusByte = (byte)(0xD0 + channel);
                midiArg1 = value;
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvChannelPressure : ", value));
                }
            }
        }
        private class EvPitchBend : AKAOEvent
        {
            private byte _msb;
            private byte _lsb;

            public EvPitchBend(byte STATUS_BYTE, byte channel, sbyte value)
            {
                this.channel = channel;
                _msb = (byte)(0x40 + value);
                _lsb = 0x00;
                deltaTime = 0x00;
                midiStatusByte = (byte)(0xE0 + channel);
                midiArg1 = _lsb;
                midiArg2 = _msb;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvPitchBend : LSB ", _lsb, "  MSB ", _msb));
                }
            }
        }
        #endregion

        #region MIDI CONTROL CHANGE Events
        private class EvBank : AKAOEvent
        {
            public EvBank(byte STATUS_BYTE, byte channel, byte bank, ushort delta = 0x00)
            {
                this.channel = channel;
                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = (byte)0x00; // 0x00 MSB (Coarse) Bank select | 0x20 LSB (Fine) Bank select
                midiArg2 = bank;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvBank : ", bank, "   channel : ", channel, "    delta : ", delta));
                }
            }
        }

        private class EvLFOPanpotRange : AKAOEvent
        {
            public EvLFOPanpotRange(byte STATUS_BYTE, byte channel, byte v1, byte v2, ushort delta = 0x00)
            {
                this.channel = channel;
                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x01;
                midiArg2 = v2;
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvLFOPanpotRange :  ", v1, ", ", v2));
                }
            }
        }

        private class EvLFOPanpotDepth : AKAOEvent
        {
            public EvLFOPanpotDepth(byte STATUS_BYTE, byte channel, byte depth, ushort delta = 0x00)
            {
                this.channel = channel;
                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x01;
                midiArg2 = depth;
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvLFOPanpotDepth : ", depth));
                }
            }
        }

        private class EvLFOPanpotOff : AKAOEvent
        {
            public EvLFOPanpotOff(byte STATUS_BYTE, byte channel, ushort delta = 0x00)
            {
                this.channel = channel;
                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x01;
                midiArg2 = 0x00;
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogError(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvLFOPanpotOff"));
                }
            }
        }

        private class EvVolume : AKAOEvent
        {

            public EvVolume(byte STATUS_BYTE, byte channel, uint volume, uint delta = 0x00)
            {
                double val = Math.Round(Math.Sqrt((volume / 127.0f)) * 127.0f);
                byte velocity = (byte)val;
                /*
                this.channel = channel;
                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x07;
                midiArg2 = (byte)velocity;
                */
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvVolume : ", volume, "   channel : ", channel, "    delta : ", delta));
                }
            }
        }
        private class EvPan : AKAOEvent
        {
            public EvPan(byte STATUS_BYTE, byte channel, int pan, uint delta = 0x00)
            {
                this.channel = channel;
                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x0A;
                midiArg2 = (byte)pan;
                //midiArg2 = 0x40;// all center

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvPan : ", pan, "   channel : ", channel, "    delta : ", delta));
                }
            }
        }
        private class EvExpr : AKAOEvent
        {
            public EvExpr(byte STATUS_BYTE, byte channel, byte expression, uint delta = 0x00)
            {
                this.channel = channel;
                double val = Math.Round(Math.Sqrt((expression / 127.0f)) * 127.0f);
                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x0B;
                midiArg2 = (byte)val;

                if (UseDebugFull)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvExpr : ", val, "   channel : ", channel, "    delta : ", delta));
                }
            }
        }
        private class EvExprSlide : AKAOEvent
        {
            public EvExprSlide(byte STATUS_BYTE, byte channel, byte expression, uint delta = 0x00)
            {
                this.channel = channel;
                double val = Math.Round(Math.Sqrt((expression / 127.0f)) * 127.0f);
                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x0B;
                midiArg2 = (byte)val;
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvExprSlide : ", val, "   channel : ", channel, "    delta : ", delta));
                }
            }
        }


        private class EvSustain : AKAOEvent
        {
            public EvSustain(byte STATUS_BYTE, byte channel, byte value, uint delta = 0)
            {
                this.channel = channel;
                /*
                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x40;
                midiArg2 = value;
                */
                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvSustain : ", value));
                }
            }
        }

        private class EvSustainRelease : AKAOEvent
        {

            public EvSustainRelease(byte STATUS_BYTE, byte channel, byte value, uint delta = 0)
            {
                this.channel = channel;
                /*
                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x40;
                midiArg2 = value;
                */
                if (AKAOComposer.UseDebug)
                {
                    // MUSIC/MUSIC021.DAT
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvSustainRelease : ", value));
                }
            }
        }
        private class EvRelease : AKAOEvent
        {
            public EvRelease(byte STATUS_BYTE, byte channel, byte release, uint delta = 0)
            {
                this.channel = channel;
                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x48;
                midiArg2 = release;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvRelease : ", release));
                }
            }
        }

        private class EvDecay : AKAOEvent
        {
            public EvDecay(byte STATUS_BYTE, byte channel, byte value, uint delta = 0)
            {
                this.channel = channel;
                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x4B;
                midiArg2 = value;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvDecay : ", value));
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

        private class EvAttack : AKAOEvent
        {
            public EvAttack(byte STATUS_BYTE, byte channel, byte attack, uint delta = 0)
            {
                this.channel = channel;
                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x49;
                midiArg2 = attack;

                if (AKAOComposer.UseDebug)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvAttack : ", attack));
                }
            }
        }
        private class EvPortamento : AKAOEvent
        {
            public EvPortamento(byte STATUS_BYTE, byte channel, byte duration, byte step)
            {
                this.channel = channel;
                deltaTime = 0x00;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x54; // 0x41 => On / Off
                midiArg2 = step;

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvPortamento :    channel : ", channel, "  duration :", duration, "  step : ", step));
                }
            }
        }
        private class EvReverbOn : AKAOEvent
        {
            public EvReverbOn(byte STATUS_BYTE, byte channel, uint delta = 0x00)
            {
                this.channel = channel;
                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x5B;
                midiArg2 = 0x40; // 127(max value) or 40(default value)
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvReverbOn"));
                }
            }
        }
        private class EvReverbOff : AKAOEvent
        {
            public EvReverbOff(byte STATUS_BYTE, byte channel, uint delta = 0x00)
            {
                this.channel = channel;
                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x5B;
                midiArg2 = 0; // reset reverb to 0
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvReverbOff"));
                }
            }
        }
        private class EvReverbLevel : AKAOEvent
        {
            /*
             * Effect 1 (Reverb Send Level) (Controller Number 91)
             * Status           2nd bytes               3rd byte
             * BnH              5BH                     vvH
             * n = MIDI channel number: 0H–FH (ch.1–ch.16)
             * vv = Control value :     00H–7FH (0–127), Initial Value = 28H (40)
             * *    This message adjusts the Reverb Send Level of each Part.
             */
            public EvReverbLevel(byte STATUS_BYTE, byte channel, byte v1, byte v2, uint delta = 0x00)
            {
                this.channel = channel;
                deltaTime = delta;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x5B;
                midiArg2 = v2;
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvReverbLevel : ", v1, ", ", v2, "   channel : ", channel, "    delta : ", delta));
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
        #endregion

        #region MIDI Meta Events


        private class EvTrackName : AKAOEvent
        {
            public EvTrackName(string trackName)
            {
                meta = true;
                deltaTime = 0x00;
                midiStatusByte = 0xFF;
                midiArg1 = 0x03;
                tail = new XString(trackName).Bytes.ToArray();
            }
        }
        private class EvMarker : AKAOEvent
        {
            public EvMarker(byte STATUS_BYTE, byte v1, byte v2)
            {
                meta = true;
                deltaTime = 0x00;
                midiStatusByte = 0xFF;
                midiArg1 = 0x06;
                tail = new XString(string.Concat("Marker ", v1)).Bytes.ToArray();
                // v2 is probably the channel, but markers are usually set in the first track / channel
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvMarker : ", v1, ", ", v2));
                }
            }
        }
        private class EvEndTrack : AKAOEvent
        {
            public EvEndTrack(byte STATUS_BYTE, uint trackId, uint delta = 0x00, bool bigEnd = false)
            {
                meta = true;
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvEndTrack : " + trackId, "    EOF : ", bigEnd));
                    Debug.LogWarning("|------------------------------------------------------------------------------------------------");
                }

            }
        }

        private class EvTempo : AKAOEvent
        {
            public EvTempo(byte STATUS_BYTE, byte val1, byte val2, uint t)
            {
                meta = true;
                double tempo = ((val2 << 8) + val1) / 218.4555555555555555555555555;
                uint microSecs = (uint)Math.Round(60000000 / tempo);
                deltaTime = t;
                midiStatusByte = 0xFF;
                midiArg1 = 0x51;
                midiArg2 = 0x03;
                tail = new byte[] { (byte)((microSecs & 0xFF0000) >> 16), (byte)((microSecs & 0x00FF00) >> 8), (byte)(microSecs & 0x0000FF) };
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvTempo : ", microSecs, "    delta : ", t));
                }
            }
        }
        private class EvTimeSign : AKAOEvent
        {

            public EvTimeSign(byte STATUS_BYTE, uint num, uint denom)
            {
                meta = true;
                uint _num = num;
                double _denom = Math.Round(Math.Log((double)(denom / 0.69314718055994530941723212145818)));
                //byte _clocks = 0x20;
                //byte _quart = 0x08;

                /*
                deltaTime = 0x00;
                midiStatusByte = 0xFF;
                midiArg1 = 0x58;
                midiArg2 = 0x04;
                tail = new byte[] { (byte)_num, (byte)_denom, _clocks, _quart };
                */

                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvTimeSign : ", num, ", ", denom));
                }
            }
        }
        #endregion









        private class EvUnknown : AKAOEvent
        {
            public EvUnknown(byte STATUS_BYTE)
            {
                if (UseDebug)
                {
                    Debug.LogError(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvUnknown : ", STATUS_BYTE));
                }
            }
            public EvUnknown(byte STATUS_BYTE, byte v)
            {
                if (UseDebug)
                {
                    Debug.LogError(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvUnknown : ", STATUS_BYTE, ", ", v));
                }
            }
            public EvUnknown(byte STATUS_BYTE, byte[] bytes)
            {
                if (UseDebug)
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





        #region Non MIDI EVENTS

        private class EvDeltaplane : AKAOEvent
        {
            public EvDeltaplane(byte STATUS_BYTE, uint duration)
            {
                if (UseDebugFull)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvDeltaplane : " + duration));
                }
            }
        }

        private class EvTieTime : AKAOEvent
        {
            public EvTieTime(byte STATUS_BYTE, uint value)
            {
                if (UseDebugFull)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvTieTime : " + value));
                }
            }
        }
        private class EvRest : AKAOEvent
        {
            public EvRest(byte STATUS_BYTE, uint duration)
            {
                if (UseDebugFull)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvRest : " + duration));
                }
            }
        }
        private class EvOctave : AKAOEvent
        {
            public EvOctave(byte STATUS_BYTE, uint octave)
            {
                if (UseDebugFull)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvOctave : ", octave));
                }
            }
        }

        private class EvOctaveUp : AKAOEvent
        {
            public EvOctaveUp(byte STATUS_BYTE)
            {
                if (UseDebugFull)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvOctaveUp"));
                }
            }
        }

        private class EvOctaveDown : AKAOEvent
        {
            public EvOctaveDown(byte STATUS_BYTE)
            {
                if (UseDebugFull)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvOctaveDown"));
                }
            }
        }


        private class EvRepeatStart : AKAOEvent
        {
            long position;

            public EvRepeatStart(byte STATUS_BYTE, long _pos)
            {
                long position = _pos;
                if (UseDebugFull)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvRepeatStart"));
                    Debug.Log(string.Concat("<<<<-----------------------------------------VV---------------------------------------------->>>>"));
                }
            }
        }
        private class EvRepeatEnd : AKAOEvent
        {
            public EvRepeatEnd(byte STATUS_BYTE, long offset)
            {
                if (UseDebugFull)
                {
                    Debug.Log(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvRepeatEnd  to : ", offset));
                }
            }

            public EvRepeatEnd(byte STATUS_BYTE, long offset, int loopId)
            {
                if (UseDebugFull)
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
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvPermaLoopBreak : ", dest, "   condition : ", cond));
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
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvPermaLoop : ", dest));
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

        private class EvSubRoutine : AKAOEvent
        {
            public EvSubRoutine(byte STATUS_BYTE, byte[] b)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvSubRoutine : ", BitConverter.ToString(b)));
                }
            }
        }
        private class EvEndSubRoutine : AKAOEvent
        {
            public EvEndSubRoutine(byte STATUS_BYTE, byte b)
            {
                if (AKAOComposer.UseDebug)
                {
                    Debug.LogWarning(string.Concat("0x", BitConverter.ToString(new byte[] { STATUS_BYTE }), "  ->  EvEndSubRoutine : ", b));
                }
            }
        }
        #endregion


    }

}
