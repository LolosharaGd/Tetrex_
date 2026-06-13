using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class MenuBox : MonoBehaviour
{
    /// <summary>
    /// Size of the box in grid blocks (0.4 units each, the sides are not counted)
    /// </summary>
    public Vector2 size;
    public Vector2 extraPixelSize;
    public bool includeEdges;
    [SerializeField] Transform cTR;
    [SerializeField] Transform cTL;
    [SerializeField] Transform cBR;
    [SerializeField] Transform cBL;
    [SerializeField] Transform sT;
    [SerializeField] Transform sR;
    [SerializeField] Transform sB;
    [SerializeField] Transform sL;
    [SerializeField] Transform center;
    public bool updateSize;

    void Update()
    {
        if (updateSize)
        {
            size = Vector2.Max(size, Vector2.one / 11f);
            Vector2 sizeInBlocks = size * 0.4f - (includeEdges ? Vector2.one * 0.4f / 11f * 4f : Vector2.zero) + extraPixelSize/11f*0.8f;
            sizeInBlocks = Vector2.Max(sizeInBlocks, Vector3.one * 0.4f / 11f);
            center.localScale = V2toV3(sizeInBlocks * 11f, 1f);

            sT.localPosition = new Vector3(0f                       , sizeInBlocks.y / 2f + 0.4f / 11f , 0f);
            sR.localPosition = new Vector3(sizeInBlocks.x / 2f + 0.4f / 11f , 0f                       , 0f);
            sB.localPosition = new Vector3(0f                       , -sizeInBlocks.y / 2f - 0.4f / 11f, 0f);
            sL.localPosition = new Vector3(-sizeInBlocks.x / 2f - 0.4f / 11f, 0f                       , 0f);

            cTR.localPosition = new Vector3(sizeInBlocks.x / 2f + 0.4f / 11f , sizeInBlocks.y / 2f + 0.4f / 11f , 0f);
            cTL.localPosition = new Vector3(-sizeInBlocks.x / 2f - 0.4f / 11f, sizeInBlocks.y / 2f + 0.4f / 11f , 0f);
            cBR.localPosition = new Vector3(sizeInBlocks.x / 2f + 0.4f / 11f , -sizeInBlocks.y / 2f - 0.4f / 11f, 0f);
            cBL.localPosition = new Vector3(-sizeInBlocks.x / 2f - 0.4f / 11f, -sizeInBlocks.y / 2f - 0.4f / 11f, 0f);

            sT.localScale = new Vector3(sizeInBlocks.x * 11f, 0.4f, 1f);
            sL.localScale = new Vector3(sizeInBlocks.y * 11f, 0.4f, 1f);
            sB.localScale = new Vector3(0.4f, sizeInBlocks.x * 11f, 1f);
            sR.localScale = new Vector3(0.4f, sizeInBlocks.y * 11f, 1f);
        }
    }

    Vector3 V2toV3(Vector2 v, float z)
    {
        return new Vector3(v.x, v.y, z);
    }
}
