using System;
using System.Collections.Generic;
using System.IO;

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
        public new uint headerSize = 12;

        public RIFF(string form) : base("RIFF", form)
        {
        }

        public static string AlignName(string name)
        {
            if (name.Length % 2 > 0)     // if the size of the name string is odd
            {
                name += (char)0x20;  // add another space byte
            }

            return name;
        }

        public bool WriteFile(string path, List<byte> buffer)
        {
            GetSize();
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
        uint GetHeaderSize();
        uint GetSize();
    }

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

        public void SetDataCapacity(int l)
        {
            data.Capacity = l;
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
            return Math.Max((uint)data.Capacity, (uint)data.Count);
        }

        public List<byte> Write()
        {
            GetSize();
            List<byte> buffer = new List<byte>();
            buffer.AddRange(new byte[] { (byte)id[0], (byte)id[1], (byte)id[2], (byte)id[3] });
            buffer.AddRange(BitConverter.GetBytes((uint)size));
            //buffer.AddRange(new byte[] { (byte)(size & 0x000000FF), (byte)((size & 0x0000FF00) >> 8), (byte)((size & 0x00FF0000) >> 16), (byte)((size & 0xFF000000) >> 24) });
            if (data != null)
            {
                buffer.AddRange(data);
            }
            return buffer;
        }

        public override string ToString()
        {
            return "Chunk #" + id[0] + id[1] + id[2] + id[3];
        }

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
            GetSize();
        }

        public IChunk AddChunk(IChunk chunk)
        {
            chunks.Add(chunk);
            GetSize();
            return chunk;
        }

        public new uint GetHeaderSize()
        {
            return headerSize;
        }

        public new uint GetSize()
        {
            size = 0;
            if (chunks != null && chunks.Count > 0)
            {
                foreach (IChunk ck in chunks)
                {
                    size += ck.GetHeaderSize() + ck.GetSize();
                }
            }
            return size;
        }

        public new List<byte> Write()
        {
            GetSize();
            List<byte> buffer = new List<byte>();
            buffer.AddRange(new byte[] { (byte)id[0], (byte)id[1], (byte)id[2], (byte)id[3] });
            buffer.AddRange(BitConverter.GetBytes((uint)size + 4));
            buffer.AddRange(new byte[] { (byte)type[0], (byte)type[1], (byte)type[2], (byte)type[3] });
            foreach (IChunk ck in chunks)
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


}
