using System;
using System.Collections.Generic;
using UnityEngine;

namespace VS.FileFormats.MPD
{
    [Serializable]
    public class MPDTile
    {
        public byte floorHeight;
        public byte floorMode;
        public byte ceilHeight;
        public byte ceilMode;
        public byte[] properties;

        private uint _id;
        private uint _x;
        private uint _y;
        private List<float> _heigths;
        private List<float> _ceilHeigths;

        public uint id { get => _id; set => _id = value; }
        public uint x { get => _x; set => _x = value; }
        public uint y { get => _y; set => _y = value; }
        public List<float> heigths { get => _heigths; set => _heigths = value; }
        public List<float> ceilHeigths { get => _ceilHeigths; set => _ceilHeigths = value; }

    }
}
