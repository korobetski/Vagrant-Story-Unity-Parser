using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Format;
using VS.Parser.Akao;
using VS.Utils;


// Minoru Akao Audio Script Format (Sequencer, SoundFont and more)

// Akao in MUSIC folder contains music instructions like a Midi file, Akao in SOUND folder contains samples collection like .sfb / .sf2 / .dls files
//http://problemkaputt.de/psx-spx.htm
namespace VS.Parser
{

    public class AKAO : FileParser
    {
        public enum AKAOType { UNKNOWN, SOUND, MUSIC, PROG, SAMPLE }

        public static readonly AKAOType UNKNOWN = AKAOType.UNKNOWN;
        public static readonly AKAOType SOUND = AKAOType.SOUND;
        public static readonly AKAOType MUSIC = AKAOType.MUSIC;
        public static readonly AKAOType PROG = AKAOType.PROG;
        public static readonly AKAOType SAMPLE = AKAOType.SAMPLE;


        public static bool CheckHeader(byte[] bytes)
        {
            //41 4B 41 4F
            //Debug.Log(BitConverter.ToString(bytes));
            if (bytes[0] == 0x41 && bytes[1] == 0x4B && bytes[2] == 0x41 && bytes[3] == 0x4F)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public AKAOType _type;
        public List<AKAOInstrument> instruments;
        public AKAOArticulation[] articulations;
        public AKAOSample[] samples;
        public AKAOComposer composer;

        public uint startingArticulationId;
        public bool bMID;
        public bool bSF2;
        public bool bDLS;
        public bool bWAV;

        public void Parse(string filePath, AKAOType type, long limit = long.MaxValue)
        {
            PreParse(filePath);
            _type = type;
            if (FileSize < 4)
            {
                return;
            }
            Parse(buffer, type, limit);

            buffer.Close();
            fileStream.Close();
        }

        public void Parse(BinaryReader buffer, AKAOType type, long limit = long.MaxValue)
        {
            _type = type;
            if (buffer.BaseStream.Length < 4)
            {
                return;
            }

            if (_type == AKAOType.UNKNOWN)
            {
                // we must try to find the AKAO type
                byte[] header = buffer.ReadBytes(4);       // AKAO
                if (!CheckHeader(header))
                {
                    return;
                }
                ushort v1 = buffer.ReadUInt16();    // ID or type
                ushort v2 = buffer.ReadUInt16();    // File Length or empty
                byte v3 = buffer.ReadByte();    // Type or empty
                byte v4 = buffer.ReadByte();    // Type var or empty

                if (v2 + v3 + v4 == 0)
                {
                    if (v1 == 0)
                    {
                        _type = AKAOType.SAMPLE;
                    }
                    else
                    {
                        if (buffer.BaseStream.Position == 10)
                        {
                            // v1 is the sample collection ID in this case
                            _type = AKAOType.SOUND;
                        }
                        else
                        {
                            // E075.P have an AKAO PROG without v3 = 0xC8...
                            _type = AKAOType.PROG;
                        }
                    }
                }
                else if (v3 == 0xC8)
                {
                    _type = AKAOType.PROG;
                }
                else if (v2 > 0 && v2 == buffer.BaseStream.Length)
                {
                    _type = AKAOType.MUSIC;
                }

                buffer.BaseStream.Position -= 10;
            }


            switch (_type)
            {
                case AKAOType.MUSIC:
                    //https://github.com/vgmtrans/vgmtrans/blob/master/src/main/formats/AkaoSeq.cpp
                    // header datas
                    byte[] header = buffer.ReadBytes(4);// AKAO
                    ushort fileId = buffer.ReadUInt16();
                    ushort byteLen = buffer.ReadUInt16();
                    ushort reverb = buffer.ReadUInt16(); // 0x0500  | Just on case 0x0400 (MUSIC000.DAT), maybe it refer to the WAVE0005.DAT sample collection
                    buffer.ReadBytes(6); // padding

                    uint unk1 = buffer.ReadUInt32(); // never > 127, maybe a general volume ? or something like that
                    uint sampleSet = buffer.ReadUInt32(); // ID of the WAVE*.DAT in the SOUND folder
                    buffer.ReadBytes(8); // padding

                    int bnumt = buffer.ReadInt32();
                    uint numTrack = ToolBox.GetNumPositiveBits(bnumt);
                    short unk3 = buffer.ReadInt16(); // (0, -1, 255 or 16383) from MUSIC050 to MUSIC101 => unk3 != 0
                    // when != 0 it seems like it's not a "music" but more like a sounds store for maps ambiance or monsters
                    // when != 0, in most of case there is no instruments set nor a drum (excp 68, 69) and Unknowns AKAO events occur a lot.
                    // in these cases you really feal that it doesn't really make sence to out a midi file and less a wav...
                    // when != 0 => sampleSet 17 to 25
                    // Case 255 (66 to 73, 82, 83, 96, 97)
                    // Case 16383 (78 to 81, 88 to 91)
                    // Case -1 all others from 50 to 101
                    buffer.ReadBytes(10); // padding

                    uint ptr1 = buffer.ReadUInt32() + 0x30; // instruments pointer
                    uint ptr2 = buffer.ReadUInt32() + 0x34; // Drums pointer
                    buffer.ReadBytes(8); // padding

                    ushort jump = buffer.ReadUInt16();
                    long basePtr = buffer.BaseStream.Position;
                    long musInstrPtr = buffer.BaseStream.Position + jump - 2;


                    if (true)
                    {
                        Debug.Log(string.Concat("AKAO from : ", FileName, " FileSize = ", FileSize, "    |    reverb : ", reverb, " numTrack : ", numTrack, " sampleSet : ", sampleSet,
                            "\r\ninstruments at : ", ptr1, "  Drums at : ", ptr2, "   musInstrPtr : ", musInstrPtr, "   |  unk1 : ", unk1, "   unk3 : ", unk3));
                    }
                    ushort[] tracksPtr = new ushort[numTrack];
                    tracksPtr[0] = (ushort)musInstrPtr;

                    for (uint i = 0; i < numTrack-1; i++)
                    {
                        tracksPtr[i+1] = (ushort)((basePtr + i * 2)+ buffer.ReadUInt16());
                    }




                    // music instuctions begin here, MIDI like format, we don't care yet, so let's jump



                    uint instrCount = 0;
                    // Instruments
                    if (ptr1 > 0x30)
                    {
                        buffer.BaseStream.Position = ptr1;
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
                        if (ptr2 > 0x34)
                        {
                            instrCount++;
                        }



                        if (UseDebug)
                        {
                            Debug.Log("Instruments number : " + instrCount);
                        }

                        instruments = new List<AKAOInstrument>();

                        for (int i = 0; i < instrPtrs.Count; i++)
                        {
                            AKAOInstrument instrument = new AKAOInstrument((uint)i, AKAOInstrument.InstrumentType.INSTR_MELODIC);
                            instrument.name = "Instrument #" + (ushort)i;
                            long instrStart = ptr1 + 0x20 + instrPtrs[i];
                            long instrEnd;
                            if (i < instrPtrs.Count - 1)
                            {
                                instrEnd = ptr1 + 0x20 + instrPtrs[i + 1];
                            }
                            else
                            {
                                if (ptr2 > 0x34)
                                {
                                    instrEnd = ptr2;
                                }
                                else
                                {
                                    instrEnd = byteLen;
                                }
                            }
                            int instrRegLoop = (int)(instrEnd - instrStart) / 0x08;
                            if (UseDebug)
                            {
                                Debug.Log(string.Concat("Instrument #", i, "   Regions count : ", instrRegLoop - 1));
                            }

                            instrument.regions = new AKAORegion[instrRegLoop - 1]; // -1 because the last 8 bytes are padding
                            for (int j = 0; j < instrRegLoop - 1; j++)
                            {
                                AKAORegion reg = new AKAORegion();
                                reg.FeedMelodic(buffer.ReadBytes(8));
                                instrument.regions[j] = reg;
                                if (UseDebug)
                                {
                                    Debug.Log(reg.ToString());
                                }
                            }
                            buffer.ReadBytes(8);// 0000 0000 0000 0000 padding

                            instruments.Add(instrument);
                        }
                    }

                    // Drum
                    if (ptr2 > 0x34)
                    {
                        if (buffer.BaseStream.Position != ptr2)
                        {
                            buffer.BaseStream.Position = ptr2;
                        }

                        // Special case when there is no melodic instruments
                        if (instruments == null)
                        {
                            instrCount++;
                            instruments = new List<AKAOInstrument>();
                        }

                        AKAOInstrument drum = new AKAOInstrument(instrCount - 1, AKAOInstrument.InstrumentType.INSTR_DRUM);
                        drum.name = "Drum";
                        int drumRegLoop = (int)(byteLen - ptr2) / 0x08;
                        if (UseDebug)
                        {
                            Debug.Log(string.Concat("Drum   Regions count : ", drumRegLoop - 1));
                        }

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
                                if (UseDebug)
                                {
                                    Debug.Log(dregion.ToString());
                                }
                            }
                        }

                        drum.regions = dr.ToArray();
                        instruments.Add(drum);
                    }

                    long end = 0;
                    if (ptr1 > 0x30)
                    {
                        end = ptr1;
                    }
                    else if (ptr2 > 0x34)
                    {
                        end = ptr2;
                    }
                    else
                    {
                        end = buffer.BaseStream.Length;
                    }

                    composer = new AKAOComposer(buffer, musInstrPtr, end, instrCount, numTrack, tracksPtr, FileName, true);


                    // AKAO from : WAVE0000 startingArticulationId = 0
                    // AKAO from : WAVE0005 startingArticulationId = 32
                    // AKAO from : WAVE0032 startingArticulationId = 32
                    // All other            startingArticulationId = 64
                    // AKAO from : WAVE0200 startingArticulationId = 128

                    // So we seek for the appropriate WAVE*.DAT in the SOUND folder
                    string[] hash = FilePath.Split("/"[0]);
                    hash[hash.Length - 2] = "SOUND";

                    AKAO[] sampleCollections = new AKAO[3];

                    // Program from 32 to 63
                    hash[hash.Length - 1] = "WAVE0005.DAT"; // wave 005 or wave 032 ? (5 seems good)
                    AKAO SampleColl32 = new AKAO();
                    SampleColl32.UseDebug = UseDebug;
                    SampleColl32.Parse(String.Join("/", hash), AKAO.SOUND);
                    sampleCollections[0] = SampleColl32;

                    string zz = "0";
                    if (sampleSet < 100)
                    {
                        zz += "0";
                    }
                    if (sampleSet < 10)
                    {
                        zz += "0";
                    }
                    hash[hash.Length - 1] = "WAVE" + zz + sampleSet + ".DAT";
                    string samplePath = String.Join("/", hash);
                    bool test = File.Exists(samplePath);

                    // Program from 64 to 95 or 127
                    AKAO SampleColl64 = new AKAO();
                    SampleColl64.UseDebug = UseDebug;
                    SampleColl64.Parse(samplePath, AKAO.SOUND);
                    sampleCollections[1] = SampleColl64;

                    // Additionnal Collection, somztimes usefull for drum kit or A1 program change
                    if (SampleColl64.articulations.Length < 64)
                    {
                        hash[hash.Length - 1] = "WAVE0069.DAT";
                        AKAO addiColl = new AKAO();
                        addiColl.UseDebug = UseDebug;
                        addiColl.Parse(String.Join("/", hash), AKAO.SOUND);
                        sampleCollections[2] = addiColl;
                    }

                    if (composer.A1Calls.Count > 0)
                    {
                        // we need to add new instruments with an unique region
                        foreach (uint iid in composer.A1Calls)
                        {
                            AKAOInstrument A1Instrument = new AKAOInstrument(iid);
                            A1Instrument.name = "A1 Instrument #" + (ushort)iid;
                            A1Instrument.a1 = true;
                            A1Instrument.regions = new AKAORegion[1];
                            AKAORegion defaultRegion = new AKAORegion();
                            defaultRegion.articulationId = (byte)iid;
                            A1Instrument.regions[0] = defaultRegion;
                            if (instruments == null)
                            {
                                instruments = new List<AKAOInstrument>();
                            }
                            instruments.Add(A1Instrument);
                        }
                    }




                    SF2 sf2 = null;
                    if (bWAV || bSF2 || bDLS)
                    {
                        sf2 = SoundFundry(this, sampleCollections);
                    }
                    if (bMID || bWAV)
                    {
                        if (unk3 != 0)
                        {
                            bWAV = false; // we don't want 1Gb crap .wav
                        }

                        composer.Synthetize(bMID, bWAV, sf2);
                    }

                    break;
                case AKAOType.SOUND:
                    // Samples Collection

                    // header datas
                    header = buffer.ReadBytes(4);       // AKAO
                    ushort sampleId = buffer.ReadUInt16();
                    buffer.ReadBytes(10); // padding

                    unk1 = buffer.ReadByte(); // almost always 0 (but one case 48 in WAVE0032)
                    unk3 = buffer.ReadByte(); // almost always 81 (two cases 49 (WAVE0000, WAVE0005), one case 16 in WAVE0032, one case 177 in WAVE0200)
                    buffer.ReadBytes(2); // padding
                    var sampleSize = buffer.ReadUInt32();
                    startingArticulationId = buffer.ReadUInt32();
                    var numArts = buffer.ReadUInt32();
                    // mostly 32, sometimes 64, one case 48 (WAVE0071), one case 96 (WAVE0200)
                    /* List of 64 arts
                     * WAVE0044
                     * WAVE0045
                     * WAVE0046
                     * WAVE0053
                     * WAVE0054
                     * WAVE0055
                     * WAVE0064
                     * WAVE0065
                     * WAVE0068
                     * WAVE0069
                     * WAVE0091
                     * WAVE0097
                     * WAVE0099
                     */

                    buffer.ReadBytes(32); // padding

                    if (UseDebug)
                    {
                        Debug.Log(string.Concat("AKAO from : ", FileName, " len = ", FileSize, "  ID : ", sampleId, " unk1 : ", unk1, " unk3 : ", unk3, " sampleSize : ", sampleSize, " stArtId : ", startingArticulationId, " numArts : ", numArts));
                    }

                    // Articulations section here
                    articulations = new AKAOArticulation[numArts];
                    for (uint i = 0; i < numArts; i++)
                    {
                        AKAOArticulation arti = new AKAOArticulation(buffer, startingArticulationId + i);
                        articulations[i] = arti;
                        if (UseDebug)
                        {
                            //Debug.Log(arti.ToString());
                        }
                    }

                    // Samples section here
                    ulong samStart = (ulong)buffer.BaseStream.Position;
                    // First we need to determine the start and the end of the samples, 16 null bytes indicate a new sample, so lets find them.
                    List<long> samPtr = new List<long>();
                    List<long> samEPtr = new List<long>();
                    while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                    {
                        if (buffer.ReadUInt64() + buffer.ReadUInt64() == 0)
                        {
                            if (samPtr.Count > 0)
                            {
                                //samEPtr.Add(buffer.BaseStream.Position - 0x20); 
                                samEPtr.Add(buffer.BaseStream.Position - 0x10);
                            }
                            //samPtr.Add(buffer.BaseStream.Position - 0x10);
                            samPtr.Add(buffer.BaseStream.Position);
                        }
                    }
                    samEPtr.Add(buffer.BaseStream.Length);

                    // Let's loop again to get samples
                    int numSam = samPtr.Count;
                    samples = new AKAOSample[numSam];
                    for (int i = 0; i < numSam; i++)
                    {
                        buffer.BaseStream.Position = samPtr[i];
                        int size = (int)(samEPtr[i] - samPtr[i]);
                        byte[] dt = buffer.ReadBytes(size);
                        AKAOSample sam = new AKAOSample(string.Concat(FileName, " Sample #", (ushort)i), dt, (ulong)samPtr[i]);
                        sam.index = i;
                        samples[i] = sam;

                        if (UseDebug && bWAV)
                        {
                            WAV wavSam = sam.ConvertToWAV();
                            wavSam.SetName(FileName + "_Sample_" + i);
                            ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Sounds/SampleColl/");
                            wavSam.WriteFile(Application.dataPath + "/../Assets/Resources/Sounds/SampleColl/" + FileName + "_Sample_" + i + ".wav", wavSam.Write());
                        }

                    }
                    // now to verify and associate each articulation with a sample index value
                    // for every sample of every instrument, we add sample_section offset, because those values
                    //  are relative to the beginning of the sample section
                    for (uint i = 0; i < articulations.Length; i++)
                    {
                        for (uint l = 0; l < samples.Length; l++)
                        {
                            //if (articulations[i].sampleOff + samStart == samples[l].offset)
                            if (articulations[i].sampleOff + samStart + 0x10 == samples[l].offset)
                            {
                                articulations[i].sampleNum = l;
                                articulations[i].sample = samples[l];
                                samples[l].loopStart = articulations[i].loopPt;

                                break;
                            }
                        }
                    }
                    break;
                case AKAOType.PROG:
                    // similar to AKAOType.MUSIC without instruments & regions, use an AKAOType.SAMPLE as an instrument i supose
                    header = buffer.ReadBytes(4);// AKAO
                    ushort id = buffer.ReadUInt16();
                    buffer.ReadUInt16();
                    ushort tp = buffer.ReadUInt16();
                    buffer.ReadUInt16();
                    buffer.ReadUInt32();

                    buffer.ReadUInt32();
                    buffer.ReadUInt32();
                    buffer.ReadUInt32();
                    buffer.ReadUInt32();

                    buffer.ReadUInt16();
                    byteLen = buffer.ReadUInt16();

                    //Debug.Log(string.Concat("AKAO PROG : id : ", id, "  tp : ", tp, "  byteLen : ", byteLen));
                    //int waveLen = (int)(limit - buffer.BaseStream.Position);

                    composer = new AKAOComposer(buffer, buffer.BaseStream.Position, limit, 0, 1, new ushort[] { (ushort)buffer.BaseStream.Position }, FileName, true);
                    composer.Synthetize(true, false);
                    break;
                case AKAOType.SAMPLE:
                    // similar to AKAOType.SOUND without articulations, we can output a WAV file
                    // header datas
                    header = buffer.ReadBytes(4);       // AKAO
                    buffer.ReadUInt16();
                    buffer.ReadBytes(10); // padding

                    buffer.ReadUInt16(); // 
                    buffer.ReadBytes(2); // padding
                    buffer.ReadUInt32();
                    buffer.ReadUInt32();
                    buffer.ReadUInt32();

                    buffer.ReadBytes(32); // padding
                    buffer.ReadBytes(16); // sample padding

                    AKAOSample sample = new AKAOSample(FileName, buffer.ReadBytes((int)(limit - buffer.BaseStream.Position)), (ulong)buffer.BaseStream.Position);
                    WAV nw = sample.ConvertToWAV();
                    nw.SetName(FileName);

                    ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Sounds/Effects/");
                    nw.WriteFile(Application.dataPath + "/../Assets/Resources/Sounds/Effects/" + FileName + ".wav", nw.Write());

                    break;
            }

        }




