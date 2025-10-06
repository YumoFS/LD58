using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioMng : MonoBehaviour
{
    public static AudioMng Instance { get; private set; }
    public AudioClip bgm1;
    public AudioClip bgm2;
    public AudioSource audioSource;
    public AudioSource sfxSource;
    void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        footstep = Resources.Load<AudioClip>("footstep");
        footstep.LoadAudioData();
    }
    public void PlayMusic(string name, float volume = 1)
    {
        audioSource.volume = volume;
        if (name == "1")
        {
            audioSource.clip = bgm1;
            audioSource.Play();
        }
        else
        {
            audioSource.clip = bgm2;
            audioSource.Play();
        }
    }
    public void PlaySound(string name, float volume = 1)
    {
        AudioClip audioClip = Resources.Load<AudioClip>(name);
        audioClip.LoadAudioData();
        sfxSource.PlayOneShot(audioClip);
    }
    private AudioClip footstep;
    private static float[] pitchOffsets = new float[] {
        1.2f, 1.8f, 1.5f, 0.8f, 1.1f, 0.9f, 1.1f, 1f, 1.2f, 0.9f
    };
    int idx = 0;
    public void PlayFootStep()
    {
        sfxSource.pitch = pitchOffsets[idx++];
        if (idx == pitchOffsets.Length)
        {
            idx = 0;
        }
        sfxSource.PlayOneShot(footstep);
    }
}