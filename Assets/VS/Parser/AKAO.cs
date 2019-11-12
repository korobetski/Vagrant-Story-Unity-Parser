using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Format;
using VS.Parser.Akao;
using VS.Utils;

//Minoru Akao
//https://github.com/vgmtrans/vgmtrans/blob/master/src/main/formats/AkaoSeq.cpp

// Akao in MUSIC folder contains music instructions like a Midi file, Akao in SOUND folder contains samples collection like .sfb / .sf2 / .dls files
// Musics need to use a sample collection to give a sampled music sound or else a midi like sound will be created.
namespace VS.Parser
{

    public class AKAO : FileParser
    {
        public static readonly AKAOType SOUND = AKAOType.SOUND;
        public static readonly AKAOType MUSIC = AKAOType.MUSIC;

        public enum AKAOType { SOUND, MUSIC }

        public AKAOType _type;
        public AKAOInstrument[] instruments;
        public AKAOArticulation[] articulations;
        public AKAOSample[] samples;
        public AKAOComposer composer;

        public uint startingArticulationId;

        public void Parse(string filePath, AKAOType type)
        {
            PreParse(filePath);
            _type = type;
            if (FileSize < 4)
            {
                return;
            }
            Parse(buffer);

            buffer.Close();
            fileStream.Close();
        }

        public void Parse(BinaryReader buffer)
        {
            if (buffer.BaseStream.Length < 4)
            {
                return;
            }

            switch (_type)
            {
                case AKAOType.MUSIC:
                    // header datas
                    byte[] header = buffer.ReadBytes(4);// AKAO
                    ushort fileId = buffer.ReadUInt16();
                    ushort byteLen = buffer.ReadUInt16();
                    ushort reverb = buffer.ReadUInt16(); // 0x0500  | Just on case 0x0400 (MUSIC000.DAT)
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
                    if (UseDebug)
                    {
                        //Debug.Log(string.Concat("Listing count : ", (jump - 2) / 2));
                    }
                    for (uint i = 0; i < (jump - 2) / 2; i++)
                    {
                        byte[] b = buffer.ReadBytes(2);
                        //Debug.Log(b[0] + "   ->   " + b[1]);
                    }



                    if (UseDebug)
                    {
                        Debug.Log("AKAO from : " + FileName + " FileSize = " + FileSize + "    |    reverb : " + reverb + " numTrack : " + numTrack + " sampleSet : " + sampleSet);
                        Debug.Log("instruments at : " + ptr1 + "  Drums at : " + ptr2 + "   |    unk1 : " + unk1 + "  unk3 : " + unk3 + "   musInstrPtr : " + musInstrPtr);
                    }



                    // music instuctions begin here, MIDI like format
                    if (buffer.BaseStream.Position != musInstrPtr)
                    {
                        buffer.BaseStream.Position = musInstrPtr;
                    }
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
                            if (UseDebug)
                            {
                                Debug.Log(buffer.BaseStream.Position + "  --  " + ptr2);
                            }
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
                            }
                        }

                        drum.regions = dr.ToArray();
                        instruments[instrCount - 1] = drum;
                    }

                    composer = new AKAOComposer(buffer, musInstrPtr, ptr1, instrCount, numTrack, FileName, UseDebug);


                    
                    // So we seek for the appropriate WAVE*.DAT in the SOUND folder
                    string[] hash = FilePath.Split("/"[0]);
                    hash[hash.Length - 2] = "SOUND";
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
                    if (UseDebug)
                    {
                        Debug.Log("Seek for : " + samplePath + " -> " + test);
                    }

                    AKAO sampleParser = new AKAO();
                    //sampleParser.UseDebug = true;
                    sampleParser.Parse(samplePath, AKAO.SOUND);
                    Synthetize(this, sampleParser);
                    


                    break;
                case AKAOType.SOUND:
                    // Samples Collection
                    // https://www.midi.org/specifications/category/dls-specifications
                    // header datas
                    header = buffer.ReadBytes(4);       // AKAO
                    ushort sampleId = buffer.ReadUInt16();
                    buffer.ReadBytes(10); // padding

                    reverb = buffer.ReadUInt16(); // 0x0031 - 0x0051
                    buffer.ReadBytes(2); // padding
                    var sampleSize = buffer.ReadUInt32();
                    startingArticulationId = buffer.ReadUInt32();
                    var numArts = buffer.ReadUInt32();