        private SF2 SoundFundry(AKAO sequencer, AKAO[] sampleCollections)
        {
            DLS dls = new DLS();
            dls.SetName(FileName + ".dls");
            SF2 sf2 = new SF2();
            sf2.InfoChunk.Bank = "Vagrant Story SoundFont for " + FileName;
            sf2.InfoChunk.Products = "Vagrant Story";
            sf2.InfoChunk.Tools = "https://github.com/korobetski/Vagrant-Story-Unity-Parser";
            sf2.InfoChunk.Designer = "Korobetski Sigfrid";
            sf2.InfoChunk.Date = DateTime.Now.ToString();
            sf2.InfoChunk.Copyright = "Musics & Samples belong to Hitoshi Sakimoto @ Squaresoft";
            sf2.InfoChunk.Comment = string.Concat("This SoundFont was generated by reading raw AKAO format from the original game in SOUND folder.\r",
            "\nNever forget that musics and samples belongs to SquareEnix, don't use them as your own. Sample collections : ",
            sampleCollections[0].FileName, ", ",
            sampleCollections[1].FileName);

            List<AKAOSample> Samples = new List<AKAOSample>();

            // MUSIC024.DAT has no instruments Oo, maybe unfinished work, or development test
            // we use composer program change to load articulations from WAVE000.DAT the only file where start id = 0
            if (sequencer.instruments == null)
            {
                foreach (uint id in sequencer.composer.progIDs)
                {
                    AKAOInstrument instrument = new AKAOInstrument(id, AKAOInstrument.InstrumentType.INSTR_MELODIC);
                    instrument.name = "No instrument " + id;
                    instrument.regions = new AKAORegion[1];
                    AKAORegion defaultRegion = new AKAORegion();
                    defaultRegion.articulationId = (byte)id;
                    instrument.regions[0] = defaultRegion;
                    sequencer.instruments = new List<AKAOInstrument>();
                    sequencer.instruments.Add(instrument);
                }
            }

            if (sequencer.instruments != null)
            {
                uint i = 0;
                foreach (AKAOInstrument instrument in sequencer.instruments)
                {
                    if (composer.progIDs.Contains(instrument.program) || composer.A1Calls.Contains(instrument.program) || instrument.IsDrum())
                    {
                        uint midiBank = 0x00000000;
                        if (instrument.IsDrum())
                        {
                            midiBank = DLS.F_INSTRUMENT_DRUMS;
                            sf2.AddPreset(instrument.name, 0, 128);
                        }
                        else
                        {
                            sf2.AddPreset(instrument.name, (ushort)instrument.program, 0);
                        }

                        sf2.AddPresetBag();
                        sf2.AddPresetGenerator(SF2Generator.ReverbEffectsSend, new SF2GeneratorAmount { UAmount = (ushort)1000 });
                        sf2.AddPresetGenerator(SF2Generator.Instrument, new SF2GeneratorAmount { UAmount = (ushort)i });
                        sf2.AddInstrument(instrument.name);
                        i++;

                        if (instrument.regions.Length > 0)
                        {
                            Lins DSLInstrument = new Lins(midiBank, instrument.program, instrument.name);

                            foreach (AKAORegion region in instrument.regions)
                            {
                                AKAOArticulation articulation = null;
                                AKAO coll = sampleCollections[2];

                                if (region.articulationId >= 0 && region.articulationId < 32)
                                {
                                    // trick for MUSIC024.DAT
                                    coll = sampleCollections[1];
                                    articulation = coll.articulations[region.articulationId];
                                }
                                else if (region.articulationId >= 32 && region.articulationId < 64)
                                {
                                    coll = sampleCollections[0];
                                    articulation = coll.articulations[region.articulationId - coll.startingArticulationId];
                                }
                                else if (region.articulationId >= 64 && region.articulationId < 128)
                                {
                                    coll = sampleCollections[1];
                                    if (region.articulationId - coll.startingArticulationId < coll.articulations.Length/* && !instrument.a1*/)
                                    {
                                        articulation = coll.articulations[region.articulationId - coll.startingArticulationId];
                                    }
                                    else
                                    {
                                        // we check in additional collection
                                        //Debug.LogWarning(region.articulationId);
                                        coll = sampleCollections[2];
                                        articulation = coll.articulations[region.articulationId - coll.startingArticulationId];

                                    }
                                }
                                if (UseDebug)
                                {
                                    Debug.Log(string.Concat("Instrument ", i, "  ", instrument.name, "  |  Region articulation  "+ region.articulationId + "  found in ", coll.FileName));
                                }



                                if (articulation != null)
                                {
                                    articulation.BuildADSR();
                                    region.articulation = articulation;
                                    AKAOSample sample = coll.samples[articulation.sampleNum];

                                    if (instrument.IsDrum())
                                    {
                                        region.unityKey = (uint)articulation.unityKey + region.lowRange - region.relativeKey; // maybe
                                    }
                                    else
                                    {
                                        region.unityKey = articulation.unityKey;
                                    }

                                    short ft = articulation.fineTune;
                                    if (ft < 0)
                                    {
                                        ft += short.MaxValue;
                                    }

                                    double freq_multiplier = ((ft * 32) + 0x100000) / (double)0x100000;
                                    double cents = (short)(1200 * Math.Log(freq_multiplier, 2));
                                    if (articulation.fineTune < 0)
                                    {
                                        cents -= 1200;
                                    }

                                    region.fineTune = (short)cents;
                                    sample.loopStart = articulation.loopPt;
                                    sample.unityKey = (byte)region.unityKey;

                                    if (!Samples.Contains(sample))
                                    {
                                        Samples.Add(sample);
                                    }

                                    int sampleIDX = Samples.IndexOf(sample);


                                    // Making DLS
                                    Lrgn reg = new Lrgn(region.lowRange, region.hiRange, 0x00, 0x7F);
                                    CKwsmp smp = new CKwsmp((ushort)region.unityKey, region.fineTune, region.attenuation, 1);
                                    if (articulation.loopPt != 0)
                                    {
                                        smp.AddLoop(new Loop(1, (uint)(articulation.loopPt * 1.75f), (uint)(sample.size * 1.75f - articulation.loopPt * 1.75f)));
                                    }
                                    reg.SetSample(smp);
                                    CKart2 iart = new CKart2();
                                    iart.AddPan(0x40);
                                    iart.AddADSR(articulation.A, articulation.D, articulation.S, articulation.R, articulation.AT, articulation.RT);
                                    reg.AddArticulation(iart);
                                    reg.SetWaveLinkInfo(0, 0, 1, region.sampleNum);
                                    DSLInstrument.AddRegion(reg);

                                    // http://linuxmao.org/SoundFont+specification+SF2
                                    sf2.AddInstrumentBag();
                                    sf2.AddInstrumentGenerator(SF2Generator.KeyRange, new SF2GeneratorAmount { LowByte = region.lowRange, HighByte = region.hiRange });
                                    sf2.AddInstrumentGenerator(SF2Generator.VelRange, new SF2GeneratorAmount { LowByte = region.lowVel, HighByte = region.hiVel }); // not sure
                                    //sf2.AddInstrumentGenerator(SF2Generator.VelRange, new SF2GeneratorAmount { LowByte = 0, HighByte = 127 });
                                    /* C'est l'atténuation, en centibels, pour laquelle une note est atténuée en dessous de la valeur maximum prévue.
                                    Si = 0, il n'y a aucune atténuation, la note sera jouée au maximum prévu.
                                    Ex : 60 indique que la note sera jouée à 6 dB en-dessous du maximum prévu pour la note.
                                    Max value = 1440 */
                                    sf2.AddInstrumentGenerator(SF2Generator.InitialAttenuation, new SF2GeneratorAmount { UAmount = (ushort)(region.attenuation / 10) });
                                    //sf2.AddInstrumentGenerator(SF2Generator.ReverbEffectsSend, new SF2GeneratorAmount { Amount = 1000 });
                                    sf2.AddInstrumentGenerator(SF2Generator.Pan, new SF2GeneratorAmount { Amount = region.pan });
                                    sf2.AddInstrumentGenerator(SF2Generator.SampleModes, new SF2GeneratorAmount { UAmount = (articulation.loopPt != 0) ? (ushort)1 : (ushort)0 });
                                    sf2.AddInstrumentGenerator(SF2Generator.OverridingRootKey, new SF2GeneratorAmount { UAmount = (ushort)region.unityKey });

                                    sf2.AddInstrumentGenerator(SF2Generator.DelayVolEnv, new SF2GeneratorAmount { Amount = (short)short.MinValue });
                                    /* En timecents absolu, c'est la durée, depuis la fin du délai de l'enveloppe de volume jusqu'au point où la valeur de l'enveloppe de volume atteint son apogée.
                                    Une valeur de 0 indique 1 seconde. Une valeur négative indique un temps inférieur à une seconde, une valeur positive un temps supérieur à une seconde.
                                    Le nombre le plus négatif (-32768) indique conventionnellement une attaque instantanée.
                                    Ex : un temps d'attaque de 10 ms serait 1200log2 (.01) = -7973.
                                    En musique, le logarithme binaire intervient dans la formule permettant de déterminer la valeur en cents d’un intervalle.
                                    Un cent, ou centième de demi-ton au tempérament égal, vaut 1200 fois le logarithme binaire du rapport de fréquence des sons concernés.
                                    546 * 60 ~= short.MaxValue */
                                    sf2.AddInstrumentGenerator(SF2Generator.AttackVolEnv, new SF2GeneratorAmount { Amount = (short)(1200 * Math.Log(articulation.A, 2)) });
                                    sf2.AddInstrumentGenerator(SF2Generator.HoldVolEnv, new SF2GeneratorAmount { Amount = (short)0 });
                                    /* C'est le temps, en timecents absolus, pour une variation de 100% de la valeur de l'enveloppe du volume pendant la phase de décroissance.
                                    Pour l'enveloppe de volume, la décroissance tend linéairement vers le niveau de maintien, ce qui provoque un changement de dB constant pour chaque unité de temps.
                                    Si le niveau de maintien = -100dB, le temps de décroissance de l'enveloppe de volume = temps de la phase de décroissance.
                                    Une valeur de 0 indique 1 seconde de temps de décroissance pour un niveau zéro. Une valeur négative indique un temps inférieur à une seconde,
                                    une valeur positive un temps supérieur à une seconde.
                                    Ex : un temps de décroissance de 10 msec serait 1200log2 (.01) = -7973.*/
                                    sf2.AddInstrumentGenerator(SF2Generator.DecayVolEnv, new SF2GeneratorAmount { Amount = (short)(1200 * Math.Log(articulation.D, 2)) });
                                    /* C'est le taux de la diminution, exprimé en centibels, pour laquelle l'enveloppe de volume décroît au cours de la phase de décroissance.
                                    Pour l'enveloppe de volume, le niveau d'atténuation du sustain est mieux exprimé en centibels. Une valeur de 0 indique que le niveau est maximum.
                                    Une valeur positive indique une décroissance au niveau correspondant. Les valeurs inférieures à zéro doivent être interprétés comme zéro;
                                    conventionnellement 1000 indique une atténuation complète.
                                    Ex : un niveau de soutien qui correspond à une valeur absolue de 12 dB en dessous du pic serait 120.*/
                                    sf2.AddInstrumentGenerator(SF2Generator.SustainVolEnv, new SF2GeneratorAmount { Amount = (short)(articulation.S) });
                                    /* C'est la durée, en timecents absolu, pour une variation de 100% de la valeur de l'enveloppe du volume pendant la phase de libération (release).
                                    Pour l'enveloppe de volume, la phase de libération tend linéairement vers zéro depuis la niveau en cours,
                                    ce qui provoque un changement en dB constant pour chaque unité de temps.
                                    Si le niveau actuel est maximum, la durée du release de l'enveloppe de volume sera le temps de libération jusqu'à ce que 100 dB d'atténuation soit atteint.
                                    Une valeur de 0 indique 1 seconde de temps de décroissance pour finir complètement. Une valeur négative indique un temps inférieur à une seconde,
                                    une valeur positive un temps de plus d'une seconde.
                                    Ex : un temps de libération de 10 msec serait 1200log2 (.01) = -7973. */
                                    sf2.AddInstrumentGenerator(SF2Generator.ReleaseVolEnv, new SF2GeneratorAmount { Amount = (short)(1200 * Math.Log(articulation.R, 2)) });
                                    /* Décalage de la hauteur, en cents, qui sera appliqué à la note.
                                    Il est additionnel à coarseTune. Une valeur positive indique que le son est reproduit à une hauteur plus élevée, une valeur négative indique une hauteur inférieure.
                                    Ex : une valeur finetune = -5 provoquera un son joué cinq cents plus bas. */
                                    sf2.AddInstrumentGenerator(SF2Generator.FineTune, new SF2GeneratorAmount { Amount = (short)(region.fineTune) });
                                    sf2.AddInstrumentGenerator(SF2Generator.SampleID, new SF2GeneratorAmount { UAmount = (ushort)sampleIDX });
                                }

                            }

                            dls.AddInstrument(DSLInstrument);
                        }

                    }
                }
            }

            if (Samples.Count > 0)
            {
                foreach (AKAOSample AKAOsmp in Samples)
                {
                    WAV nw = AKAOsmp.ConvertToWAV();
                    nw.SetName(AKAOsmp.name);
                    nw.Riff = false;
                    dls.AddWave(nw);

                    short[] pcm = AKAOsmp.WAVDatas.ToArray();
                    AKAOsmp.loopStart = (uint)(AKAOsmp.loopStart * 1.75);
                    sf2.AddSample(pcm, AKAOsmp.name, (AKAOsmp.loopStart > 0), AKAOsmp.loopStart, 44100, AKAOsmp.unityKey, 0);
                }
            }

            if (bDLS)
            {
                ToolBox.DirExNorCreate("Assets/Resources/Sounds/DLS/");
                dls.WriteFile("Assets/Resources/Sounds/DLS/" + FileName + ".dls");
            }

            if (bSF2)
            {
                ToolBox.DirExNorCreate("Assets/Resources/Sounds/SF2/");
                sf2.Save("Assets/Resources/Sounds/SF2/" + FileName + ".sf2");
            }


            return sf2;
        }
    }
}
