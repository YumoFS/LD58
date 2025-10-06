using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

[RequireComponent(typeof(Collider))]
public class SandStormAmbience : MonoBehaviour
{
    [Header("Player Detection")]
    [Tooltip("Explicit player reference. If left empty the player will be located using the Player Tag.")]
    [SerializeField] private GameObject player;
    [Tooltip("Player tag used when no explicit reference is set.")]
    [SerializeField] private string playerTag = "Player";

    [Header("Visual FX")]
    [Tooltip("Particle systems to enable when the player is inside the sandstorm volume.")]
    [SerializeField] private ParticleSystem[] sandParticleSystems;
    [Tooltip("Should the particle systems be stopped and cleared when the scene starts?")]
    [SerializeField] private bool resetParticlesOnStart = true;

    [Header("Audio FX")]
    [Tooltip("Ambient wind audio source that fades in when the storm is active.")]
    [SerializeField] private AudioSource sandstormAudio;
    [Range(0f, 1f)]
    [Tooltip("Target volume reached while the player remains inside the storm.")]
    [SerializeField] private float audioTargetVolume = 0.7f;
    [Tooltip("Seconds to fade the audio in when the player enters.")]
    [SerializeField] private float audioFadeInDuration = 1.5f;
    [Tooltip("Seconds to fade the audio out when the player leaves.")]
    [SerializeField] private float audioFadeOutDuration = 1.5f;

    [Header("Post Processing (Optional)")]
    [Tooltip("Optional Volume (fog, color grading, etc.) blended while the storm is active.")]
    [SerializeField] private Volume sandstormVolume;
    [Range(0f, 1f)]
    [Tooltip("Blend weight applied to the Volume while the storm is active.")]
    [SerializeField] private float volumeTargetWeight = 0.8f;
    [Tooltip("Seconds to fade the Volume weight when the player enters.")]
    [SerializeField] private float volumeFadeInDuration = 1.5f;
    [Tooltip("Seconds to fade the Volume weight when the player exits.")]
    [SerializeField] private float volumeFadeOutDuration = 1.5f;

    [Header("Events")]
    [Tooltip("Raised when the player first enters the sandstorm volume.")]
    [SerializeField] private UnityEvent onSandstormEnter;
    [Tooltip("Raised when the player exits the sandstorm volume.")]
    [SerializeField] private UnityEvent onSandstormExit;

    private Collider triggerCollider;
    private bool isPlayerInside;
    private Coroutine audioFadeCoroutine;
    private Coroutine volumeFadeCoroutine;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
    }

    private void Start()
    {
        if (player == null && !string.IsNullOrWhiteSpace(playerTag))
        {
            player = GameObject.FindGameObjectWithTag(playerTag);
        }

        if (sandParticleSystems != null && resetParticlesOnStart)
        {
            foreach (ParticleSystem ps in sandParticleSystems)
            {
                if (ps == null) continue;
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        if (sandstormAudio != null)
        {
            sandstormAudio.loop = true;
            sandstormAudio.volume = 0f;
        }

        if (sandstormVolume != null)
        {
            sandstormVolume.weight = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPlayerInside)
        {
            return;
        }

        if (!IsPlayer(other.gameObject))
        {
            return;
        }

        isPlayerInside = true;
        ActivateStorm();
        onSandstormEnter?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isPlayerInside)
        {
            return;
        }

        if (!IsPlayer(other.gameObject))
        {
            return;
        }

        isPlayerInside = false;
        DeactivateStorm();
        onSandstormExit?.Invoke();
    }

    private void ActivateStorm()
    {
        if (sandParticleSystems != null)
        {
            foreach (ParticleSystem ps in sandParticleSystems)
            {
                if (ps == null) continue;
                if (!ps.isPlaying)
                {
                    ps.Play();
                }
            }
        }

        StartAudioFade(audioTargetVolume, audioFadeInDuration);
        StartVolumeFade(volumeTargetWeight, volumeFadeInDuration);
    }

    private void DeactivateStorm()
    {
        if (sandParticleSystems != null)
        {
            foreach (ParticleSystem ps in sandParticleSystems)
            {
                if (ps == null) continue;
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        StartAudioFade(0f, audioFadeOutDuration);
        StartVolumeFade(0f, volumeFadeOutDuration);
    }

    private void StartAudioFade(float targetVolume, float duration)
    {
        if (sandstormAudio == null)
        {
            return;
        }

        if (audioFadeCoroutine != null)
        {
            StopCoroutine(audioFadeCoroutine);
        }

        audioFadeCoroutine = StartCoroutine(FadeAudio(targetVolume, duration));
    }

    private IEnumerator FadeAudio(float targetVolume, float duration)
    {
        targetVolume = Mathf.Clamp01(targetVolume);

        if (duration <= 0f)
        {
            ApplyAudioVolume(targetVolume);
            yield break;
        }

        float startVolume = sandstormAudio.volume;
        float elapsed = 0f;

        if (targetVolume > 0f && !sandstormAudio.isPlaying)
        {
            sandstormAudio.Play();
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float volume = Mathf.Lerp(startVolume, targetVolume, t);
            sandstormAudio.volume = volume;
            yield return null;
        }

        ApplyAudioVolume(targetVolume);
    }

    private void ApplyAudioVolume(float volume)
    {
        sandstormAudio.volume = volume;

        if (Mathf.Approximately(volume, 0f))
        {
            sandstormAudio.Stop();
            sandstormAudio.volume = 0f;
        }
    }

    private void StartVolumeFade(float targetWeight, float duration)
    {
        if (sandstormVolume == null)
        {
            return;
        }

        if (volumeFadeCoroutine != null)
        {
            StopCoroutine(volumeFadeCoroutine);
        }

        volumeFadeCoroutine = StartCoroutine(FadeVolume(targetWeight, duration));
    }

    private IEnumerator FadeVolume(float targetWeight, float duration)
    {
        targetWeight = Mathf.Clamp01(targetWeight);

        if (duration <= 0f)
        {
            ApplyVolumeWeight(targetWeight);
            yield break;
        }

        float startWeight = sandstormVolume.weight;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float weight = Mathf.Lerp(startWeight, targetWeight, t);
            sandstormVolume.weight = weight;
            yield return null;
        }

        ApplyVolumeWeight(targetWeight);
    }

    private void ApplyVolumeWeight(float weight)
    {
        sandstormVolume.weight = weight;

        if (Mathf.Approximately(weight, 0f))
        {
            sandstormVolume.weight = 0f;
        }
    }

    private bool IsPlayer(GameObject candidate)
    {
        if (player != null)
        {
            if (candidate == player || candidate.transform.IsChildOf(player.transform))
            {
                return true;
            }
        }

        if (!string.IsNullOrWhiteSpace(playerTag) && candidate.CompareTag(playerTag))
        {
            return true;
        }

        return false;
    }

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnDisable()
    {
        if (sandstormAudio != null)
        {
            sandstormAudio.volume = 0f;
            sandstormAudio.Stop();
        }

        if (sandstormVolume != null)
        {
            sandstormVolume.weight = 0f;
        }

        if (sandParticleSystems != null)
        {
            foreach (ParticleSystem ps in sandParticleSystems)
            {
                if (ps == null) continue;
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}
