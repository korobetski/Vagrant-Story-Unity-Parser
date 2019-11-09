using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VS.Format
{
    //https://www.midi.org/specifications/item/xmf-specification-all
    //https://www.midi.org/specifications-old/item/registered-xmf-ids

    public class XMF
    {
        private string _fileID = "XMF_";    // 4 bytes
        private string _fileVers = "1.00";  // 4 bytes
        private VLQ _fileSize;         // In VLQ
        private VLQ _metaDatasSize;    // Ine VLQ;
        private List<byte> _metaDatas;
        private VLQ _treeStart;        // In VLQ
        private VLQ _treeEnd;          // In VLQ



        public XMF()
        {

        }

        public void Write(string path)
        {
            List<byte> buffer = Feed();

            using (FileStream fs = File.Create(path))
            {
                for (int i = 0; i < buffer.Count; i++)
                {
                    fs.WriteByte(buffer[i]);
                }
                fs.Close();
            }
        }

        private List<byte> Feed()
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(Encoding.ASCII.GetBytes(_fileID));
            buffer.AddRange(Encoding.ASCII.GetBytes(_fileVers));

            return buffer;
        }



        private class Tree
        {
            public Node rootNode;

            public Tree()
            {
                rootNode = new Node();
            }
        }

        private class Node
        {
            public VLQ length;         // VLQ
            public VLQ items;          // number of child nodes non recursive VLQ
            public VLQ contentOffset;  // Pointer to node content VLQ
            // NodeMetaData
            // NodesUnpackers
            

            public VLQ refTypeID;      // VLQ XMF DOC page 18 / 143
            public List<byte> refData;


            public Node()
            {

            }
        }


    }
}
