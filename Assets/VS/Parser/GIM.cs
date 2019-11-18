using UnityEngine;

namespace VS.Parser
{

    public class GIM : FileParser
    {
        public Texture2D texture;

        public GIM(string path)
        {
            UseDebug = true;
            Parse(path);
        }


        public void Parse(string filePath)
        {
            if (!filePath.EndsWith(".GIM"))
            {
                return;
            }

            PreParse(filePath);

            buffer.ReadBytes(16);
            return;

            // compressed pictures, i don't know how to decompress it right now
            // datas seems to be packed in 128 bytes blocks


            /*
            int width = 128;
            int height = 128;
            int size = width * height;
            int pad = Mathf.FloorToInt(FileSize / size);
            height *= pad;
            size = width * height;

            Color[] clut = new Color[size];
            for (int i = 0; i < size; ++i)
            {
                float g = buffer.ReadByte() / 255;
                clut[i] = new Color(g, g, g, 1f);
            }

            texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            texture.SetPixels(clut);
            texture.Apply();

            byte[] bytes = texture.EncodeToPNG();
            ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/GIM/");
            File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/GIM/" + FileName + ".png", bytes);
            */
        }

    }
}
