using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VS.Utils;

namespace VS.FileFormats.AKAO
{
    public class AKAOSequence : ScriptableObject
    {
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

            uint endTracks = 0;
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
                tracks[i].SetDatas(buffer.ReadBytes(trackLength));
            }


            uint instrCount = 0;
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

                instrCount = (uint)instrPtrs.Count;
                if (ptrDrums > 0x34)
                {
                    instrCount++;
                }

                instruments = new AKAOInstrument[instrPtrs.Count];
                for (int i = 0; i < instrPtrs.Count; i++)
                {
                    AKAOInstrument instrument = new AKAOInstrument((uint)i);
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

                    AKAOInstrument drum = new AKAOInstrument(instrCount - 1, true);
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
        }
    }
}
