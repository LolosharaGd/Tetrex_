using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] PropertiesScriptableObject prop;

    [SerializeField] GameController controller;
    [SerializeField] SoundController soundController;
    [SerializeField] SaveManager saveManager;

    /// <summary>
    /// Score
    /// </summary>
    public int score;
    /// <summary>
    /// TMP component where to write the score
    /// </summary>
    public TextMeshProUGUI scoreText;
    /// <summary>
    /// TMP component where to write the score goal
    /// </summary>
    public TextMeshProUGUI goalText;

    /// <summary>
    /// GameObject of the label that says COMBO
    /// </summary>
    public GameObject comboLabel;
    /// <summary>
    /// TMPro component where to write the combo
    /// </summary>
    public TextMeshProUGUI comboText;

    /// <summary>
    /// Combo of how many block lands in a row have coused at least one row clear. Added score is scaled according to the combo
    /// </summary>
    public int combo;

    [SerializeField] GameObject clearLabel;
    float[] winAnimRowClearTimes = new float[20];

    /// <summary>
    /// Score goal that, when reached, ends the level
    /// </summary>
    public int scoreGoal;

    float winAnimationTimer;
    float prevWinAnimTimer;

    void Start()
    {
        for (int i = 0; i < winAnimRowClearTimes.Length; i++)
        {
            winAnimRowClearTimes[i] = 2f + (i * 0.1f);
        }

        string text = Mathf.Clamp(scoreGoal, 0, 99999) + "";
        for (int i = text.Length; i < 5; i++) text = "0" + text;
        goalText.text = text;
    }

    public void WinAnimationTick(float delta)
    {
        if (winAnimationTimer == 0f) // Setup
        {
            clearLabel.SetActive(true);
        }
        else if (winAnimationTimer < 4f) // Rows clearing
        {
            // If the grid is clear, skip to next. Otherwise - clear a row
            bool gridIsClear = true;
            for (int y = 0; y < controller.blockGrid.GetLength(1); y++)
            {
                for (int x = 0; x < controller.blockGrid.GetLength(0); x++)
                {
                    if (controller.blockGrid[x, y] != null)
                    {
                        gridIsClear = false;
                        break;
                    }
                }

                if (!gridIsClear) break;
            }

            if (!gridIsClear)
            {
                foreach (float winAnimRowClearTime in winAnimRowClearTimes)
                {
                    if (WinTimerJustReached(winAnimRowClearTime))
                    {
                        // Check if bottom row is clear
                        bool bottomRowIsClear = true;
                        for (int i = 0; i < controller.blockGrid.GetLength(0); i++) if (controller.blockGrid[i, 0] != null) bottomRowIsClear = false;

                        // Clear row
                        controller.ClearRow(0);

                        // If the row had blocks, play the sound and add score
                        if (!bottomRowIsClear)
                        {
                            AddScoreFromLines(1, true);
                            soundController.PlayRowClearSound(1);
                        }
                    }
                }
            }
            else
            {
                winAnimationTimer = 4f;
            }
        }
        else if (winAnimationTimer >= 5f && prevWinAnimTimer < 5f) // After rows are cleared
        {
            saveManager.RoundEnd();
        }

        prevWinAnimTimer = winAnimationTimer;
        winAnimationTimer += delta;
    }

    public bool WinTimerJustReached(float time)
    {
        return winAnimationTimer >= time && prevWinAnimTimer < time;
    }

    /// <summary>
    /// Universal function to add score and increase combo
    /// </summary>
    /// <param name="clearedLines">Reward for how many cleared lines should be given</param>
    public void AddScoreFromLines(int clearedLines, bool ignoreCombo = false)
    {
        if (!ignoreCombo)
        {
            if (clearedLines > 0)
                combo++;
            else
                ResetCombo();
        }

        if (clearedLines <= 4) score += prop.scoreRewards[clearedLines] * (ignoreCombo ? 1 : combo);
        else score += prop.scoreRewards[5] * (int)MathF.Pow(1.5f, clearedLines - 5) * (ignoreCombo ? 1 : combo);

        UpdateScoreText();
        UpdateComboText();
    }

    /// <summary>
    /// Function to reset the combo
    /// </summary>
    public void ResetCombo()
    {
        combo = 0;
    }

    /// <summary>
    /// Function that updates the score text
    /// </summary>
    public void UpdateScoreText()
    {
        string text = MathF.Min(score, 99999) + "";

        for (int i = text.Length; i < 5; i++)
        {
            text = "0" + text;
        }

        scoreText.text = text;
    }

    /// <summary>
    /// Function that updates the combo text
    /// </summary>
    public void UpdateComboText()
    {
        if (combo > 1)
        {
            comboLabel.SetActive(true);
            comboText.gameObject.SetActive(true);

            comboText.text = "X" + combo;
        }
        else
        {
            comboLabel.SetActive(false);
            comboText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>True if player won, false if not</returns>
    public bool CheckWinCondition()
    {
        return score >= scoreGoal;
    }
}
