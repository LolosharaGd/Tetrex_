using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tetrex.DataStructures;
using TMPro;

public class TextObject : MonoBehaviour
{
    public TextMeshPro text;
    public TextMeshProUGUI textGUI;
    [SerializeField] bool isGUI = false;

    void Awake()
    {
        if (!isGUI)
            text = GetComponent<TextMeshPro>();
        else
            textGUI = GetComponent<TextMeshProUGUI>();
    }

    public void ChangeText(string targetText, float timeToEmpty, float timeToTarget = float.MaxValue)
    {
        // Duration of text dissapearing. Duration of text appearing
        float timeOut = timeToTarget == float.MaxValue ? timeToEmpty / 2f : timeToEmpty;
        float timeIn = timeToTarget == float.MaxValue ? timeToEmpty / 2f : timeToTarget;

        // Start changing text
        StopAllCoroutines();
        if (!isGUI)
        {
            if (text.text.Length == 0)
                StartCoroutine(CharChange(timeIn / targetText.Length, targetText, timeIn, true));
            else
                StartCoroutine(CharChange(timeOut / text.text.Length, targetText, timeIn, false));
        } else {
            if (textGUI.text.Length == 0)
                StartCoroutine(CharChange(timeIn / targetText.Length, targetText, timeIn, true));
            else
                StartCoroutine(CharChange(timeOut / textGUI.text.Length, targetText, timeIn, false));
        }
    }

    IEnumerator CharChange(float waitToNext, string targetText, float timeToTarget, bool addText)
    {
        if (!isGUI) // I have no idea how to do it without duplicationg the code
        {
            if (text.text.Length != 0 && !addText)                                                                  // If REMOVING TEXT and TEXT IS NOT EMPTY
                text.text = text.text.Remove(text.text.Length - 1);                                                 //   Remove one character
            else if (text.text != targetText && addText)                                                            // Else if ADDING TEXT and TEXT IS NOT COMPLETE
                text.text = text.text + targetText[text.text.Length];                                               //   Add one character
            yield return new WaitForSeconds(waitToNext);                                                            // Wait
            if (text.text.Length != 0 && !addText)                                                                  // If REMOVING TEXT and TEXT IS STILL NOT EMPTY
                StartCoroutine(CharChange(waitToNext, targetText, timeToTarget, false));                            //   Remove one character again
            else if (text.text.Length == 0 && !addText) {                                                           // If REMOVING TEXT and TEXT IS EMPTY
                if (targetText.Length != 0)                                                                         //   If TARGET TEXT IS NOT EMPTY
                    StartCoroutine(CharChange(timeToTarget / targetText.Length, targetText, timeToTarget, true)); } //     Start adding characters
            else if (text.text != targetText && addText)                                                            // If ADDING TEXT and TEXT IS NOT COMPLETE
                StartCoroutine(CharChange(waitToNext, targetText, timeToTarget, true));                             //   Add one character again
        } else
        {
            if (textGUI.text.Length != 0 && !addText)                                                               // If REMOVING TEXT and TEXT IS NOT EMPTY
                textGUI.text = textGUI.text.Remove(textGUI.text.Length - 1);                                        //   Remove one character
            else if (textGUI.text != targetText && addText)                                                         // Else if ADDING TEXT and TEXT IS NOT COMPLETE
                textGUI.text = textGUI.text + targetText[textGUI.text.Length];                                      //   Add one character
            yield return new WaitForSeconds(waitToNext);                                                            // Wait
            if (textGUI.text.Length != 0 && !addText)                                                               // If REMOVING TEXT and TEXT IS STILL NOT EMPTY
                StartCoroutine(CharChange(waitToNext, targetText, timeToTarget, false));                            //   Remove one character again
            else if (textGUI.text.Length == 0 && !addText) {                                                        // If REMOVING TEXT and TEXT IS EMPTY
                if (targetText.Length != 0)                                                                         //   If TARGET TEXT IS NOT EMPTY
                    StartCoroutine(CharChange(timeToTarget / targetText.Length, targetText, timeToTarget, true)); } //     Start adding characters
            else if (textGUI.text != targetText && addText)                                                         // If ADDING TEXT and TEXT IS NOT COMPLETE
                StartCoroutine(CharChange(waitToNext, targetText, timeToTarget, true));                             //   Add one character again
        }
    }
}
