using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VS.Parser.Effect;

public class SpriteAnimator : MonoBehaviour
{
    public uint frameRate = 30;
    public List<EffectFrame> frames;
    private SpriteRenderer renderer;

    // Use this for initialization
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(VFX());
    }

    IEnumerator VFX()
    {
        if  (frames != null)
        {
            int i;
            i = 0;
            while (i < frames.Count)
            {
                renderer.sprite = frames[i].sprite;
                renderer.size = new Vector2(frames[i].destRect.width, frames[i].destRect.height);
                i++;
                yield return new WaitForSeconds(1 / frameRate);
                yield return 0;

            }
            StartCoroutine(VFX());
        }
    }
}
