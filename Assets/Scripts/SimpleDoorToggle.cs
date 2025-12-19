using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Collider))]
public class SimpleDoorToggle : MonoBehaviour
{
    public XRSimpleInteractable interactable;
    public Transform pivot;                 // Hinge transform (required)
    public float openAngle = 90f;           // Degrees
    public float duration = 0.35f;          // Seconds
    public bool startOpen = false;

    Quaternion _closed, _open;
    bool _openState;
    Coroutine _anim;

    void Reset()
    {
        interactable = GetComponent<XRSimpleInteractable>();
    }

    void Awake()
    {
        if (!interactable) interactable = GetComponent<XRSimpleInteractable>();
        if (!pivot) pivot = transform;
        _closed = pivot.localRotation;
        _open   = _closed * Quaternion.AngleAxis(openAngle, Vector3.up);
        pivot.localRotation = startOpen ? _open : _closed;
        _openState = startOpen;
    }

    void OnEnable()  { interactable.selectEntered.AddListener(OnSelect); }
    void OnDisable() { interactable.selectEntered.RemoveListener(OnSelect); }

    void OnSelect(SelectEnterEventArgs _)
    {
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(Animate(!_openState));
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
                if (_anim != null) StopCoroutine(_anim);
                _anim = StartCoroutine(Animate(!_openState));
            }
            else
            {
                Debug.Log($"[DoorDebug] {gameObject.name}: Pressed 'K'. Missed. (XR Hover: {interactable?.isHovered}, Camera Hit: {isLookedAt})");
            }
        }
    }

    // Public method to force Open/Close state from other scripts (like AngerRoomTrigger)
    public void SetState(bool isOpen)
    {
        if (_openState == isOpen) return; // Already in desired state
        
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(Animate(isOpen));
    }

    IEnumerator Animate(bool toOpen)
    {
        _openState = toOpen;
        Quaternion a = pivot.localRotation;
        Quaternion b = toOpen ? _open : _closed;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            pivot.localRotation = Quaternion.Slerp(a, b, Mathf.Clamp01(t / duration));
            yield return null;
        }
        pivot.localRotation = b;
        _anim = null;
    }
}