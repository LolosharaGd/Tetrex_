using System.Collections;
using System.Collections.Generic;
using Tetrex.DataStructures;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    [SerializeField] PropertiesScriptableObject prop;

    public AudioSource blockMoveSource;
    public AudioSource blockRotateSource;
    public AudioSource blockDashSource;
    public AudioSource blockTrashSource;

    public AudioSource rowClearSource;

    public AudioSource musicSource;

    [SerializeField] List<AudioLowPassFilter> pauseMuffled = new();
    public float muffledFrequency;

    public void PlayRandomMoveSound()
    {
        blockMoveSource.clip = prop.blockMoveSound.RandomClip;
        blockMoveSource.Play();
    }

    public void PlayRandomRotateSound()
    {
        blockRotateSource.clip = prop.blockRotateSound.RandomClip;
        blockRotateSource.Play();
    }

    public void PlayRandomDashSound()
    {
        blockDashSource.clip = prop.blockDashSound.RandomClip;
        blockDashSource.Play();
    }

    public void PlayRandomTrashSound()
    {
        blockTrashSource.clip = prop.blockTrashSound.RandomClip;
        blockTrashSource.Play();
    }

    public void PlayRowClearSound(int rowsCleared)
    {
        if (rowsCleared == 0) return;
        rowClearSource.clip = prop.rowClearSounds[rowsCleared].RandomClip;
        rowClearSource.Play();
    }

    public void PlayStageMusic(int stage)
    {
        musicSource.clip = prop.stageMusic[stage].RandomClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayStageBossMusic(int stage)
    {
        musicSource.clip = prop.stageBossMusic[stage].RandomClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayShopMusic()
    {
        musicSource.clip = prop.shopMusic.RandomClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void SetPauseMuffling(float percent)
    {
        float scaledPercent = 1f - Mathf.Pow(percent - 1f, 6f);
        foreach (var lp in pauseMuffled)
        {
            lp.cutoffFrequency = Mathf.Lerp(22000f, muffledFrequency, scaledPercent);
        }
    }
}
