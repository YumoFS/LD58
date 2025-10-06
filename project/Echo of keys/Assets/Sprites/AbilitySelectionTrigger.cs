using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class AbilitySelectionTrigger : MonoBehaviour
{
    [Header("Player Detection")]
    [Tooltip("Explicit player reference. If left empty the player will be searched by tag.")]
    [SerializeField] private GameObject player;
    [Tooltip("Fallback player tag when no explicit reference is supplied.")]
    [SerializeField] private string playerTag = "Player";

    [Header("UI Handling")]
    [Tooltip("UI controller responsible for presenting ability choices.")]
    [SerializeField] private AbilityChoiceUI abilityChoiceUI;
    [Tooltip("Automatically hide the UI on Start so it only appears when triggered.")]
    [SerializeField] private bool hideUIOnStart = true;
    [Tooltip("Allow players to reuse this trigger after completing a selection.")]
    [SerializeField] private bool allowRepeatSelection = false;

    [Header("Events")]
    [Tooltip("Invoked when the selection UI opens.")]
    [SerializeField] private UnityEvent onSelectionStarted;
    [Tooltip("Invoked after the player finalises both choices.")]
    [SerializeField] private UnityEvent onSelectionCompleted;

    private Collider triggerCollider;
    private bool selectionInProgress;
    private bool hasCompletedOnce;

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

        if (abilityChoiceUI == null)
        {
            Debug.LogError($"AbilitySelectionTrigger on {name} is missing an AbilityChoiceUI reference.");
            enabled = false;
            return;
        }

        if (hideUIOnStart)
        {
            abilityChoiceUI.HideInstantly();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (selectionInProgress)
        {
            return;
        }

        if (!allowRepeatSelection && hasCompletedOnce)
        {
            return;
        }

        if (!IsPlayer(other.gameObject))
        {
            return;
        }

        BeginSelection(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (hideUIOnStart && !selectionInProgress && IsPlayer(other.gameObject))
        {
            abilityChoiceUI.HideInstantly();
        }
    }

    private void BeginSelection(GameObject playerObject)
    {
        Move_Controller controller = playerObject.GetComponent<Move_Controller>();
        if (controller == null)
        {
            controller = player != null ? player.GetComponent<Move_Controller>() : null;
        }

        selectionInProgress = true;
        triggerCollider.enabled = false;
        onSelectionStarted?.Invoke();

        abilityChoiceUI.OpenForSelection(this, controller);
    }

    internal void NotifySelectionFinished()
    {
        if (!selectionInProgress)
        {
            return;
        }

        selectionInProgress = false;
        hasCompletedOnce = true;
        onSelectionCompleted?.Invoke();

        if (allowRepeatSelection && triggerCollider != null)
        {
            triggerCollider.enabled = true;
            hasCompletedOnce = false;
        }

        if (hideUIOnStart)
        {
            abilityChoiceUI.HideInstantly();
        }
    }

    internal Move_Controller GetFallbackController()
    {
        return player != null ? player.GetComponent<Move_Controller>() : null;
    }

    public void ResetTrigger()
    {
        hasCompletedOnce = false;
        selectionInProgress = false;
        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }

        if (hideUIOnStart)
        {
            abilityChoiceUI.HideInstantly();
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
}
