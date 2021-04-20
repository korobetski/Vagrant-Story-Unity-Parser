using System;
using System.IO;
using UnityEngine;

namespace VS.FileFormats.MPD
{

    [Serializable]
    public class MPDFace
    {
        public Vector3[] vertices;
        public Color32[] colors;
        public Vector2[] uvs;
        public byte type;
        public ushort palettePtr;
        public ushort textureId;

        public MPDFace(BinaryReader buffer)
        {
            Vector3 vertex1 = new Vector3(buffer.ReadInt16(), buffer.ReadInt16(), buffer.ReadInt16());
            Vector3 vertex2 = new Vector3(buffer.ReadSByte(), buffer.ReadSByte(), buffer.ReadSByte());
            Vector3 vertex3 = new Vector3(buffer.ReadSByte(), buffer.ReadSByte(), buffer.ReadSByte());
            Color32 color1 = new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 255);
            type = buffer.ReadByte();
            Color32 color2 = new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 255);
            Vector2 uv1 = new Vector2(buffer.ReadByte(), 0);
            Color32 color3 = new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 255);
            uv1.y = buffer.ReadByte();
            Vector2 uv2 = new Vector2(buffer.ReadByte(), buffer.ReadByte());
            palettePtr = buffer.ReadUInt16();
            Vector2 uv3 = new Vector2(buffer.ReadByte(), buffer.ReadByte());
            textureId = buffer.ReadUInt16();

            if (type >= 0x3C)
            {
                // its a quad
                Vector3 vertex4 = new Vector3(buffer.ReadSByte(), buffer.ReadSByte(), buffer.ReadSByte());
                Vector2 uv4 = new Vector2(buffer.ReadByte(), 0);
                Color32 color4 = new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 255);
                uv1.y = buffer.ReadByte();

                vertices = new Vector3[] { vertex1 , vertex2, vertex3, vertex4 };
                colors = new Color32[] { color1, color2, color3, color4 };
                uvs = new Vector2[] { uv1, uv2, uv3, uv4 };
            } else
            {
                vertices = new Vector3[] { vertex1, vertex2, vertex3 };
                colors = new Color32[] { color1, color2, color3 };
                uvs = new Vector2[] { uv1, uv2, uv3 };
            }
        }

        public bool isQuad { get => (type >= 0x3C); }
        public string materialRef { get => string.Concat(textureId, '@', palettePtr); }

        public Vector3 GetOpVertex(MPDGroup group, uint vId)
        {
            switch (vId)
            {
                case 0:
                    return group.position + vertices[0];
                case 1:
                    return group.position + vertices[0] + vertices[1] * group.scale;
                case 2:
                    return group.position + vertices[0] + vertices[2] * group.scale;
                case 3:
                    return group.position + vertices[0] + vertices[3] * group.scale;
                default:
                    return group.position + vertices[0];
            }
            
        }
    }
}
