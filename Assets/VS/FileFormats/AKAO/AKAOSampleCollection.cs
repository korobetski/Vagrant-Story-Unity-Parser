using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VS.FileFormats.AKAO
{
    public class AKAOSampleCollection:ScriptableObject
    {
        public string Filename;

        public ushort sampleId;
        public byte[] padx10;
        public byte unk1;
        public byte unk2;
        public ushort unk3;
        public uint sampleSize;
        public uint startingArticulationId;
        public uint numArticulations;

        public AKAOArticulation[] articulations;
        public AKAOSample[] samples;


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

            sampleId = buffer.ReadUInt16();
            padx10 = buffer.ReadBytes(10); // padding

            unk1 = buffer.ReadByte(); // almost always 0 (but one case 48 in WAVE0032)
            unk2 = buffer.ReadByte(); // almost always 81 (two cases 49 (WAVE0000, WAVE0005), one case 16 in WAVE0032, one case 177 in WAVE0200)
            unk3 = buffer.ReadUInt16(); // padding
            sampleSize = buffer.ReadUInt32();
            startingArticulationId = buffer.ReadUInt32();
            numArticulations = buffer.ReadUInt32();
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

            // Articulations section here
            articulations = new AKAOArticulation[numArticulations];
            for (uint i = 0; i < numArticulations; i++)
            {
                AKAOArticulation articulation = new AKAOArticulation();
                articulation.id = startingArticulationId + i;
                articulation.samplePointer = buffer.ReadUInt32();
                articulation.loopPt = buffer.ReadUInt32() - articulation.samplePointer;
                articulation.fineTune = buffer.ReadInt16();
                articulation.unityKey = buffer.ReadUInt16();
                articulation.ADSR1 = buffer.ReadUInt16();
                articulation.ADSR2 = buffer.ReadUInt16();
                articulations[i] = articulation;
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
                    samPtr.Add(buffer.BaseStream.Position - 0x10);
                    //samPtr.Add(buffer.BaseStream.Position);
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
                AKAOSample sample = new AKAOSample();
                sample.id = (byte)i;
                sample.name = string.Concat(Filename, " Sample #", (ushort)i);
                sample.rawDatas = dt;
                sample.pointer = (ulong)samPtr[i];
                samples[i] = sample;
                /*
                if (UseDebug && bWAV)
                {
                    WAV wavSam = sam.ConvertToWAV();
                    wavSam.SetName(FileName + "_Sample_" + i);
                    ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Sounds/SampleColl/");
                    wavSam.WriteFile(Application.dataPath + "/../Assets/Resources/Sounds/SampleColl/" + FileName + "_Sample_" + i + ".wav", wavSam.Write());
                }
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
                    if (articulations[i].samplePointer + samStart == samples[l].pointer)
                    {
                        articulations[i].sampleId = (ushort)l;
                        articulations[i].sample = samples[l];
                        samples[l].loopStart = articulations[i].loopPt;

                        break;
                    }
                }
            }
        }
    }
}
