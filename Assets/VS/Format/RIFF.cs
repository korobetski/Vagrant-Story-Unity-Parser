using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VS.Format
{

/*
Notation Description
<element label>             RIFF file element with the label “ element label”
<element label: TYPE>       RIFF file element with data type “ TYPE”
[<element label>]           Optional RIFF file element
<element label>...          One or more copies of the specified element
[<element label>]...        Zero or more copies of the specified element
*/

    public class RIFF : ListTypeChunk
    {


        public RIFF(string form) : base("RIFF", form)
        {
        }


        public static void AlignName(string name)
        {
            name += (char)0x00;
            if (name.Length % 2 > 0)     // if the size of the name string is odd
            {
                name += (char)0x00;  // add another null byte
            }
        }

        public bool WriteFile(string path, List<byte> buffer)
        {
            using (FileStream fs = File.Create(path))
            {
                for (int i = 0; i < buffer.Count; i++)
                {
                    fs.WriteByte(buffer[i]);
                }
                fs.Close();
            }
            return true;
        }
    }

    public interface IChunk
    {
        List<byte> Write();
        uint GetPaddedSize();
        uint Resize();
        uint GetHeaderSize();
        uint GetSize();
    }


    public class ListTypeChunk : Chunk, IChunk
    {
        public new uint headerSize = 12;
        public char[] type = new char[4];
        public List<IChunk> chunks;

        public ListTypeChunk(string sId, string sType) : base(sId)
        {
            size = 0;
            type = sType.ToCharArray();
            chunks = new List<IChunk>();
        }

        public ListTypeChunk(string sId, string sType, List<IChunk> lchunks) : base(sId)
        {
            type = sType.ToCharArray();
            chunks = lchunks;
            size = 0;
            foreach (IChunk ck in chunks)
            {
                size += ck.GetHeaderSize() + ck.GetSize();
                chunks = new List<IChunk>();
            }
        }


        public IChunk AddChunk(IChunk chunk)
        {
            chunks.Add(chunk);
            size += chunk.GetHeaderSize()+chunk.GetSize();
            return chunk;
        }


        public new uint Resize()
        {
            size = 0;
            if (chunks != null && chunks.Count > 0)
            {
                foreach (IChunk ck in chunks)
                {
                    size += ck.GetHeaderSize() + ck.Resize();
                }
            }

            return size;
        }

        public new List<byte> Write()
        {
            Resize();
            List<byte> buffer = new List<byte>();
            buffer.AddRange(new byte[] { (byte)id[0], (byte)id[1], (byte)id[2], (byte)id[3] });
            buffer.AddRange(BitConverter.GetBytes((uint)size));
            buffer.AddRange(new byte[] { (byte)type[0], (byte)type[1], (byte)type[2], (byte)type[3] });

            foreach(IChunk ck in chunks)
            {
                buffer.AddRange(ck.Write());
            }

            return buffer;
        }
    };


    public class LISTChunk : ListTypeChunk, IChunk
    {
        public LISTChunk(string sType) : base("LIST", sType)
        {
        }

        public LISTChunk(string sType, List<IChunk> lchunks) : base("LIST", sType, lchunks)
        {
        }

        public override string ToString()
        {
            return "LIST Chunk #" + type[0] + type[1] + type[2] + type[3];
        }
    };

    public class Chunk : IChunk
    {
        public char[] id = new char[4]; //  A chunk ID identifies the type of data within the chunk.
        public uint headerSize = 8;
        public uint size = 0; //  The size of the chunk data in bytes, excluding any pad byte.
        private List<byte> data; //  The actual data not including a possible pad byte to word align

        public Chunk(string sId)
        {
            id = sId.ToCharArray();
            data = new List<byte>();
        }

        public Chunk(string sId, List<byte> bData)
        {
            id = sId.ToCharArray();
            SetData(bData);
        }

        public void SetData(List<byte> bData)
        {
            data = bData;
            size = (uint)data.Count;
        }
        public void SetData(byte[] bData)
        {
            data = new List<byte>(bData);
            size = (uint)data.Count;
        }
        public void AddDatas(byte[] bData)
        {
            data.AddRange(new List<byte>(bData));
            size = (uint)data.Count;
        }

        public uint GetPaddedSize()
        {
            return size + size % 2;
        }

        public uint GetHeaderSize()
        {
            return headerSize;
        }

        public uint GetSize()
        {
            return size;
        }

        public List<byte> Write()
        {
            Resize();
            List<byte> buffer = new List<byte>();
            buffer.AddRange(new byte[]{ (byte)id[0], (byte)id[1], (byte)id[2], (byte)id[3] });
            buffer.AddRange(BitConverter.GetBytes((uint)size));

            if (data != null) {
                buffer.AddRange(data);
            }
            return buffer;
        }

        public uint Resize()
        {
            size = 0;
            if (data != null && data.Count > 0)
            {
                size += (uint)data.Count;
            }

            return size;
        }

        public override string ToString()
        {
            return "Chunk #" + id[0] + id[1] + id[2] + id[3];
        }

    }
}
 