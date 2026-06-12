using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Tetrex.DataStructures;

public enum VFXMode { INGAME, SHOP }

//[ExecuteAlways]
public class VFXManager : MonoBehaviour
{
    public PropertiesScriptableObject prop;

    public Camera blurredCamera;
    public Material gameBlurredMaterial;
    public GameObject gameBlurredQuad;
    public float blurSpeed;
    [SerializeField] float maxBlur;
    [SerializeField] float blurProgress;
    public bool blurringNow;
    float blurTimer;

    [SerializeField] Camera cam;
    [SerializeField] GameController controller;
    [SerializeField] ShopController shopController;

    // Camera background color
    public Material transitionBgMaterial;
    public Color newBgColor;

    // Camera shake
    /// <summary>
    /// Position changes per second
    /// </summary>
    public float shakeSpeed;
    public float shakeTimer;
    public float shakePower;
    public float shakeFade;

    public float IGTransitionProgress
    {
        get => transitionBgMaterial.GetFloat("_Transition_Progress");
    }

    // Shop BG
    public Material shopBgMaterial;
    public Vector3 shopBgCircle;
    public bool shopEndingAnimation;
    public bool roundEndingAnimation;
    public TextMeshPro[] shopText;
    public TextMeshProUGUI[] gameText;
    public TransitionTransformation[] shopTTs;
    public TransitionTransformation[] gameTTs;
    public List<TransitionTransformation> gameBlockTTs = new();
    /// <summary>
    /// Grid of all block effect TTs on the board
    /// </summary>
    public TransitionTransformation[,] beTTtGrid = new TransitionTransformation[10, 24];

    public float endAnimSpeed;

    public int curLevel;
    [SerializeField] List<Transform> ppbPoints = new();
    [SerializeField] List<SpriteRenderer> ppbSprites = new();
    [SerializeField] List<LineRenderer> ppbLines = new();

    [SerializeField] Sprite normalStageSprite;
    [SerializeField] Sprite completedStageSprite;
    [SerializeField] Sprite normalLevelSprite;
    [SerializeField] Sprite currentLevelSprite;
    [SerializeField] Sprite completedLevelSprite;
    [SerializeField] Sprite bossLevelSprite;
    [SerializeField] Sprite currentBossLevelSprite;

    float prevTransitionProgress;

    public VFXMode mode;

    void Awake()
    {
        cam = Camera.main;

        if (mode == VFXMode.INGAME)
        {
            ChangeBgColor();
            InGameUpdate();
        } else if (mode == VFXMode.SHOP)
        {
            shopBgMaterial.SetVector("_Noise_Speed", UnityEngine.Random.onUnitSphere);
        }
    }

    void Update()
    {
        if (mode == VFXMode.INGAME)
        {
            InGameUpdate();
        } else if (mode == VFXMode.SHOP)
        {
            InShopUpdate();
        }
    }

