using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable))]
public class InteractableLogger : MonoBehaviour
{
    UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
    }

    void OnEnable()
    {
        // Hover / unhover
        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);

        // Select (grab / select entered) / deselect
        interactable.selectEntered.AddListener(OnSelectEntered);
        interactable.selectExited.AddListener(OnSelectExited);

        // Activation (press / activate) / deactivate
        interactable.activated.AddListener(OnActivated);
        interactable.deactivated.AddListener(OnDeactivated);
    }

    void OnDisable()
    {
        interactable.hoverEntered.RemoveListener(OnHoverEntered);
        interactable.hoverExited.RemoveListener(OnHoverExited);

        interactable.selectEntered.RemoveListener(OnSelectEntered);
        interactable.selectExited.RemoveListener(OnSelectExited);

        interactable.activated.RemoveListener(OnActivated);
        interactable.deactivated.RemoveListener(OnDeactivated);
    }

    // Event handlers
    void OnHoverEntered(HoverEnterEventArgs args)
    {
        Debug.Log($"[InteractableLogger] HoverEntered on '{gameObject.name}' by {args.interactorObject?.transform?.name}");
    }

    void OnHoverExited(HoverExitEventArgs args)
    {
        Debug.Log($"[InteractableLogger] HoverExited on '{gameObject.name}' by {args.interactorObject?.transform?.name}");
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log($"[InteractableLogger] SelectEntered (grab) on '{gameObject.name}' by {args.interactorObject?.transform?.name}");
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        Debug.Log($"[InteractableLogger] SelectExited on '{gameObject.name}' by {args.interactorObject?.transform?.name}");
    }

    void OnActivated(ActivateEventArgs args)
    {
        Debug.Log($"[InteractableLogger] Activated (press) on '{gameObject.name}' by {args.interactorObject?.transform?.name}");
    }

    void OnDeactivated(DeactivateEventArgs args)
    {
        Debug.Log($"[InteractableLogger] Deactivated on '{gameObject.name}' by {args.interactorObject?.transform?.name}");
    }
}