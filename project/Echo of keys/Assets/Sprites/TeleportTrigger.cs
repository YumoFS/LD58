using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TeleportTrigger : MonoBehaviour
{
    [Header("Player Detection")]
    [Tooltip("Explicit player object. If left empty, the object with the specified tag will be used.")]
    [SerializeField] private GameObject player;
    [Tooltip("Player tag to fall back to when no specific player object is assigned.")]
    [SerializeField] private string playerTag = "Player";

    [Header("Destination")]
    [Tooltip("Target transform to move the player to. If left empty, Manual Target Position will be used when enabled.")]
    [SerializeField] private Transform targetTransform;
    [Tooltip("Use the Manual Target Position instead of a transform reference.")]
    [SerializeField] private bool useManualTargetPosition = false;
    [Tooltip("Manual world-space position to teleport the player to when no target transform is assigned.")]
    [SerializeField] private Vector3 manualTargetPosition;
    [Tooltip("Copy the rotation of the target transform when teleporting (ignored when using manual position).")]
    [SerializeField] private bool matchTargetRotation = false;

    [Header("Trigger Behaviour")]
    [Tooltip("Allow the player to trigger teleport multiple times.")]
    [SerializeField] private bool allowMultipleTriggers = false;
    [Tooltip("Optional cool down in seconds before the trigger can activate again (only when multiple triggers are allowed).")]
    [SerializeField] private float reactivationDelay = 0f;

    [Header("Effects & Events")]
    [Tooltip("Effect to spawn at the player's original position when teleporting.")]
    [SerializeField] private GameObject leaveEffect;
    [Tooltip("Effect to spawn at the player's destination after teleporting.")]
    [SerializeField] private GameObject arriveEffect;
    [Tooltip("Optional UnityEvent invoked after the teleport is completed.")]
    [SerializeField] private UnityEvent onTeleported;

    private Collider triggerCollider;
    private bool isTeleporting;
    private bool hasTriggered;
    private float lastTriggerTime;

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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other.gameObject))
        {
            return;
        }

        if (!allowMultipleTriggers && hasTriggered)
        {
            return;
        }

        if (isTeleporting)
        {
            return;
        }

        if (allowMultipleTriggers && reactivationDelay > 0f)
        {
            if (Time.time - lastTriggerTime < reactivationDelay)
            {
                return;
            }
        }

        if (!IsDestinationConfigured(out Vector3 destination, out Quaternion destinationRotation))
        {
            Debug.LogWarning($"TeleportTrigger on {name} was activated but no valid destination is configured.");
            return;
        }

        StartCoroutine(TeleportPlayerRoutine(other.gameObject, destination, destinationRotation));
    }

    private System.Collections.IEnumerator TeleportPlayerRoutine(GameObject playerObject, Vector3 destination, Quaternion rotation)
    {
        isTeleporting = true;
        hasTriggered = true;
        lastTriggerTime = Time.time;

        if (!allowMultipleTriggers && triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        SpawnEffect(leaveEffect, playerObject.transform.position);

        CharacterController controller = playerObject.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        playerObject.transform.position = destination;
        if (matchTargetRotation && targetTransform != null)
        {
            playerObject.transform.rotation = rotation;
        }

        if (controller != null)
        {
            controller.enabled = true;
        }

        SpawnEffect(arriveEffect, destination);

        onTeleported?.Invoke();

        if (allowMultipleTriggers)
        {
            isTeleporting = false;
        }

        yield return null;

        if (!allowMultipleTriggers)
        {
            isTeleporting = false;
        }
    }

    private bool IsDestinationConfigured(out Vector3 position, out Quaternion rotation)
    {
        if (targetTransform != null)
        {
            position = targetTransform.position;
            rotation = targetTransform.rotation;
            return true;
        }

        if (useManualTargetPosition)
        {
            position = manualTargetPosition;
            rotation = Quaternion.identity;
            return true;
        }

        position = Vector3.zero;
        rotation = Quaternion.identity;
        return false;
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

    private void SpawnEffect(GameObject effectPrefab, Vector3 position)
    {
        if (effectPrefab == null)
        {
            return;
        }

        Instantiate(effectPrefab, position, Quaternion.identity);
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
        if (reactivationDelay < 0f)
        {
            reactivationDelay = 0f;
        }
    }
}
