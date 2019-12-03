using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Format;
using VS.Parser.Akao;
using VS.Utils;


// Minoru Akao

// Akao in MUSIC folder contains music instructions like a Midi file, Akao in SOUND folder contains samples collection like .sfb / .sf2 / .dls files
// Musics need to use a sample collection to give a sampled music sound or else a midi like sound will be created.
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
        public AKAOInstrument[] instruments;
        public AKAOArticulation[] articulations;
        public AKAOSample[] samples;
        public AKAOComposer composer;

        public uint startingArticulationId;

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

                    uint unk1 = buffer.ReadUInt32();
                    uint sampleSet = buffer.ReadUInt32(); // ID of the WAVE*.DAT in the SOUND folder
                    buffer.ReadBytes(8); // padding

                    int bnumt = buffer.ReadInt32();
                    short unk3 = buffer.ReadInt16();
                    buffer.ReadBytes(10); // padding

                    int ptr1 = buffer.ReadUInt16() + 0x30; // instruments pointer
                    buffer.ReadUInt16(); // always 0000
                    int ptr2 = buffer.ReadUInt16() + 0x34; // Drums pointer
                    buffer.ReadBytes(10); // padding

                    ushort jump = buffer.ReadUInt16();
                    long musInstrPtr = buffer.BaseStream.Position + jump - 2;
                    uint numTrack = ToolBox.GetNumPositiveBits(bnumt);


                    // kind of listing here, i don't know what it is.
                    for (uint i = 0; i < (jump - 2) / 2; i++)
                    {
                        byte[] b = buffer.ReadBytes(2);
                        //Debug.Log(b[0] + "   ->   " + b[1]);
                    }



                    if (UseDebug)
                    {
                        Debug.Log(string.Concat("AKAO from : ", FileName, " FileSize = ", FileSize, "    |    reverb : ", reverb, " numTrack : ", numTrack, " sampleSet : ", sampleSet,
                            "\r\ninstruments at : ", ptr1, "  Drums at : ", ptr2, "   musInstrPtr : ", musInstrPtr, "   |  unk1 : ", unk1, "   unk3 : ", unk3));
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
                                //if (UseDebug) Debug.Log("Instrument "+ instrPtrs.Count+ " ptr : " + instrPtr);
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

                        instruments = new AKAOInstrument[instrCount];

                        for (int i = 0; i < instrPtrs.Count; i++)
                        {
                            AKAOInstrument instrument = new AKAOInstrument(AKAOInstrument.InstrumentType.INSTR_MELODIC);
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
                                Debug.Log("reg.articulationId : " + reg.articulationId);
                            }
                            buffer.ReadBytes(8);// 0000 0000 0000 0000 padding

                            instruments[i] = instrument;
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
                            instruments = new AKAOInstrument[1];
                        }

                        AKAOInstrument drum = new AKAOInstrument(AKAOInstrument.InstrumentType.INSTR_DRUM);
                        drum.name = "Drum";
                        int drumRegLoop = (int)(byteLen - ptr2) / 0x08;

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
                                Debug.Log("dregion.articulationId : " + dregion.articulationId);
                            }
                        }

                        drum.regions = dr.ToArray();
                        instruments[instrCount - 1] = drum;
                    }

                    composer = new AKAOComposer(buffer, musInstrPtr, ptr1, instrCount, numTrack, FileName, false);

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
                    hash[hash.Length - 1] = "WAVE0005.DAT"; // wave 005 or wave 032 ?
                    AKAO SampleColl32 = new AKAO();
                    SampleColl32.UseDebug = true;
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
                    SampleColl64.UseDebug = true;
                    SampleColl64.Parse(samplePath, AKAO.SOUND);
                    sampleCollections[1] = SampleColl64;

                    // Additionnal Collection in case the SampleColl64 isn't 64 arts long, this one must be 64 arts long
                    if (SampleColl64.articulations.Length < 64)
                    {
                        hash[hash.Length - 1] = "WAVE0099.DAT";
                        AKAO addiColl = new AKAO();
                        addiColl.UseDebug = true;
                        addiColl.Parse(String.Join("/", hash), AKAO.SOUND);
                        sampleCollections[2] = addiColl;
                    }

                    Synthetize(this, sampleCollections);



                    break;
                case AKAOType.SOUND:
                    // Samples Collection

                    // header datas
                    header = buffer.ReadBytes(4);       // AKAO
                    ushort sampleId = buffer.ReadUInt16();
                    buffer.ReadBytes(10); // padding

                    unk1 = buffer.ReadByte();
                    unk3 = buffer.ReadByte();
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
                        AKAOArticulation arti = new AKAOArticulation(buffer);
                        articulations[i] = arti;
                    }

                    // Samples section here
                    ulong samStart = (ulong)buffer.BaseStream.Position;
                    //Debug.Log(string.Concat("samStart : ", samStart));
                    // First we need to determine the start and the end of the samples, 16 null bytes indicate a new sample, so lets find them.
                    List<long> samPtr = new List<long>();
                    List<long> samEPtr = new List<long>();
                    while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                    {
                        if (buffer.ReadUInt64() + buffer.ReadUInt64() == 0)
                        {
                            if (samPtr.Count > 0)
                            {
                                samEPtr.Add(buffer.BaseStream.Position - 0x20); // samEPtr.Add(buffer.BaseStream.Position - 0x10);
                            }
                            samPtr.Add(buffer.BaseStream.Position - 0x10); // samPtr.Add(buffer.BaseStream.Position);
                        }
                    }
                    samEPtr.Add(buffer.BaseStream.Length);

                    // Let's loop again to get samples
                    int numSam = samPtr.Count;
                    //Debug.Log("numSam : " + numSam);
                    samples = new AKAOSample[numSam];
                    for (int i = 0; i < numSam; i++)
                    {
                        buffer.BaseStream.Position = samPtr[i];
                        int size = (int)(samEPtr[i] - samPtr[i]);
                        byte[] dt = buffer.ReadBytes(size);
                        AKAOSample sam = new AKAOSample(string.Concat("Sample #", (ushort)i), dt, (ulong)samPtr[i]);
                        sam.index = i;
                        samples[i] = sam;

                        /*
                        WAV wavSam = sam.ConvertToWAV();
                        wavSam.SetName(FileName + "_Sample_" + i);
                        ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Sounds/SampleColl/");
                        wavSam.WriteFile(Application.dataPath + "/../Assets/Resources/Sounds/SampleColl/" + FileName + "_Sample_"+i+".wav", wavSam.Write());
                        */
                    }
                    // now to verify and associate each articulation with a sample index value
                    // for every sample of every instrument, we add sample_section offset, because those values
                    //  are relative to the beginning of the sample section
                    for (uint i = 0; i < articulations.Length; i++)
                    {
                        for (uint l = 0; l < samples.Length; l++)
                        {
                            //if (articulations[i].sampleOff + samStart + 0x10 == samples[l].offset)
                            if (articulations[i].sampleOff + samStart == samples[l].offset)
                            {
                                articulations[i].sampleNum = l;
                                articulations[i].sample = samples[l];
                                //Debug.Log("articulations["+i+"].sampleNum : " + l);
                                break;
                            }
                        }
                    }
                    break;
                case AKAOType.PROG:
                    // Not understood yet
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
                    int waveLen = (int)(limit - buffer.BaseStream.Position);
                    if (waveLen > 0)
                    {
                        buffer.ReadBytes(waveLen);
                    }

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




        private void Synthetize(AKAO sequencer, AKAO[] sampleCollections)
        {
            SF2 sf2 = new SF2();

            List<AKAOSample> Samples = new List<AKAOSample>();

            if (sequencer.instruments != null)
            {
                uint i = 0;
                foreach (AKAOInstrument instrument in sequencer.instruments)
                {
                    sf2.AddPreset(instrument.name, (ushort)i, 0);
                    sf2.AddPresetBag();
                    sf2.AddPresetGenerator(SF2Generator.ReverbEffectsSend, new SF2GeneratorAmount { UAmount = (ushort)250 });
                    sf2.AddPresetGenerator(SF2Generator.Instrument, new SF2GeneratorAmount { UAmount = (ushort)i });
                    sf2.AddInstrument(instrument.name);

                    if (instrument.regions.Length > 0)
                    {
                        uint midiBank = 0x00000000;
                        if (instrument.IsDrum())
                        {
                            midiBank = DLS.F_INSTRUMENT_DRUMS;
                        }

                        foreach (AKAORegion region in instrument.regions)
                        {
                            AKAOArticulation articulation = null;
                            AKAO coll = sampleCollections[2];
                            //Debug.Log(string.Concat("region.articulationId : ", region.articulationId));

                            if (region.articulationId >= 32 && region.articulationId < 64)
                            {
                                coll = sampleCollections[0];
                                articulation = coll.articulations[region.articulationId - coll.startingArticulationId];

                            }
                            else if (region.articulationId >= 64 && region.articulationId < 128)
                            {
                                coll = sampleCollections[1];
                                if (region.articulationId - coll.startingArticulationId < coll.articulations.Length)
                                {
                                    articulation = coll.articulations[region.articulationId - coll.startingArticulationId];
                                }
                                else
                                {
                                    //Debug.LogError("region.articulationId out of range : "+ region.articulationId);
                                    // we check in additional collection
                                    coll = sampleCollections[2];
                                    articulation = coll.articulations[region.articulationId - coll.startingArticulationId];

                                }
                            }
                            //Debug.Log(string.Concat("articulation.sampleNum : ", articulation.sampleNum, "   coll start id : ", coll.startingArticulationId));



                            if (articulation != null)
                            {
                                articulation.BuildADSR();
                                region.articulation = articulation;
                                AKAOSample sample = coll.samples[articulation.sampleNum];

                                if (instrument.IsDrum())
                                {
                                    region.unityKey = (uint)articulation.unityKey + region.lowRange - region.relativeKey;
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

                                // this gives us the pitch multiplier value ex. 1.05946
                                double freq_multiplier = ((ft * 32) + 0x100000) / (double)0x100000;
                                double cents = Mathf.Log((float)freq_multiplier) / Mathf.Log(2) * 1200;
                                if (articulation.fineTune < 0)
                                {
                                    cents -= 1200;
                                }

                                region.fineTune = (short)cents;
                                sample.unityKey = (byte)region.unityKey;
                                sample.fineTune = (sbyte)region.fineTune;
                                sample.loopStart = (uint)articulation.loopPt;

                                if (!Samples.Contains(sample))
                                {
                                    Samples.Add(sample);
                                }

                                int sampleIDX = Samples.IndexOf(sample);


                                sf2.AddInstrumentBag();
                                sf2.AddInstrumentGenerator(SF2Generator.KeyRange, new SF2GeneratorAmount { LowByte = region.lowRange, HighByte = region.hiRange });
                                sf2.AddInstrumentGenerator(SF2Generator.VelRange, new SF2GeneratorAmount { LowByte = 0, HighByte = 127 });
                                sf2.AddInstrumentGenerator(SF2Generator.InitialAttenuation, new SF2GeneratorAmount { Amount = (short)(region.volume / 10) });
                                sf2.AddInstrumentGenerator(SF2Generator.Pan, new SF2GeneratorAmount { UAmount = 0x00 });
                                sf2.AddInstrumentGenerator(SF2Generator.SampleModes, new SF2GeneratorAmount { UAmount = (articulation.loopPt != 0) ? (ushort)1 : (ushort)0 });
                                sf2.AddInstrumentGenerator(SF2Generator.OverridingRootKey, new SF2GeneratorAmount { UAmount = (ushort)region.unityKey });

                                sf2.AddInstrumentGenerator(SF2Generator.AttackVolEnv, new SF2GeneratorAmount { Amount = (short)(short.MinValue + articulation.Ar) }); // articulation.A
                                sf2.AddInstrumentGenerator(SF2Generator.DecayVolEnv, new SF2GeneratorAmount { Amount = (short)2415 }); // articulation.D
                                sf2.AddInstrumentGenerator(SF2Generator.SustainVolEnv, new SF2GeneratorAmount { Amount = (short)0 }); // articulation.S
                                sf2.AddInstrumentGenerator(SF2Generator.ReleaseVolEnv, new SF2GeneratorAmount { Amount = (short)-4350 }); // articulation.R

                                sf2.AddInstrumentGenerator(SF2Generator.SampleID, new SF2GeneratorAmount { UAmount = (ushort)sampleIDX });
                            }

                        }

                    }

                    i++;
                }
            }

            if (Samples.Count > 0)
            {
                foreach (AKAOSample AKAOsmp in Samples)
                {
                    WAV nw = AKAOsmp.ConvertToWAV();
                    nw.SetName(AKAOsmp.name);
                    nw.Riff = false;

                    short[] pcm = AKAOsmp.WAVDatas.ToArray();
                    sf2.AddSample(pcm, AKAOsmp.name, (AKAOsmp.loopStart > 0), (uint)(AKAOsmp.loopStart * 1.75), 44100, AKAOsmp.unityKey, AKAOsmp.fineTune);
                }
            }

            ToolBox.DirExNorCreate("Assets/Resources/Sounds/SF2/");
            sf2.Save("Assets/Resources/Sounds/SF2/" + FileName + ".sf2");

        }
    }
}
