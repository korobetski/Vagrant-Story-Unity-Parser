using System.Collections;
using UnityEditor;
using UnityEngine;

namespace VS.FileFormats.EFFECT
{
    class EffectLoader:MonoBehaviour
    {
        public uint frameRate = 30;
        public EFFECT effect;
        private SpriteRenderer sp;

        private int i;

        // Use this for initialization
        void Start()
        {
            sp = GetComponent<SpriteRenderer>();
        }
        void Update()
        {
            StartCoroutine(VFX());
        }

        IEnumerator VFX()
        {
            if (sp == null) sp = GetComponent<SpriteRenderer>();
            if (effect != null)
            {
                sp.drawMode = SpriteDrawMode.Sliced;
                sp.spriteSortPoint = SpriteSortPoint.Pivot;
                i = 0;
                while (i < effect.p.sprites.Length)
                {
                    sp.sprite = effect.p.sprites[i].sprite;
                    sp.size = new Vector2(effect.p.sprites[i].destRect.width, effect.p.sprites[i].destRect.height);
                    i++;
                    yield return new WaitForSeconds(1 / frameRate);
                    yield return 0;

                }
                StartCoroutine(VFX());
            }
        }

        public void StartCoroutine()
        {
            StartCoroutine(VFX());
        }
    }


    [CustomEditor(typeof(EffectLoader))]
    public class FXLEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var fxl = target as EffectLoader;
            DrawDefaultInspector();
            if (GUILayout.Button("Start"))
            {
                fxl.StartCoroutine();
            }
        }
    }


}
