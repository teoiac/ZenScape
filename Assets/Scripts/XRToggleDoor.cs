using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class XRToggleDoor : MonoBehaviour
{
    [Header("Door Setup")]
    [Tooltip("Transform that rotates (the actual door mesh/pivot). If left empty, uses this GameObject.")]
    public Transform doorTransform;

    [Tooltip("Y rotation when the door is CLOSED.")]
    public float closedY = 0f;

    [Tooltip("Y rotation when the door is OPEN.")]
    public float openY = 90f;

    [Tooltip("How fast the door rotates (degrees per second).")]
    public float rotateSpeed = 180f;

    bool isOpen = false;
    Quaternion targetRotation;
    XRSimpleInteractable interactable;

    void Awake()
    {
        if (doorTransform == null)
            doorTransform = transform;

        // Start in closed position
        Vector3 e = doorTransform.localEulerAngles;
        e.y = closedY;
        doorTransform.localEulerAngles = e;
        targetRotation = doorTransform.localRotation;

        interactable = GetComponent<XRSimpleInteractable>();
    }

    void OnEnable()
    {
        if (interactable != null)
            interactable.selectEntered.AddListener(OnSelectEntered);
    }

    void OnDisable()
    {
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnSelectEntered);
    }

    void Update()
    {
        if (doorTransform != null)
        {
            doorTransform.localRotation = Quaternion.RotateTowards(
                doorTransform.localRotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );
        }
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        ToggleDoor();
    }

    void ToggleDoor()
    {
        isOpen = !isOpen;
        float targetY = isOpen ? openY : closedY;
        SetTargetAngle(targetY);
    }

    void SetTargetAngle(float yAngle)
    {
        if (doorTransform == null) return;

        Vector3 e = doorTransform.localEulerAngles;
        targetRotation = Quaternion.Euler(e.x, yAngle, e.z);
    }
}