                    buffer.ReadBytes(32); // padding

                    if (UseDebug)
                    {
                        Debug.Log("AKAO from : " + FileName + " len = " + FileSize);
                        Debug.Log("ID : " + sampleId + " reverb : " + reverb + " sampleSize : " + sampleSize + " stArtId : " + startingArticulationId + " numArts : " + numArts);
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
                                samEPtr.Add(buffer.BaseStream.Position - 16);
                            }
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
                        AKAOSample sam = new AKAOSample("Sample #" + (ushort)i, dt, (ulong)samPtr[i]);
                        samples[i] = sam;
                    }


                    // now to verify and associate each articulation with a sample index value
                    // for every sample of every instrument, we add sample_section offset, because those values
                    //  are relative to the beginning of the sample section
                    for (uint i = 0; i < articulations.Length; i++)
                    {
                        for (uint l = 0; l < samples.Length; l++)
                        {
                            // 0x10 = empty space between samples
                            if (articulations[i].sampleOff + samStart + 0x10 == samples[l].offset)
                            {
                                articulations[i].sampleNum = l;
                                articulations[i].sample = samples[l];
                                break;
                            }
                        }
                    }
                    break;
            }

        }




        private void Synthetize(AKAO sequencer, AKAO sampler)
        {
            DLS dls = new DLS();
            dls.SetName(FileName + ".dls");

            if (sequencer.instruments != null)
            {
                uint i = 0;
                foreach (AKAOInstrument instrument in sequencer.instruments)
                {
                    if (instrument.regions.Length > 0)
                    {
                        Lins DSLInstrument = new Lins(0, (uint)(sequencer.instruments.Length + sequencer.startingArticulationId + i), instrument.name);

                        foreach (AKAORegion region in instrument.regions)
                        {
                            AKAOArticulation articulation;
                            //Debug.Log(string.Concat("region.articulationId : ", region.articulationId));
                            if (!((region.articulationId - sampler.startingArticulationId) >= 0 && region.articulationId - sampler.startingArticulationId < 200))
                            {
                                //Debug.LogWarning("Articulation #" + region.articulationId + " does not exist in the samp collection.");
                                articulation = sampler.articulations[0];
                            }

                            if (region.articulationId - sampler.startingArticulationId >= sampler.articulations.Length)
                            {
                                //Debug.LogWarning("Articulation #" + region.articulationId + " referenced but not loaded");
                                articulation = sampler.articulations[sampler.articulations.Length - 1];
                            }
                            else
                            {
                                articulation = sampler.articulations[region.articulationId - sampler.startingArticulationId];
                            }

                            region.articulation = articulation;
                            region.sampleNum = articulation.sampleNum;
                            region.ComputeADSR();

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



                            // Making DLS
                            Lrgn reg = new Lrgn(region.lowRange, region.hiRange, 0x00, 0x7F);
                            CKwsmp smp = new CKwsmp((ushort)region.unityKey, region.fineTune, region.attenuation, 1);

                            if (articulation.loopPt != 0)
                            {
                                smp.AddLoop(new Loop(1, articulation.loopPt, (uint)(sampler.samples[region.sampleNum].size - articulation.loopPt)));
                            }

                            reg.SetSample(smp);
                            if (instrument.IsDrum())
                            {
                                CKart2 dart = new CKart2();
                                dart.AddPan(0x40);
                                reg.AddArticulation(dart);
                            }
                            else
                            {
                                if (articulation != null)
                                {
                                    CKart2 iart = new CKart2();
                                    iart.AddPan(0x40);
                                    DSLInstrument.AddArticulation(iart);
                                }
                            }

                            reg.SetWaveLinkInfo(0, 0, 1, region.sampleNum);
                            DSLInstrument.AddRegion(reg);

                        }


                        dls.AddInstrument(DSLInstrument);
                    }

                    i++;
                }
            }

            if (sampler.samples != null)
            {
                foreach (AKAOSample AKAOsmp in sampler.samples)
                {
                    WAV nw = AKAOsmp.ConvertToWAV();
                    //nw.SetName(AKAOsmp.name);
                    nw.Riff = false;
                    dls.AddWave(nw);
                }
            }

            ToolBox.DirExNorCreate("Assets/Resources/Sounds/DLS/");
            dls.WriteFile("Assets/Resources/Sounds/DLS/" + FileName + ".dls");
        }
    }
}
