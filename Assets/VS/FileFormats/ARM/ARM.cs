using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.FileFormats.GEOM;

namespace VS.FileFormats.ARM
{
    public class ARM:ScriptableObject
    {
        // http://datacrystal.romhacking.net/wiki/Vagrant_Story:ARM_files
        // http://datacrystal.romhacking.net/wiki/Vagrant_Story:areas_list
        // MINI MAP File format in SMALL/ folder
        // This File Format is near 100% explored

        public string filename;
        public uint NumRooms;
        public ARMRoom[] Rooms;

        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            filename = fp.FileName;
            ParseFromBuffer(fp.buffer, fp.FileSize);

            fp.Close();
        }

        public void ParseFromBuffer(BinaryReader buffer, long limit)
        {
            NumRooms = buffer.ReadUInt32();
            Rooms = new ARMRoom[NumRooms];
            for (int i = 0; i < NumRooms; i++)
            {
                ARMRoom room = new ARMRoom();
                room.name = "Room_" + i;
                buffer.ReadUInt32(); // always 0
                room.Length = buffer.ReadUInt32(); // lenght of map graphics section (RAM: pointer to section)
                room.ZoneId = buffer.ReadUInt16();
                room.MapId = buffer.ReadUInt16();
                Rooms[i] = room;
            }

            for (int i = 0; i < NumRooms; i++)
            {
                Rooms[i].NumVertices = buffer.ReadUInt32();
                Rooms[i].Vertices = new Vertex[Rooms[i].NumVertices];
                for (int j = 0; j < Rooms[i].NumVertices; j++)
                {
                    Vertex vertex = new Vertex();
                    vertex.position = new Vector4(buffer.ReadInt16(), buffer.ReadInt16(), buffer.ReadInt16(), buffer.ReadInt16());
                    Rooms[i].Vertices[j] = vertex;
                }

                Rooms[i].NumTriangles = buffer.ReadUInt32();
                Rooms[i].Triangles = new Face[Rooms[i].NumTriangles];
                for (int j = 0; j < Rooms[i].NumTriangles; j++)
                {
                    Face face = new Face();
                    face.verticesCount = 3;
                    face.type = 0x24;
                    face.side = 8;
                    face.vertices = new List<ushort>();
                    face.vertices.Add(buffer.ReadByte());
                    face.vertices.Add(buffer.ReadByte());
                    face.vertices.Add(buffer.ReadByte());
                    face.vertices.Add(buffer.ReadByte());
                    Rooms[i].Triangles[j] = face;
                }
                Rooms[i].NumQuads = buffer.ReadUInt32();
                Rooms[i].Quads = new Face[Rooms[i].NumQuads];
                for (int j = 0; j < Rooms[i].NumQuads; j++)
                {
                    Face face = new Face();
                    face.verticesCount = 4;
                    face.type = 0x2C;
                    face.side = 8;
                    face.vertices = new List<ushort>();
                    face.vertices.Add(buffer.ReadByte());
                    face.vertices.Add(buffer.ReadByte());
                    face.vertices.Add(buffer.ReadByte());
                    face.vertices.Add(buffer.ReadByte());
                    Rooms[i].Quads[j] = face;
                }

                Rooms[i].NumFloorLines = buffer.ReadUInt32();
                Rooms[i].FloorLines = new Line[Rooms[i].NumFloorLines];
                for (int j = 0; j < Rooms[i].NumFloorLines; j++)
                {
                    Line line = new Line();
                    line.verticesId[0] = buffer.ReadByte();
                    line.verticesId[1] = buffer.ReadByte();
                    line.pad = buffer.ReadUInt16();
                    Rooms[i].FloorLines[j] = line;
                }

                Rooms[i].NumCeilLines = buffer.ReadUInt32();
                Rooms[i].CeilLines = new Line[Rooms[i].NumCeilLines];
                for (int j = 0; j < Rooms[i].NumCeilLines; j++)
                {
                    Line line = new Line();
                    line.verticesId[0] = buffer.ReadByte();
                    line.verticesId[1] = buffer.ReadByte();
                    line.pad = buffer.ReadUInt16();
                    Rooms[i].CeilLines[j] = line;
                }

                Rooms[i].NumMarkers = buffer.ReadUInt32();
                Rooms[i].Markers = new ARMMarker[Rooms[i].NumMarkers];

                for (int j = 0; j < Rooms[i].NumMarkers; j++)
                {
                    Rooms[i].Markers[j] = new ARMMarker();
                    Rooms[i].Markers[j].SetDatas(buffer.ReadBytes(4));
                }
            }

            if (buffer.BaseStream.Position < limit)
            {
                // we seek rooms name
                for (int i = 0; i < NumRooms; i++)
                {
                    if (buffer.BaseStream.Position + 36 <= limit)
                    {
                        Rooms[i].prev = buffer.ReadBytes(6);
                        Rooms[i].name = Utils.L10n.CleanTranslate(buffer.ReadBytes(24));
                        Rooms[i].next = buffer.ReadBytes(6);
                    }
                }
            }
        }
    }
}
