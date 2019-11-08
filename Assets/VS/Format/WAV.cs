using System;
using System.Collections.Generic;

namespace VS.Format
{
    //http://soundfile.sapp.org/doc/WaveFormat/
    public class WAV : RIFF, IChunk
    {
        public ushort AudioFormat = 1; // PCM
        public ushort NumChannels = 1;
        public uint SampleRate = 44100;
        public uint ByteRate = 88200;
        public ushort BlockAlign = 2;
        public ushort BitsPerSample = 16;
        public double compressionRatio = 3.5;
        public bool Riff = true;
        public bool hasLoop = false;
        private Loop loop;

        public WAV(List<byte> datas, ushort AF = 1, ushort NC = 1, uint SR = 44100, ushort BPS = 16) : base("WAVE")
        {
            AudioFormat = AF;
            NumChannels = NC;
            SampleRate = SR;
            ByteRate = SampleRate * NumChannels * BitsPerSample / 8;
            BlockAlign = (ushort)(NumChannels * BitsPerSample / 8);
            BitsPerSample = BPS;

            Chunk fmt = new Chunk("fmt ");
            fmt.SetDataCapacity(16);
            List<byte> fmtb = new List<byte>();
            fmtb.AddRange(BitConverter.GetBytes((ushort)AudioFormat));      // Audio Format
            fmtb.AddRange(BitConverter.GetBytes((ushort)NumChannels));      // Num Channels
            fmtb.AddRange(BitConverter.GetBytes((uint)SampleRate));         // Sample Rate
            fmtb.AddRange(BitConverter.GetBytes((uint)ByteRate));           // Byte Rate
            fmtb.AddRange(BitConverter.GetBytes((ushort)BlockAlign));       // BlockAlign
            fmtb.AddRange(BitConverter.GetBytes((ushort)BitsPerSample));    // Bits Per Sample
            fmt.SetData(fmtb);
            AddChunk(fmt);

            Chunk data = new Chunk("data");
            data.SetData(datas);
            data.SetDataCapacity(datas.Count);
            AddChunk(data);

            if (hasLoop)
            {
                int origFormatBytesPerSamp = BitsPerSample / 8;

                // If the sample loops, but the loop length is 0, then assume the length should
                // extend to the end of the sample.
                uint loopLength = loop.loopLength;
                if (loop.loopStatus > 0 && loop.loopLength == 0)
                {
                    loopLength = (uint)datas.Count - loop.loopStart;
                }

                uint loopStart =
                    (!loop.loopAll) ?
                    (uint)((loop.loopStart * compressionRatio) / origFormatBytesPerSamp) :
                    loop.loopStart;
                uint loopLenInSamp =
                    (!loop.loopAll) ?
                    (uint)((loopLength * compressionRatio) / origFormatBytesPerSamp) :
                    loopLength;
                uint loopEnd = loopStart + loopLenInSamp;

                Chunk smpl = new Chunk("smpl");
                List<byte> smplb = new List<byte>();
                smplb.AddRange(BitConverter.GetBytes((uint)0));      // manufacturer
                smplb.AddRange(BitConverter.GetBytes((uint)0));      // product
                smplb.AddRange(BitConverter.GetBytes((uint)1000000000 / SampleRate));         // sample period Rate
                smplb.AddRange(BitConverter.GetBytes((uint)60));           // MIDI uniti note (C5)
                smplb.AddRange(BitConverter.GetBytes((uint)0));       // MIDI pitch fraction
                smplb.AddRange(BitConverter.GetBytes((uint)0));    // SMPTE format
                smplb.AddRange(BitConverter.GetBytes((uint)0));    // SMPTE format
                smplb.AddRange(BitConverter.GetBytes((uint)1));    // sample loops
                smplb.AddRange(BitConverter.GetBytes((uint)0));       // sampler data
                smplb.AddRange(BitConverter.GetBytes((uint)0));    // cue point ID
                smplb.AddRange(BitConverter.GetBytes((uint)0));    // type (loop forward)
                smplb.AddRange(BitConverter.GetBytes((uint)loopStart));    // start sample #
                smplb.AddRange(BitConverter.GetBytes((uint)loopEnd));    // end sample #
                smplb.AddRange(BitConverter.GetBytes((uint)0));    // fraction
                smplb.AddRange(BitConverter.GetBytes((uint)0));    // playcount

                smpl.SetDataCapacity(0x50);
                smpl.SetData(smplb);
                AddChunk(smpl);
            }
        }

        public void SetLoop(Loop lp)
        {
            loop = lp;
        }

        public new List<byte> Write()
        {
            List<byte> buffer = new List<byte>();
            if (Riff == true)
            {
                buffer.AddRange(new byte[] { (byte)id[0], (byte)id[1], (byte)id[2], (byte)id[3] });
                buffer.AddRange(BitConverter.GetBytes((uint)size+4));
            }
            else
            {
                buffer.AddRange(new byte[] { 0x4C, 0x49, 0x53, 0x54 }); // LIST
                buffer.AddRange(BitConverter.GetBytes((uint)size+4));
            }
            buffer.AddRange(new byte[] { (byte)type[0], (byte)type[1], (byte)type[2], (byte)type[3] });
            foreach (IChunk ck in chunks)
            {
                buffer.AddRange(ck.Write());
            }
            return buffer;
        }
    }

}