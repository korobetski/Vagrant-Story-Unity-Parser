using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VS.Format
{
    //https://www.midi.org/specifications/item/xmf-specification-all
    //https://www.midi.org/specifications-old/item/registered-xmf-ids

    // Advice for XMF Writers Doc Page 48 / 143

    public class XMF
    {
        private string _fileID = "XMF_";    // 4 bytes
        private string _fileVers = "1.00";  // 4 bytes
        private VLQ _fileSize;         // In VLQ
        private VLQ _metaDatasSize;    // Ine VLQ;
        private List<byte> _metaDatas;
        private VLQ _treeStart;        // In VLQ
        private VLQ _treeEnd;          // In VLQ

        private Node _rootNode;

        /*
         * 
         * XMF version 1.00
         * File Header
         * |- Tree
         *      |- Root Node
         *              |- SMF Node
         *                      |- Meta datas
         *                      |- File
         *              |- DLS Node
         *                      |- Meta datas
         *                      |- File
         */


        public XMF()
        {
            _rootNode = new Node();
            _treeStart.Value = 0x20;
        }


        public void AttachSMFWithURI(string path)
        {
            Node smfNode = new Node();
            smfNode.refTypeID = new VLQ((int)Node.RefID.External_File);
            smfNode.refData = new XString(path).Bytes;
            _rootNode.AddNode(smfNode);
        }

        public void AttachDLSWithURI(string path)
        {

            Node dlsNode = new Node();
            dlsNode.refTypeID = new VLQ((int)Node.RefID.External_File);
            dlsNode.refData = new XString(path).Bytes;
            _rootNode.AddNode(dlsNode);
        }

        private List<byte> Feed()
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(Encoding.ASCII.GetBytes(_fileID));
            buffer.AddRange(Encoding.ASCII.GetBytes(_fileVers));
            _fileSize.Value = 0x20 + _rootNode.Resize();
            buffer.AddRange(_fileSize.Bytes);
            buffer.AddRange(_metaDatasSize.Bytes);
            buffer.AddRange(_treeStart.Bytes);
            _treeEnd.Value += _treeStart.Value + _rootNode.GetSize();
            buffer.AddRange(_treeEnd.Bytes);
            if (buffer.Count < 0x20)
            {
                buffer.AddRange(new byte[0x20 - buffer.Count]);
            }

            return buffer;
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



        private class Node
        {
            public enum RefID { Inline_Ressource = 1, Infile_Ressource = 2, Infile_Node = 3, External_File = 4, External_XMF = 5 };


            public VLQ size = new VLQ(0);       // Total node length in bytes, including node content.
            public VLQ itemNum = new VLQ(0);    // number of child nodes non recursive
            public VLQ contentOffset;           // Pointer to node content
            public NodeMetaData metadatas;      // NodeMetaData
            public NodeUnpackers unpackers;     // NodesUnpackers

            public VLQ refTypeID;               // VLQ XMF DOC page 18 / 143
            public List<byte> refData;


            private List<Node> _nodes;          //


            public Node()
            {

            }

            public void AddNode(Node subNode)
            {
                if (_nodes == null)
                {
                    _nodes = new List<Node>();
                }

                _nodes.Add(subNode);
                itemNum.Value = (uint)_nodes.Count;


                Resize();
            }

            public uint Resize()
            {
                size.Value = 0;
                size.Value += (uint)itemNum.Bytes.Count;

                contentOffset.Value = 0;
                contentOffset.Value += (uint)itemNum.Bytes.Count;

                if (itemNum.Value > 0)
                {
                    foreach (Node subNode in _nodes)
                    {
                        size.Value += subNode.Resize();
                    }
                }
                if (metadatas != null)
                {
                    size.Value += metadatas.GetSize();
                    contentOffset.Value += metadatas.GetSize();
                }
                if (unpackers != null)
                {
                    size.Value += unpackers.GetSize();
                    contentOffset.Value += unpackers.GetSize();
                }
                size.Value += (uint)refTypeID.Bytes.Count;
                size.Value += (uint)refData.Count;
                size.Value += (uint)contentOffset.Bytes.Count;


                contentOffset.Value += (uint)size.Bytes.Count;

                return size.Value;
            }

            internal uint GetSize()
            {
                return size.Value;
            }
        }

        private class NodeMetaData
        {
            private VLQ _length;
            private VLQ _count;
            private List<MetaEntry> _metadatas;

            public NodeMetaData()
            {
                _length = new VLQ(0);
                _count = new VLQ(0);
            }

            internal uint GetSize()
            {
                return _length.Value;
            }

            private class MetaEntry
            {
                private VLQ _type;
                private VLQ _format;
                private VLQ _lang;
            }
        }


        private class NodeUnpackers
        {
            private VLQ _length;
            private List<VLQ> _unpackers;

            public NodeUnpackers()
            {
                _length = new VLQ(0);
                _unpackers = new List<VLQ>();
                _unpackers.Add(new VLQ(0));
            }

            internal uint GetSize()
            {
                return _length.Value;
            }
        }
    }
}
