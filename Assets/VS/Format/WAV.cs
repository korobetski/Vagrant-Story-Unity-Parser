using System;
using System.Collections.Generic;

namespace VS.Format
{
    //http://soundfile.sapp.org/doc/WaveFormat/
    public class WAV : RIFF
    {
        public ushort AudioFormat;
        public ushort NumChannels;
        public ulong SampleRate;
        public ulong ByteRate;
        public ushort BlockAlign;
        public ushort BitsPerSample;

        public WAV(List<byte> datas, ushort AF = 1, ushort NC = 1, ulong SR = 44100, ushort BPS = 16) : base("WAVE")
        {
            AudioFormat = AF;
            NumChannels = NC;
            SampleRate = SR;
            ByteRate = SampleRate * NumChannels * BitsPerSample / 8;
            BlockAlign = (ushort)(NumChannels * BitsPerSample / 8);
            BitsPerSample = BPS;

            Chunk fmt = new Chunk("fmt ");
            List<byte> fmtb = new List<byte>();
            fmtb.AddRange(BitConverter.GetBytes((ushort)AudioFormat)); // Audio Format
            fmtb.AddRange(BitConverter.GetBytes((ushort)NumChannels)); // Num Channels
            fmtb.AddRange(BitConverter.GetBytes((ulong)SampleRate)); // Sample Rate
            fmtb.AddRange(BitConverter.GetBytes((ulong)ByteRate)); // Byte Rate
            fmtb.AddRange(BitConverter.GetBytes((ushort)BlockAlign)); // BlockAlign
            fmtb.AddRange(BitConverter.GetBytes((ushort)BitsPerSample)); // Bits Per Sample
            fmt.data = fmtb;
            AddChunk(fmt);

            Chunk data = new Chunk("data");
            fmt.data = datas;
            AddChunk(data);
        }

    }

}