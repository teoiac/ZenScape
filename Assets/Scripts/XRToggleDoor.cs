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
        // Debug Input: Press 'K' to toggle (New Input System support)
        // Changed to 'K' because 'T' and 'Shift' trigger Simulator modes (Hand Lock/Grip).
        if (UnityEngine.InputSystem.Keyboard.current != null && 
            UnityEngine.InputSystem.Keyboard.current.kKey.wasPressedThisFrame)
        {
            bool isLookedAt = false;
            // Failsafe: Raycast from Camera Center
            Camera cam = Camera.main;
            if (cam == null) cam = UnityEngine.Object.FindFirstObjectByType<Camera>();
            
            if (cam != null)
            {
                Ray ray = new Ray(cam.transform.position, cam.transform.forward);
                RaycastHit hit;
                // Cast 5 meters (Increased from 3m per user request)
                if (Physics.Raycast(ray, out hit, 5.0f))
                {
                   // Check if we hit THIS door, or a parent/child of it
                   if (hit.collider.gameObject == gameObject || 
                       hit.collider.transform.IsChildOf(transform) || 
                       transform.IsChildOf(hit.collider.transform))
                   {
                       isLookedAt = true;
                   }
                }
            }

            // Check EITHER XR Hover OR Raycast Failsafe
            if ((interactable != null && interactable.isHovered) || isLookedAt)
            {
                Debug.Log($"[DoorDebug] {gameObject.name}: 'K' pressed. Triggered by {(isLookedAt ? "CAMERA RAY" : "XR HOVER")}. Opening!");
                ToggleDoor();
            }
            else
            {
                Debug.Log($"[DoorDebug] {gameObject.name}: Pressed 'K'. Missed. (XR Hover: {interactable?.isHovered}, Camera Hit: {isLookedAt})");
            }
        }

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
