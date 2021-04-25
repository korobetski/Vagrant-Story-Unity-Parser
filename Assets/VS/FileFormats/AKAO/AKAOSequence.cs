using FileFormats;
using Scripts.FileFormats.MIDI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VS.Core;
using VS.Utils;

namespace VS.FileFormats.AKAO
{
    public class AKAOSequence : ScriptableObject
    {
        public static readonly ushort[] DELTA_TIME_TABLE = { 192, 96, 48, 24, 12, 6, 3, 32, 16, 8, 4 };
        public string Filename;

        public ushort id;
        public ushort length;
        public Enums.AKAO.Reverb reverb;
        //private byte[] padx6;

        public uint unk1;
        public uint sampleCollectionId;
        //private byte[] padx8;

        public int bitwiseNumTracks;
        public short unk2;
        //private short unk3;
        public short unk4;
        //private byte[] padx6_2;

        public uint ptrInstruments;
        public uint ptrDrums;
        //private byte[] padx8_2;

        public ushort ptrTracksLength;

        public uint[] ptrTracks;
        public AKAOTrack[] tracks;
        public AKAOInstrument[] instruments;




        public uint numTracks { get => ToolBox.GetNumPositiveBits(bitwiseNumTracks); }

        public List<byte> programs
        {
            get
            {
                List<byte> progs = new List<byte>();
                foreach (AKAOTrack trk in tracks)
                {
                    foreach (byte prg in trk.programs)
                    {
                        if (!progs.Contains(prg)) progs.Add(prg);
                    }
                }
                return progs;
            }
        }



        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            Filename = fp.FileName;
            ParseFromBuffer(fp.buffer, fp.FileSize);

            fp.Close();
        }