    void InGameUpdate()
    {
        // Blur
        blurTimer = Mathf.Clamp(blurTimer + Time.deltaTime * (blurringNow ? 1f : -1f) * blurSpeed, 0f, 1f);
        blurProgress = 1f - Mathf.Pow(blurTimer - 1f, 4);
        gameBlurredMaterial.SetFloat("_Step_Size", blurProgress * maxBlur);
        // Text transparecy later in function
        PauseMenuUpdate();

        // Properly resize the blurred quad
        Vector3 newGBQSize = new Vector3(blurredCamera.orthographicSize * 2f * blurredCamera.aspect, blurredCamera.orthographicSize * 2f, 1f);
        gameBlurredQuad.transform.localScale = newGBQSize;

        // Update camera bg color
        float camH, camS, camV;
        Color.RGBToHSV(transitionBgMaterial.GetColor("_Normal_Color"), out camH, out camS, out camV);
        float newH, newS, newV;
        Color.RGBToHSV(newBgColor, out newH, out newS, out newV);

        transitionBgMaterial.SetColor("_Normal_Color", Color.HSVToRGB(
            Mathf.LerpAngle(camH * 360f, newH * 360, prop.colorChangeSpeed) / 360f % 1,
            Mathf.Lerp(camS, newS, prop.colorChangeSpeed),
            Mathf.Lerp(camV, newV, prop.colorChangeSpeed)
        ));

        // Camera shake
        if (shakeTimer <= 0f)
        {
            shakeTimer = 1 / shakeSpeed;

            Vector2 randomPos = UnityEngine.Random.insideUnitCircle * shakePower;
            Vector3 camNewPos = new Vector3(randomPos.x, randomPos.y, -10f);
            blurredCamera.transform.position = camNewPos;
        }

        float transitionProgress = IGTransitionProgress;

        // Set all text transparency (including blur)
        foreach (var text in gameText)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Min(Mathf.Min(1f - transitionProgress * 5f, 1f), 1f - blurProgress));
        }

        // Apply transition transformations
        float transitionScaling = transitionProgress >= 0.95f ? 10000f : Mathf.Pow(Mathf.Tan(transitionProgress * Mathf.PI / 2f), 3f);
        foreach (var transformation in gameTTs)
        {
            transformation.gameObject.transform.position = transformation.startPosition + transformation.direction * transitionScaling;
        }

        // Apply transition transformations on blocks if transition is happening
        if (prevTransitionProgress > 0f)
        foreach (var transformation in gameBlockTTs)
        {
            transformation.gameObject.transform.position = transformation.startPosition + transformation.direction * transitionScaling;
        }

        // Apply transition transformations on block effects
        if (prevTransitionProgress > 0f)
        foreach (var transformation in beTTtGrid)
        {
            if (transformation != null)
            {
                transformation.gameObject.transform.position = transformation.startPosition + transformation.direction * transitionScaling;
            }
        }

        // Transition
        if (roundEndingAnimation)
        {
            transitionBgMaterial.SetFloat("_Transition_Progress", transitionProgress + Time.deltaTime * endAnimSpeed);

            if (IGTransitionProgress >= 1f)
            {
                SceneManager.LoadScene(1);
            }
        }
        else
        {
            transitionBgMaterial.SetFloat("_Transition_Progress", Mathf.Max(0f, transitionProgress - Time.deltaTime * endAnimSpeed));
        }

        shakeTimer -= Time.deltaTime;
        shakePower -= shakeFade * Time.deltaTime;
        shakePower = Mathf.Max(shakePower, 0f);

        prevTransitionProgress = transitionProgress;
    }

    void PauseMenuUpdate()
    {
        float normalKerning = 0.7f;
        float instageKerning = 0.5f;
        float totalWidth = normalKerning * 6 + instageKerning;
        float vertOffs = 0.25f;

        float transitionOffset = 1f;

        float accumulatedKerning = 0f;

        for (int i = 0; i < 9; i++)
        {
            // Points
            ppbSprites[i].color = new Color(ppbSprites[i].color.r, ppbSprites[i].color.g, ppbSprites[i].color.b, Mathf.Min(1f, blurProgress * 2f));

            Vector3 offs = Vector3.zero;

            // If this point is a completed stage
            if (i + 1 < LevelToStage(curLevel))
            {
                offs = new Vector3(totalWidth / -2f + accumulatedKerning, 0f, 0f);
                accumulatedKerning += normalKerning;
            }
            // If this point is in current stage
            else if (i + 1 < LevelToStage(curLevel) + 3)
            {
                if (i + 1 == LevelToStage(curLevel)) offs = new Vector3(totalWidth / -2f + accumulatedKerning, vertOffs, 0f); // First
                if (i + 1 == LevelToStage(curLevel) + 1) { offs = new Vector3(totalWidth / -2f + accumulatedKerning, -vertOffs, 0f); accumulatedKerning += instageKerning; } // Second
                if (i + 1 == LevelToStage(curLevel) + 2) { offs = new Vector3(totalWidth / -2f + accumulatedKerning, 0f, 0f); accumulatedKerning += normalKerning; } // Boss
            }
            // If this point is an upcoming stage
            else
            {
                offs = new Vector3(totalWidth / -2f + accumulatedKerning, 0f, 0f);
                accumulatedKerning += normalKerning;
            }

            offs.z -= cam.transform.position.z;

            float angle = (Time.time * 2.3f * (1f + i / 9f)) + Mathf.Pow(i, 3f) * 1.4f;
            offs += new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * Mathf.Max(1f - blurProgress, 0.01f);

            ppbPoints[i].position = cam.transform.position + offs;

            // Lines
            if (i != 8)
            {
                ppbLines[i].startWidth = 0.18181818f * Mathf.Max(0f, blurProgress * 2f - 1f);
                ppbLines[i].SetPositions(new Vector3[] { ppbPoints[i].position + Vector3.forward, ppbPoints[i + 1].position + Vector3.forward });
            }
        }
    }

    public void UpdatePPBPointsTextures()
    {
        for (int i = 0; i < 9; i++)
        {
            // If this point is a completed stage
            if (i + 1 < LevelToStage(curLevel))
            {
                ppbSprites[i].sprite = completedStageSprite;
            }
            // If this point is in current stage
            else if (i + 1 < LevelToStage(curLevel) + 3)
            {
                if (i + 1 == LevelToStage(curLevel)) // First
                {
                    ppbSprites[i].sprite = LevelToLevelInStage(curLevel) == 1 ? currentLevelSprite : completedLevelSprite;
                }
                if (i + 1 == LevelToStage(curLevel) + 1) // Second
                {
                    if      (LevelToLevelInStage(curLevel) == 1) ppbSprites[i].sprite = normalLevelSprite;
                    else if (LevelToLevelInStage(curLevel) == 2) ppbSprites[i].sprite = currentLevelSprite;
                    else                                         ppbSprites[i].sprite = completedLevelSprite;
                }
                if (i + 1 == LevelToStage(curLevel) + 2) // Boss
                {
                    ppbSprites[i].sprite = LevelToLevelInStage(curLevel) == 3 ? currentBossLevelSprite : bossLevelSprite;
                }
            }
            // If this point is an upcoming stage
            else
            {
                ppbSprites[i].sprite = normalStageSprite;
            }
        }
    }

    void InShopUpdate()
    {
        // Scroll switch mods
        float rawSwitchMod = Mathf.Cos(shopController.blockScrollSwitchTimer * Mathf.PI / 2f);
        float shopScrollSwitchMod = shopController.shopScrollSelected ? rawSwitchMod : 1f - rawSwitchMod;
        float equippedScrollSwitchMod = shopController.shopScrollSelected ? 1f - rawSwitchMod : rawSwitchMod;

        // Set BG targets
        shopBgMaterial.SetVector("_Target_1", shopController.shopBlocksStart.position + Vector3.forward * 10f);
        shopBgMaterial.SetVector("_Target_2", shopController.equippedBlocksStart.position + Vector3.forward * 10f);
        shopBgMaterial.SetFloat("_Target_1_Size", 2.25f * shopScrollSwitchMod);
        shopBgMaterial.SetFloat("_Target_2_Size", 2.25f * equippedScrollSwitchMod);

        float transitionProgress = shopBgMaterial.GetFloat("_Transition_Progress");

        // Set all text transparency
        foreach (var text in shopText)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Min(1f - transitionProgress * 5f, 1f));
        }

        // Apply transition transformations
        float transitionScaling = transitionProgress >= 0.9f ? 100f : Mathf.Pow(Mathf.Tan(transitionProgress * Mathf.PI / 2f), 3f);
        foreach (var transformation in shopTTs)
        {
            transformation.gameObject.transform.position = transformation.startPosition + transformation.direction * transitionScaling;
        }

        // Camera shake
        if (shakeTimer <= 0f)
        {
            shakeTimer = 1 / shakeSpeed;

            Vector2 randomPos = UnityEngine.Random.insideUnitCircle * shakePower;
            Vector3 camNewPos = new Vector3(randomPos.x, randomPos.y, -10f);
            cam.transform.position = camNewPos;
        }

        // End animation
        if (shopEndingAnimation)
        {
            shopBgMaterial.SetFloat("_Transition_Progress", shopBgMaterial.GetFloat("_Transition_Progress") + Time.deltaTime * endAnimSpeed);

            if (shopBgMaterial.GetFloat("_Transition_Progress") >= 1f)
            {
                SceneManager.LoadScene(0);
            }
        }
        else
        {
            shopBgMaterial.SetFloat("_Transition_Progress", Mathf.Max(0f, shopBgMaterial.GetFloat("_Transition_Progress") - Time.deltaTime * endAnimSpeed));
        }

        shakeTimer -= Time.deltaTime;
        shakePower -= shakeFade * Time.deltaTime;
        shakePower = Mathf.Max(shakePower, 0f);
    }

    public void ChangeBgColor()
    {
        float minHue, minSaturation, minValue;
        float maxHue, maxSaturation, maxValue;

        Color.RGBToHSV(prop.minColor, out minHue, out minSaturation, out minValue);
        Color.RGBToHSV(prop.maxColor, out maxHue, out maxSaturation, out maxValue);

        float newHue = Rand(minHue - (minHue > maxHue ? 1 : 0), maxHue);

        newBgColor = Color.HSVToRGB(newHue, Rand(minSaturation, maxSaturation), Rand(minValue, maxValue));
    }

    public void CameraShake(float power, float time, float speed = 0f)
    {
        shakePower = power;
        shakeSpeed = speed == 0 ? shakeSpeed : speed;
        shakeFade = power / time;
        shakeTimer = 0f;
    }

    float Rand(float x, float y)
    {
        return UnityEngine.Random.Range(x, y);
    }

    public int LevelToStage(int level)
    {
        return ((level - 1) / 3) + 1;
    }

    public int LevelToLevelInStage(int level)
    {
        return level - (LevelToStage(level) - 1) * 3;
    }
}
