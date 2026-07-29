using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockEffectFlicker : MonoBehaviour
{
    SpriteRenderer render;
    float flickerWavePosRelation = 2.4f;
    float flickerWaveSpeed = 5f;

    private void Start()
    {
        render = GetComponent<SpriteRenderer>();

        Vector3 myPos = transform.position;
        float opacity = Mathf.Sin(Time.time * flickerWaveSpeed + (myPos.x + myPos.y) * flickerWavePosRelation) / 20f + 0.15f;
        Color myColor = render.color;
        myColor.a = opacity;
        render.color = myColor;
    }

    void Update()
    {
        Vector3 myPos = transform.position;
        float opacity = Mathf.Sin(Time.time * flickerWaveSpeed + (myPos.x + myPos.y) * flickerWavePosRelation) / 20f + 0.15f;
        Color myColor = render.color;
        myColor.a = opacity;
        render.color = myColor;
    }
}
