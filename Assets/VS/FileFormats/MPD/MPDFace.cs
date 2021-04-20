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
        public bool doubleSided = false;
        public bool translucent = false;

        public MPDFace(BinaryReader buffer)
        {
            Vector3 vertex1 = new Vector3(buffer.ReadInt16(), buffer.ReadInt16(), buffer.ReadInt16());
            Vector3 vertex2 = new Vector3(buffer.ReadSByte(), buffer.ReadSByte(), buffer.ReadSByte());
            Vector3 vertex3 = new Vector3(buffer.ReadSByte(), buffer.ReadSByte(), buffer.ReadSByte());
            Color32 color1 = new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 255);
            type = buffer.ReadByte();
            /*
            # types :
            # 34 is a triangle
            # 35 double sided tri ?
            # 36 is maybe a translucent triangle
            # 37 double sided translucent tri ?
            # -- maybe its never used
            # 38 v colored without texture ?
            # 39 ? v colored double
            # 3A ? v colored with translucent
            # 3B ? v colored with translucent

            # 3C is a quad
            # 3D is maybe double sided
            # 3E is maybe translucent
            # 3F is maybe double sided and translucent
            # 40 hard crash !
            */
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
                uv4.y = buffer.ReadByte();

                vertices = new Vector3[] { vertex1 , vertex2, vertex3, vertex4 };
                colors = new Color32[] { color1, color2, color3, color4 };
                uvs = new Vector2[] { uv1, uv2, uv3, uv4 };
                if (type == 0x3D || type == 0x3F) doubleSided = true;
                if (type == 0x3E || type == 0x3F) translucent = true;
            } else
            {
                vertices = new Vector3[] { vertex1, vertex2, vertex3 };
                colors = new Color32[] { color1, color2, color3 };
                uvs = new Vector2[] { uv1, uv2, uv3 };
                if (type == 0x35 || type == 0x37) doubleSided = true;
                if (type == 0x36 || type == 0x37) translucent = true;
            }
        }

        public bool isQuad { get => (type >= 0x3C); }
        public string materialRef { get => string.Concat(textureId, '@', palettePtr); }

        public Vector3 GetOpVertex(MPDGroup group, uint vId)
        {
            Vector3 position = new Vector3();
            switch (vId)
            {
                case 0:
                    position = group.position + vertices[0];
                    break;
                case 1:
                    position = group.position + vertices[0] + vertices[1] * group.scale;
                    break;
                case 2:
                    position = group.position + vertices[0] + vertices[2] * group.scale;
                    break;
                case 3:
                    position = group.position + vertices[0] + vertices[3] * group.scale;
                    break;
                default:
                    position = group.position + vertices[0];
                    break;
            }
            position.y = -position.y;
            return position;
        }
    }
}
