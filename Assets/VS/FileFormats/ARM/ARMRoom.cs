using System;
using UnityEngine;
using VS.FileFormats.GEOM;

namespace VS.FileFormats.ARM
{
    [Serializable]
    public class ARMRoom
    {
        public string name;

        public uint Length;
        public ushort ZoneId;
        public ushort MapId;

        public uint NumVertices;
        public Vertex[] Vertices;
        public uint NumTriangles;
        public Face[] Triangles;
        public uint NumQuads;
        public Face[] Quads;
        public uint NumFloorLines;
        public Line[] FloorLines;
        public uint NumCeilLines;
        public Line[] CeilLines;
        public uint NumMarkers;
        public ARMMarker[] Markers;

        public byte[] prev;
        public byte[] next;


        internal Vector3[] GetLinePositions(int lId, bool floor = true)
        {
            Vector3 p0;
            Vector3 p1;
            if (floor)
            {
                p0 = Vertices[FloorLines[lId].verticesId[0]].position;
                p1 = Vertices[FloorLines[lId].verticesId[1]].position;
            } else
            {
                p0 = Vertices[CeilLines[lId].verticesId[0]].position;
                p1 = Vertices[CeilLines[lId].verticesId[1]].position;
            }
            return new Vector3[] { p0, p1 };
        }

        public Vertex[] NegateVertices()
        {
            Vertex[] NVertices = new Vertex[NumVertices];
            for (uint i = 0; i < NumVertices; i++)
            {
                NVertices[i] = Vertices[i].Negate();
            }
            return NVertices;
        }
    }
}