        public void ParseFromBuffer(BinaryReader buffer, long limit)
        {
            if (buffer.BaseStream.Position + buffer.BaseStream.Length < 4)
            {
                return;
            }

            byte[] header = buffer.ReadBytes(4);       // AKAO
            if (!AKAO.CheckHeader(header))
            {
                return;
            }


            id = buffer.ReadUInt16();
            length = buffer.ReadUInt16();
            reverb = (Enums.AKAO.Reverb) buffer.ReadUInt16(); // 0x0500  | Just on case 0x0400 (MUSIC000.DAT)
            buffer.ReadBytes(6); //padx6

            unk1 = buffer.ReadUInt32(); // never > 127, maybe a general volume ? or something like that
            sampleCollectionId = buffer.ReadUInt32(); // ID of the WAVE*.DAT in the SOUND folder
            buffer.ReadBytes(8);//padx8

            bitwiseNumTracks = buffer.ReadInt32();
            unk2 = buffer.ReadInt16(); // (0, -1, 255 or 16383) from MUSIC050 to MUSIC101 => unk3 != 0
                                       // when != 0 it seems like it's not a "music" but more like a sounds store for maps ambiance or monsters
                                       // when != 0, in most of case there is no instruments set nor a drum (excp 68, 69) and Unknowns AKAO events occur a lot.
                                       // in these cases you really feal that it doesn't really make sence to out a midi file and less a wav...
                                       // when != 0 => sampleSet 17 to 25
                                       // Case 255 (66 to 73, 82, 83, 96, 97)
                                       // Case 16383 (78 to 81, 88 to 91)
                                       // Case -1 all others from 50 to 101
            buffer.ReadInt16(); //unk3 = always 0
            unk4 = buffer.ReadInt16();
            buffer.ReadBytes(6); //padx6 = padding

            ptrInstruments = buffer.ReadUInt32() + 0x30; // instruments pointer
            ptrDrums = buffer.ReadUInt32() + 0x34; // Drums pointer
            buffer.ReadBytes(8); //padx8 = padding

            ptrTracksLength = buffer.ReadUInt16(); // we can alos deduce numtracks with this value / 2

            ptrTracks = new uint[numTracks+1];
            ptrTracks[0] = (uint)buffer.BaseStream.Position + ptrTracksLength - 2;

            for (uint i = 1; i < numTracks; i++)
            {
                ptrTracks[i] = (ushort)(buffer.BaseStream.Position + buffer.ReadUInt16());
            }

            uint endTracks;
            if (ptrInstruments > 0x30)
            {
                endTracks = ptrInstruments;
            }
            else if (ptrDrums > 0x34)
            {
                endTracks = ptrDrums;
            }
            else
            {
                endTracks = (uint)buffer.BaseStream.Length;
            }

            ptrTracks[numTracks] = endTracks;


            // tracks operations here
            tracks = new AKAOTrack[numTracks];
            for (int i = 0; i < numTracks; i++)
            {
                int trackLength = (int)ptrTracks[i + 1] - (int)ptrTracks[i];
                if (trackLength < 0) trackLength = 0; 
                tracks[i] = new AKAOTrack();
                tracks[i].SetDatas(buffer.ReadBytes(trackLength), ptrTracks[i]);
            }


            byte instrCount;
            // Instruments
            if (ptrInstruments > 0x30)
            {
                buffer.BaseStream.Position = ptrInstruments;
                // Instruments Header always 0x20 ?
                List<ushort> instrPtrs = new List<ushort>();
                for (int i = 0; i < 0x10; i++)
                {
                    ushort instrPtr = buffer.ReadUInt16();
                    if (instrPtr != 0xFFFF)
                    {
                        instrPtrs.Add(instrPtr);
                    }
                    else
                    {
                        // Padding
                    }
                }

                instrCount = (byte)instrPtrs.Count;
                if (ptrDrums > 0x34)
                {
                    instrCount++;
                }

                instruments = new AKAOInstrument[instrPtrs.Count];
                for (int i = 0; i < instrPtrs.Count; i++)
                {
                    AKAOInstrument instrument = new AKAOInstrument((byte)i);
                    instrument.name = "Instrument #" + (ushort)i;
                    long instrStart = ptrInstruments + 0x20 + instrPtrs[i];
                    long instrEnd;
                    if (i < instrPtrs.Count - 1)
                    {
                        instrEnd = ptrInstruments + 0x20 + instrPtrs[i + 1];
                    }
                    else
                    {
                        if (ptrDrums > 0x34)
                        {
                            instrEnd = ptrDrums;
                        }
                        else
                        {
                            instrEnd = length;
                        }
                    }
                    int instrRegLoop = (int)(instrEnd - instrStart) / 0x08;

                    instrument.regions = new AKAORegion[instrRegLoop - 1]; // -1 because the last 8 bytes are padding
                    for (int j = 0; j < instrRegLoop - 1; j++)
                    {
                        AKAORegion reg = new AKAORegion();
                        reg.FeedMelodic(buffer.ReadBytes(8));
                        instrument.regions[j] = reg;
                    }
                    buffer.ReadBytes(8);// 0000 0000 0000 0000 padding

                    instruments[i] = (instrument);
                }

                // Drum
                if (ptrDrums > 0x34)
                {
                    if (buffer.BaseStream.Position != ptrDrums)
                    {
                        buffer.BaseStream.Position = ptrDrums;
                    }

                    // Special case when there is no melodic instruments
                    if (instruments == null)
                    {
                        instrCount++;
                        instruments = new AKAOInstrument[1];
                    }

                    AKAOInstrument drum = new AKAOInstrument((byte)(instrCount - 1), true);
                    drum.name = "Drum";
                    int drumRegLoop = (int)(length - ptrDrums) / 0x08;

                    List<AKAORegion> dr = new List<AKAORegion>();
                    for (int j = 0; j < drumRegLoop - 1; j++)
                    {
                        byte[] b = buffer.ReadBytes(8);
                        if (b[0] == 0xFF && b[1] == 0xFF && b[2] == 0xFF && b[3] == 0xFF && b[4] == 0xFF && b[5] == 0xFF && b[6] == 0xFF && b[7] == 0xFF)
                        {
                            break;
                        }
                        if (b[0] > 0 && b[1] > 0 && b[6] > 0 && b[7] > 0)
                        {
                            AKAORegion dregion = new AKAORegion();
                            dregion.FeedDrum(b, j);
                            dr.Add(dregion);
                        }
                    }

                    drum.regions = dr.ToArray();
                    instruments[0] = drum;
                }

            }

            if (unk2 == 0) ConvertToMidi();
        }

