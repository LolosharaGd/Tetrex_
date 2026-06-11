using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenAnchor : MonoBehaviour
{
    Camera cam;
    [SerializeField] Transform anchoredObject;
    /// <summary>
    /// True to change size preserving width, false to change size preserving height
    /// </summary>
    [SerializeField] bool anchorWidth;

    [SerializeField] Vector2 normalScreenSize = new Vector2(1280, 720);
    Vector2 screenSize;

    void Start()
    {
        cam = Camera.main;
        transform.localScale = anchoredObject.localScale;
        screenSize = new Vector2(cam.pixelWidth, cam.pixelHeight);
    }

    void Update()
    {
        anchoredObject.position = cam.WorldToScreenPoint(transform.position);

        float sizeMod = anchorWidth ? screenSize.x / normalScreenSize.x : screenSize.y / normalScreenSize.y;

        anchoredObject.localScale = transform.localScale * sizeMod;
    }

    void FixedUpdate()
    {
        if (cam.pixelWidth != screenSize.x)
            screenSize = new Vector2(cam.pixelWidth, cam.pixelHeight);
    }
}
