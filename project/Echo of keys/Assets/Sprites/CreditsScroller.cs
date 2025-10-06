using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class CreditsScroller : MonoBehaviour
{
        [Header("Behaviour")]
        [Tooltip("Automatically start scrolling when the GameObject becomes enabled.")]
        [SerializeField] private bool autoStartOnEnable = true;
        [Tooltip("Seconds to wait before the text begins to move.")]
        [SerializeField] private float startDelay = 1f;
        [Tooltip("Vertical speed in UI units (anchored position) per second.")]
        [SerializeField] private float scrollSpeed = 120f;
        [Tooltip("Total vertical distance to travel before stopping (UI units).")]
        [SerializeField] private float travelDistance = 1500f;
        [Tooltip("Seconds to wait after reaching the destination before firing the completion event.")]
        [SerializeField] private float holdDuration = 1f;
        [Tooltip("Return the credits to their initial position each time scrolling starts.")]
        [SerializeField] private bool resetToStartOnReplay = true;

        [Header("Events")]
        [Tooltip("Raised once the credits have finished scrolling and the hold duration has elapsed.")]
        [SerializeField] private UnityEvent onScrollComplete;

        [Header("Scene Transition")]
        [SerializeField] private string titleSceneName = "Title";
        [SerializeField] private float sceneTransitionDelay = 3f;
        [SerializeField] private GameObject[] objectsToActivateAfterTransition;

        private RectTransform content;
        private Vector2 initialAnchoredPosition;
        private Coroutine scrollRoutine;

    private void Awake()
    {
        content = transform as RectTransform;
        if (content != null)
        {
            initialAnchoredPosition = content.anchoredPosition;
        }
    }

    private void OnEnable()
    {
        if (autoStartOnEnable)
        {
            StartScrolling();
        }
    }

    private void OnDisable()
    {
        if (scrollRoutine != null)
        {
            StopCoroutine(scrollRoutine);
            scrollRoutine = null;
        }
    }

    public void StartScrolling()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (resetToStartOnReplay)
        {
            content.anchoredPosition = initialAnchoredPosition;
        }

        if (scrollRoutine != null)
        {
            StopCoroutine(scrollRoutine);
        }
    scrollRoutine = StartCoroutine(ScrollCredits());
    }

    public void StopScrolling()
    {
        if (scrollRoutine != null)
        {
            StopCoroutine(scrollRoutine);
            scrollRoutine = null;
        }
    }

    public void ResetPosition()
    {
        if (content != null)
        {
            content.anchoredPosition = initialAnchoredPosition;
        }
    }

    private IEnumerator ScrollCredits()
    {
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        if (content == null)
        {
            scrollRoutine = null;
            yield break;
        }

        float targetDistance = Mathf.Abs(travelDistance);
        if (targetDistance <= 0f)
        {
            if (holdDuration > 0f)
            {
                yield return new WaitForSeconds(holdDuration);
            }
            onScrollComplete?.Invoke();
            scrollRoutine = null;
            yield break;
        }

        float travelled = 0f;
        float direction = Mathf.Sign(travelDistance);
        Vector2 startPosition = content.anchoredPosition;

        while (travelled < targetDistance)
        {
            float step = scrollSpeed * Time.deltaTime;
            if (step <= 0f)
            {
                yield return null;
                continue;
            }

            float remaining = targetDistance - travelled;
            float move = Mathf.Min(step, remaining);
            travelled += move;

            float newY = startPosition.y + direction * travelled;
            content.anchoredPosition = new Vector2(startPosition.x, newY);

            yield return null;
        }

        float finalY = startPosition.y + direction * targetDistance;
        content.anchoredPosition = new Vector2(startPosition.x, finalY);

        if (holdDuration > 0f)
        {
            yield return new WaitForSeconds(holdDuration);
        }

        onScrollComplete?.Invoke();
        
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.ScheduleTransitionToTitle(
                titleSceneName,
                objectsToActivateAfterTransition,
                sceneTransitionDelay
            );
        }

        scrollRoutine = null;
    }
}