        public void ConvertToMidi(bool produceSF2 = true)
        {
            List<byte> A1PC = new List<byte>();


            // MIDI formats can handle 15 voice channels + 1 drum channel (9)
            // for now we create a new midi channel per AKAO track
            // we will merge channels with the same instrument later
            List<MIDIChannel> channels = new List<MIDIChannel>();
            for (uint t = 0; t < numTracks; t++)
            {
                AKAOTrack track = tracks[t];
                uint trackPtr = ptrTracks[t];

                MIDIChannel channel = new MIDIChannel(0);
                channel.id = t;
                channel.programs = track.programs;
                
                channels.Add(channel);

                int repeatIndex = -1;
                int repeatNumber = 0;
                List<long> repeaterStartPositions = new List<long>();
                List<long> repeaterEndPositions = new List<long>();

                bool keyPlaying = false;
                byte currentKey = 0;
                uint delta = 0;
                byte octave = 0;
                byte velocity = 0x80;

                void NoteOff()
                {
                    if (keyPlaying == true)
                    {
                        channel.AddEvent(MIDIEvent.NoteOff(delta, 0, currentKey, velocity));
                        delta = 0;
                        currentKey = 0;
                        keyPlaying = false;
                    }
                }

                for (uint e = 0; e < track.operations.Length; e++)
                {
                    void BackToStart(byte inc = 2)
                    {
                        if (!repeaterEndPositions.Contains(e))
                        {
                            repeaterEndPositions.Add(e);
                            repeatNumber = inc;
                        }

                        if (repeatNumber >= 2 && repeaterStartPositions.Count > repeatIndex)
                        {
                            e = (uint)repeaterStartPositions[repeatIndex];
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
                    }

                    AKAOP op = track.operations[e];
                    uint eventPtr = op.adr;
                    switch (op.name)
                    {
                        case "Note On":
                            NoteOff();
                            int relativeKey = op.op / 11;
                            uint length = (uint) op.op % 11;
                            byte key = (byte)(octave * 12 + relativeKey);
                            channel.AddEvent(MIDIEvent.NoteOn(delta, 0, key, velocity));
                            delta = DELTA_TIME_TABLE[length];
                            keyPlaying = true;
                            currentKey = key;
                            break;
                        case "Note On *":
                            NoteOff();
                            relativeKey = op.op - 0xF0;
                            length = op.parameters[0];
                            key = (byte)(octave * 12 + relativeKey);
                            channel.AddEvent(MIDIEvent.NoteOn(delta, 0, key, velocity));
                            delta = length;
                            keyPlaying = true;
                            currentKey = key;
                            break;
                        case "Tie":
                            length = (uint)op.op % 11;
                            delta += DELTA_TIME_TABLE[length];
                            break;
                        case "Tie *":
                            length = op.parameters[0];
                            delta += length;
                            break;
                        case "Rest":
                            NoteOff();
                            length = (uint) op.op % 11;
                            delta += DELTA_TIME_TABLE[length];
                            break;
                        case "Rest *":
                            NoteOff();
                            length = op.parameters[0];
                            delta += length;
                            break;
                        case "End Track":
                            NoteOff();
                            //channel.AddEvent(MIDIEvent.MetaEndTrack());
                            delta = 0;
                            break;
                        case "Program Change(A1)":
                            byte program = op.parameters[0];
                            if (!A1PC.Contains(program)) A1PC.Add(program);
                            channel.AddEvent(MIDIEvent.MetaTrackName(string.Concat("Instrument ", program)));
                            channel.AddEvent(MIDIEvent.ProgramChange(delta, 0, program));
                            delta = 0;
                            break;
                        case "Delta":
                            length = op.parameters[0];
                            delta = length;
                            break;
                        case "Volume":
                            byte volume = op.parameters[0];
                            channel.AddEvent(MIDIEvent.ControllerVolume(delta, 0, volume));
                            delta = 0;
                            break;
                        case "Portamento":
                            break;
                        case "Octave":
                            octave = op.parameters[0];
                            break;
                        case "Increase Octave":
                            octave++;
                            break;
                        case "Decrease Octave":
                            octave--;
                            break;
                        case "Expression":
                            // we use expression to set next notes velocity instead
                            // channel volume will only be controlled by op Volume
                            byte expression = op.parameters[0];
                            velocity = expression;
                            //channel.AddEvent(MIDIEvent.ControllerExpression(delta, 0, expression));
                            break;
                        case "Expression Slide":
                            break;
                        case "Pan":
                            byte pan = op.parameters[0];
                            if (channel.pan == null) channel.pan = pan; 
                            channel.AddEvent(MIDIEvent.ControllerPan(delta, 0, pan));
                            delta = 0;
                            break;
                        case "Pan Slide":
                            break;
                        case "Reverb On":
                            channel.AddEvent(MIDIEvent.ControllerReverb(delta, 0, 0x80));
                            delta = 0;
                            break;
                        case "Reverb Off":
                            channel.AddEvent(MIDIEvent.ControllerReverb(delta, 0, 0x00));
                            delta = 0;
                            break;
                        case "Reverb Depth":
                            // the first parameter seems to be always 0x00
                            channel.AddEvent(MIDIEvent.ControllerReverb(delta, 0, op.parameters[1]));
                            delta = 0;
                            break;

                        case "Loop Start":
                            repeaterStartPositions.Add(e);
                            repeatIndex++;
                            break;
                        case "Loop Return X":
                            byte numLoop = op.parameters[0];
                            BackToStart(numLoop);
                            break;
                        case "Loop Return":
                            BackToStart();
                            break;

                        case "Tempo":
                            ushort tempo = BitConverter.ToUInt16(op.parameters, 0);
                            channel.AddEvent(MIDIEvent.MetaTempo(delta, tempo));
                            delta = 0;
                            break;
                        case "Drum Mode On":
                            channel.drum = true;
                            break;
                        case "Drum Mode Off":
                            break;
                        case "Jump To":
                            break;
                        case "Conditinal Jump":
                            break;
                        case "Loop Jump":
                            break;
                        case "Loop Break":
                            break;
                        case "Program Change * ":
                            program = op.parameters[0];
                            channel.AddEvent(MIDIEvent.MetaTrackName(string.Concat("Instrument ", program)));
                            channel.AddEvent(MIDIEvent.ProgramChange(delta, 0, program));
                            delta = 0;
                            break;
                        case "Time Signature":
                            break;
                        case "Measure Number":
                            channel.AddEvent(MIDIEvent.MetaMarker(op.parameters[0], op.parameters[1]));
                            break;
                    }
                }
            }

            MIDI midi = new MIDI();
            midi.MergeChannels(channels);
            midi.SaveAs(Filename);

            if (produceSF2)
            {
                AKAO.BuildSoundFont(this, AKAO.GetAKAOSampleCollections(this, A1PC));
            }
        }
    }

    [CustomEditor(typeof(AKAOSequence))]
    public class AKAOSequenceEditor : Editor
    {
        bool sf2Trigger = true;
        public override void OnInspectorGUI()
        {
            var akaoSeq = target as AKAOSequence;
            DrawDefaultInspector();
            sf2Trigger = GUILayout.Toggle(sf2Trigger, new GUIContent("Produce a SF2 (soundfont) file ?"));
            if (GUILayout.Button("Export as MIDI File"))
            {
                akaoSeq.ConvertToMidi(sf2Trigger);
            }
        }
    }
}
