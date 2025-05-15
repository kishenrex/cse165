using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource sfxSource;
    public AudioClip checkpointBeep, crashSound, checkpointReachedSound;
    public parse parse;
    public void PlayCrashSound()
    {
        sfxSource.PlayOneShot(crashSound);
    }
    public void PlayCheckPointReached()
    {
        sfxSource.PlayOneShot(checkpointReachedSound);
    }

    public void PlayCheckPointBeep()
    {
        sfxSource.PlayOneShot(checkpointBeep);
    }

    void Update()
    {
        if (gameObject.transform.position == parse.currentCheckpoint[0])
        {
            PlayCheckPointBeep();
        }
    }
}
