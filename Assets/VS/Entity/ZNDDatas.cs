using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VS.Entity
{
    public class ZNDDatas : MonoBehaviour
    {
        [SerializeField]
        public string filePath;
        [SerializeField]
        public Texture2D[] textures;
        [SerializeField]
        public Material[] materials;
        [SerializeField]
        public VSTIM[] tims;
        [SerializeField]
        public Vector3[] MPDInfos;
        [SerializeField]
        public Vector3[] ZUDInfos;


        public ZNDDatas()
        {
            textures = new Texture2D[0];
            materials = new Material[0];
            tims = new VSTIM[0];
            MPDInfos = new Vector3[0];
            ZUDInfos = new Vector3[0];
        }

        private Texture2D contains(string matIdx)
        {
            int lm = textures.Length;
            for (int i = 0; i < lm; i++)
            {
                if (textures[i].name == matIdx)
                {
                    return textures[i];
                }
            }

            return null;
        }

        public Texture2D getTexture(uint textureId, uint clutId)
        {
            string idx = textureId.ToString() + "_" + clutId.ToString();
            Texture2D texture = contains(idx);
            if (!texture)
            {
                texture = buildTexture(idx, textureId, clutId);
            }

            return texture;
        }

        private Texture2D buildTexture(string idx, uint textureId, uint clutId)
        {
            int lt = tims.Length;
            if (lt > 0)
            {

                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var buffer = new BinaryReader(fileStream);

                Texture2D texture;
                VSTIM toutim = getTIM(textureId);
                uint x = (clutId * 16) % 1024;
                uint y = (uint)Mathf.Floor((clutId * 16) / 1024);
                for (int i = 0; i < lt; i++)
                {
                    if (tims[i].fx <= x && tims[i].fx + tims[i].width > x && tims[i].fy <= y && tims[i].fy + tims[i].height > y)
                    {
                        Color32[] clut = tims[i].buildCLUT(x, y, buffer);
                        texture = toutim.buildTexture(clut, buffer);
                        texture.name = idx;
                        texture.anisoLevel = 4;
#if UNITY_EDITOR
                        texture.alphaIsTransparency = true;
#endif
                        texture.wrapMode = TextureWrapMode.Repeat;
                        texture.Compress(true);
                        List<Texture2D> texList = new List<Texture2D>(textures);
                        texList.Add(texture);
                        textures = texList.ToArray();
                        buffer.Close();
                        fileStream.Close();

                        Shader shader = Shader.Find("Standard");
                        Material mat = new Material(shader);
                        mat.SetTexture("_MainTex", texture);
                        mat.SetFloat("_Mode", 1);
                        mat.SetFloat("_Cutoff", 0.5f);
                        mat.SetFloat("_Glossiness", 0.0f);
                        mat.SetFloat("_SpecularHighlights", 0.0f);
                        mat.SetFloat("_GlossyReflections", 0.0f);
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        mat.SetInt("_ZWrite", 1);
                        mat.EnableKeyword("_ALPHATEST_ON");
                        mat.DisableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");


                        List<Material> mats = new List<Material>(materials);
                        mats.Add(mat);
                        materials = mats.ToArray();
#if UNITY_EDITOR
                        /*
                        AssetDatabase.AddObjectToAsset(texture, this.transform);
                        AssetDatabase.AddObjectToAsset(mat, this.transform);
                        AssetDatabase.SaveAssets();
                        */
#endif
                        return texture;
                    }
                }

                buffer.Close();
                fileStream.Close();
            }

            return null;
        }

        private VSTIM getTIM(uint idx)
        {
            uint x = (idx * 64) % 1024;
            //int y = Mathf.FloorToInt((idx * 64) / 1024);
            int lt = tims.Length;
            for (int i = 0; i < lt; i++)
            {
                if (tims[i].fx == x)
                {
                    return tims[i];
                }
            }

            return tims[0];
        }

        internal Material GetMaterial(uint textureId, uint clutId)
        {
            string idx = textureId.ToString() + "_" + clutId.ToString();

            int lm = textures.Length;
            for (int i = 0; i < lm; i++)
            {
                if (textures[i].name == idx)
                {
                    return materials[i];
                }
            }

            getTexture(textureId, clutId);
            return GetMaterial(textureId, clutId);
        }
    }

}
