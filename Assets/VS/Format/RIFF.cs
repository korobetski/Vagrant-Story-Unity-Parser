using System;
using System.IO;
using System.Collections.Generic;
using VS.Utils;

namespace VS.Format
{
    /// <summary>
    /// Class representing a generic RIFF-Like file
    /// </summary>
    public class RIFF
    {
        public char[] fileIdentifier;

        public char[] format;

        Dictionary<char[], Chunk> chunks = new Dictionary<char[], Chunk>(new ByteArrayValueComparer());

        /// <summary>
        /// Initializes a new instance of the Riff class.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public RIFF(string fileName)
        {
            byte[] raw = File.ReadAllBytes(fileName);

            fileIdentifier = new char[4];

            Array.Copy(raw, fileIdentifier, 4);

            //Debug. Print identifier
            char[] testArray = new char[4];
            Array.Copy(raw, 8, testArray, 0, 4);
            Console.WriteLine(testArray);
        }

        public RIFF(string fileName, bool forceRIFF)
        {
            byte[] raw = File.ReadAllBytes(fileName);

            fileIdentifier = new char[4];

            Array.Copy(raw, fileIdentifier, 4);

            //Load
            Chunk c = new Chunk();
            List<Chunk> ch = c.Initialize(Util.Slice(raw, 8, raw.Length));

            Dictionary<char[], int> chunkTypeCount = new Dictionary<char[], int>();

            foreach (Chunk chunk in ch)
            {
                int chunkCount = 0;
                chunkTypeCount.TryGetValue(chunk.identifier, out chunkCount);
                Console.WriteLine(chunkCount);

                char[] numeral = chunkCount.ToString().ToCharArray();
                char[] key = new char[4 + numeral.Length];
                key[0] = chunk.identifier[0];
                key[1] = chunk.identifier[1];
                key[2] = chunk.identifier[2];
                key[3] = chunk.identifier[3];

                Array.Copy(numeral, 0, key, 4, numeral.Length);


                Console.WriteLine(key);
                chunks.Add(key, chunk);

                chunkTypeCount[chunk.identifier] = ++chunkCount;
            }
        }

        public Chunk GetChunk(string identifier)
        {
            return GetChunk(identifier.ToCharArray());
        }

        public Chunk GetChunk(string identifier, int number)
        {
            return GetChunk(identifier.ToCharArray(), number);
        }

        /// <summary>
        /// Get a chunk.
        /// </summary>
        /// <returns>The zeroth chunk with given identifier.</returns>
        /// <param name="identifier">Chunk type identifier.</param>
        public Chunk GetChunk(char[] identifier)
        {
            return GetChunk(identifier, 0);
        }

        /// <summary>
        /// Get a chunk
        /// </summary>
        /// <returns>The nth chunk with given identifier.</returns>
        /// <param name="identifier">Chunk type identifier.</param>
        /// <param name="number">Which chunk of given type.</param>
        public Chunk GetChunk(char[] identifier, int number)
        {
            return chunks[new char[] { identifier[0], identifier[1], identifier[2], identifier[3], '0' }];
        }
    }

    /// <summary>
    /// Represents a loaded chunk
    /// </summary>
    public class Chunk
    {
        public char[] identifier;
        public uint contentLength;
        public uint childrenLength;

        public byte[] contents;

        public Chunk parent;
        public List<Chunk> children;

        public List<Chunk> Initialize(byte[] data, Chunk parent)
        {
            this.parent = parent;
            return Initialize(data);
        }

        public List<Chunk> Initialize(byte[] data)
        {
            identifier = new char[4];

            Array.Copy(data, identifier, 4);
            Console.WriteLine(identifier);

            contentLength = BitConverter.ToUInt32(data, 4);
            childrenLength = BitConverter.ToUInt32(data, 8);

            contents = new byte[contentLength];
            Array.Copy(data, 12, contents, 0, contentLength);

            //Create children
            uint startPointer = 12 + contentLength;

            children = new List<Chunk>();

            while (startPointer < childrenLength)
            {
                Chunk c = new Chunk();
                List<Chunk> ch = c.Initialize(Util.Slice(data, startPointer, (uint)data.Length), this);

                children.AddRange(ch);

                startPointer += ch[0].contentLength + ch[0].childrenLength + 12;
            }

            List<Chunk> output = new List<Chunk>();
            output.Add(this);
            output.AddRange(children);


            return output;
        }

        public byte[] GetContents()
        {
            return contents;
        }

        public override string ToString()
        {
            return identifier.ToString();
        }


    }
}