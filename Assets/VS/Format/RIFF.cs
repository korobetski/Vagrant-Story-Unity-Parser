using System;
using System.IO;
using System.Collections.Generic;
using VS.Utils;
using UnityEngine;

namespace VS.Format
{

    public class RIFF:ListTypeChunk
    {


        public RIFF(string form):base("RIFF", form)
        {
        }


        public static void AlignName(string name)
        {
            name += (char)0x00;
            if (name.Length % 2 > 0)     // if the size of the name string is odd
                name += (char)0x00;  // add another null byte
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


    public class ListTypeChunk : Chunk
    {
        public char[] type = new char[4];
        public List<Chunk> chunks;

        public ListTypeChunk(string sId, string sType) : base(sId)
        {
            size = 12; // id + size + type
            type = sType.ToCharArray();
            chunks = new List<Chunk>();
        }

        public ListTypeChunk(string sId, string sType, List<Chunk> lchunks) : base(sId)
        {
            type = sType.ToCharArray();
            chunks = lchunks;
            size = 12; // id + size + type
            foreach (Chunk c in chunks)
            {
                size += c.size;
            }
        }

        public void AddChunk(Chunk chunk)
        {
            chunks.Add(chunk);
            size += chunk.GetPaddedSize();
        }

    };


    public class LISTChunk : ListTypeChunk
    {
        public LISTChunk(string sType) : base("LIST", sType)
        {
        }

        public LISTChunk(string sType, List<Chunk> lchunks) : base("LIST", sType, lchunks)
        {
        }

    };
    public class Chunk
    {
        public char[] id = new char[4]; //  A chunk ID identifies the type of data within the chunk.
        public uint size = 0; //  The size of the chunk data in bytes, excluding any pad byte.
        public List<byte> data; //  The actual data not including a possible pad byte to word align

        public Chunk(string sId)
        {
            id = sId.ToCharArray();
        }

        public Chunk(string sId, List<byte> bData)
        {
            id = sId.ToCharArray();
            data = bData;
            size = (uint)data.Count;
        }

        public void SetData(List<byte> bData)
        {
            data = bData;
            size = (uint)data.Count;
        }

        public uint GetPaddedSize()
        {
            return size + size % 2;
        }

        public void Write(List<byte> buffer)
        {

        }

    }
}