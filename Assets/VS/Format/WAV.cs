using System;
using System.Collections.Generic;

namespace VS.Format
{
    //http://soundfile.sapp.org/doc/WaveFormat/
    public class WAV : RIFF, IChunk
    {
        public ushort AudioFormat = 1;
        public ushort NumChannels = 1;
        public uint SampleRate = 44100;
        public uint ByteRate = 88200;
        public ushort BlockAlign = 2;
        public ushort BitsPerSample = 16;
        public bool Riff = true;

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
        }

        public new List<byte> Write()
        {
            List<byte> buffer = new List<byte>();
            if (Riff == true)
            {
                buffer.AddRange(new byte[] { (byte)id[0], (byte)id[1], (byte)id[2], (byte)id[3] });
                buffer.AddRange(BitConverter.GetBytes((uint)size));
            }
            else
            {
                buffer.AddRange(new byte[] { 0x4C, 0x49, 0x53, 0x54 }); // LIST
                buffer.AddRange(BitConverter.GetBytes((uint)size));
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