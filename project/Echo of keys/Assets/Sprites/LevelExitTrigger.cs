using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class LevelExitTrigger : MonoBehaviour
{
    [Header("Player Detection")]
    [Tooltip("Explicit player object. If left empty, the object with the specified tag will be used.")]
    [SerializeField] private GameObject player;
    [Tooltip("Player tag to fall back to when no specific player object is assigned.")]
    [SerializeField] private string playerTag = "Player";

    [Header("Completion UI")]
    [Tooltip("UI GameObject to enable when the player reaches this trigger.")]
    [SerializeField] private GameObject completionUI;
    [Tooltip("Automatically hide the completion UI on Start so designers can preview it in the Scene view.")]
    [SerializeField] private bool hideUIOnStart = true;

    [Header("Scene Transition")]
    [Tooltip("Name of the next scene to load after showing the completion UI.")]
    [SerializeField] private string nextSceneName;
    [Tooltip("Delay (in seconds) before loading the next scene. Useful to let the completion UI play animations.")]
    [SerializeField] private float loadDelay = 1.5f;
    [Tooltip("Use asynchronous loading to avoid frame hitches.")]
    [SerializeField] private bool loadAsync = true;

    [Header("Events")]
    [Tooltip("Optional UnityEvent invoked right before the scene load begins.")]
    [SerializeField] private UnityEvent onLevelCompleted;

    private bool hasTriggered;
    private Collider triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
    }

    private void Start()
    {
        if (hideUIOnStart && completionUI != null)
        {
            completionUI.SetActive(false);
        }

        if (player == null && !string.IsNullOrWhiteSpace(playerTag))
        {
            player = GameObject.FindGameObjectWithTag(playerTag);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !IsPlayer(other.gameObject))
        {
            return;
        }

        hasTriggered = true;

        if (completionUI != null)
        {
            completionUI.SetActive(true);
        }

        onLevelCompleted?.Invoke();

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        if (!string.IsNullOrWhiteSpace(nextSceneName))
        {
            StartCoroutine(LoadNextSceneRoutine());
        }
        else
        {
            Debug.LogWarning($"LevelExitTrigger on {name} was triggered but no next scene name is set.");
        }
    }

    private IEnumerator LoadNextSceneRoutine()
    {
        if (loadDelay > 0f)
        {
            yield return new WaitForSeconds(loadDelay);
        }

        if (loadAsync)
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
            if (loadOperation == null)
            {
                Debug.LogError($"Failed to load scene '{nextSceneName}'. Please verify the scene name is added to Build Settings.");
            }
        }
        else
        {
            try
            {
                SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Exception occurred while loading scene '{nextSceneName}': {ex.Message}");
            }
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

    private void OnValidate()
    {
        if (loadDelay < 0f)
        {
            loadDelay = 0f;
        }
    }
}
