using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AbilityChoiceUI : MonoBehaviour
{
    public enum DeleteChoice
    {
        None = 0,
        DeleteBlocks = 1,
        CreateBlocks = 2
    }

    public enum ShiftChoice
    {
        None = 0,
        Teleport = 1,
        Recall = 2
    }

    [Header("UI Root")]
    [Tooltip("Root object that should be toggled when the UI opens or closes.")]
    [SerializeField] private GameObject root;

    [Header("Button References")]
    [Tooltip("Button representing the first Delete option.")]
    [SerializeField] private Button deleteOptionAButton;
    [Tooltip("Button representing the second Delete option.")]
    [SerializeField] private Button deleteOptionBButton;
    [Tooltip("Button representing the first Shift option.")]
    [SerializeField] private Button shiftOptionAButton;
    [Tooltip("Button representing the second Shift option.")]
    [SerializeField] private Button shiftOptionBButton;

    [Header("Feedback")]
    [Tooltip("Optional event raised whenever this UI is shown.")]
    [SerializeField] private UnityEvent onOpened;
    [Tooltip("Optional event raised once both choices are confirmed.")]
    [SerializeField] private UnityEvent onCompleted;

    private AbilitySelectionTrigger activeTrigger;
    private Move_Controller playerController;

    private DeleteChoice deleteChoice = DeleteChoice.None;
    private ShiftChoice shiftChoice = ShiftChoice.None;

    private void Awake()
    {
        if (root == null)
        {
            root = gameObject;
        }

        RegisterButtonCallbacks();
    }

    private void RegisterButtonCallbacks()
    {
        if (deleteOptionAButton != null)
        {
            deleteOptionAButton.onClick.AddListener(() => ChooseDeleteOption(DeleteChoice.DeleteBlocks));
        }

        if (deleteOptionBButton != null)
        {
            deleteOptionBButton.onClick.AddListener(() => ChooseDeleteOption(DeleteChoice.CreateBlocks));
        }

        if (shiftOptionAButton != null)
        {
            shiftOptionAButton.onClick.AddListener(() => ChooseShiftOption(ShiftChoice.Teleport));
        }

        if (shiftOptionBButton != null)
        {
            shiftOptionBButton.onClick.AddListener(() => ChooseShiftOption(ShiftChoice.Recall));
        }
    }

    public void HideInstantly()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
    }

    public void OpenForSelection(AbilitySelectionTrigger trigger, Move_Controller controller)
    {
        activeTrigger = trigger;
        playerController = controller != null ? controller : trigger != null ? trigger.GetFallbackController() : playerController;

        ResetState();

        if (root != null)
        {
            root.SetActive(true);
        }

        onOpened?.Invoke();
    }

    private void ResetState()
    {
        deleteChoice = DeleteChoice.None;
        shiftChoice = ShiftChoice.None;

        UpdateButtonInteractivity(deleteOptionAButton, true);
        UpdateButtonInteractivity(deleteOptionBButton, true);
        UpdateButtonInteractivity(shiftOptionAButton, true);
        UpdateButtonInteractivity(shiftOptionBButton, true);
    }

    private void ChooseDeleteOption(DeleteChoice choice)
    {
        if (choice == DeleteChoice.None)
        {
            return;
        }

        deleteChoice = choice;
        ApplyDeleteChoice(choice);

        UpdateButtonInteractivity(deleteOptionAButton, deleteOptionAButton != null && choice != DeleteChoice.DeleteBlocks);
        UpdateButtonInteractivity(deleteOptionBButton, deleteOptionBButton != null && choice != DeleteChoice.CreateBlocks);

        TryComplete();
    }

    private void ChooseShiftOption(ShiftChoice choice)
    {
        if (choice == ShiftChoice.None)
        {
            return;
        }

        shiftChoice = choice;
        ApplyShiftChoice(choice);

        UpdateButtonInteractivity(shiftOptionAButton, shiftOptionAButton != null && choice != ShiftChoice.Teleport);
        UpdateButtonInteractivity(shiftOptionBButton, shiftOptionBButton != null && choice != ShiftChoice.Recall);

        TryComplete();
    }

    private void ApplyDeleteChoice(DeleteChoice choice)
    {
        if (playerController == null)
        {
            Debug.LogWarning("AbilityChoiceUI could not apply Delete choice because Move_Controller reference is missing.");
            return;
        }

        switch (choice)
        {
            case DeleteChoice.DeleteBlocks:
                playerController.canDelete = true;
                playerController.canAdd = false;
                break;
            case DeleteChoice.CreateBlocks:
                playerController.canDelete = false;
                playerController.canAdd = true;
                break;
        }
    }

    private void ApplyShiftChoice(ShiftChoice choice)
    {
        if (playerController == null)
        {
            Debug.LogWarning("AbilityChoiceUI could not apply Shift choice because Move_Controller reference is missing.");
            return;
        }

        switch (choice)
        {
            case ShiftChoice.Teleport:
                playerController.canTeleport = true;
                playerController.canRecall = false;
                break;
            case ShiftChoice.Recall:
                playerController.canTeleport = false;
                playerController.canRecall = true;
                break;
        }
    }

    private void TryComplete()
    {
        if (deleteChoice != DeleteChoice.None && shiftChoice != ShiftChoice.None)
        {
            CompleteSelection();
        }
    }

    private void CompleteSelection()
    {
        onCompleted?.Invoke();
        HideInstantly();

        if (activeTrigger != null)
        {
            activeTrigger.NotifySelectionFinished();
            activeTrigger = null;
        }
    }

    private void UpdateButtonInteractivity(Button button, bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    public void UnlockAllButtons()
    {
        UpdateButtonInteractivity(deleteOptionAButton, true);
        UpdateButtonInteractivity(deleteOptionBButton, true);
        UpdateButtonInteractivity(shiftOptionAButton, true);
        UpdateButtonInteractivity(shiftOptionBButton, true);
    }
}
