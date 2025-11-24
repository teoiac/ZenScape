using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

[DisallowMultipleComponent]
public class DoorInteractableXR : MonoBehaviour
{
    public enum DoorMode { Rotate, Slide }
    [Header("Interaction")]
    [Tooltip("The XRSimpleInteractable on the clickable object (usually this same GameObject).")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    [Tooltip("If true, a single select toggles open/close. If false, use Activate to open while held.")]
    public bool toggleOnSelect = true;

    [Header("Behavior")]
    public DoorMode mode = DoorMode.Rotate;

    [Tooltip("Transform used as the moving part (the door mesh/panel). If null, uses this transform.")]
    public Transform door;

    [Tooltip("Optional pivot for rotation (e.g., hinge empty). If null, uses 'door'.")]
    public Transform pivot;

    [Tooltip("Seconds to complete open/close.")]
    [Min(0.01f)] public float duration = 0.35f;

    [Tooltip("Easing curve for the motion.")]
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Rotate Settings")]
    [Tooltip("Local axis to rotate around (in pivot local space).")]
    public Vector3 localAxis = Vector3.up;

    [Tooltip("Angle when fully open (degrees). Closed is 0). Positive = right-hand rule around localAxis.")]
    public float openAngle = 90f;

    [Tooltip("Start opened at play?")]
    public bool startOpen = false;

    [Header("Slide Settings")]
    [Tooltip("Local offset from closed to open, applied to 'door' localPosition.")]
    public Vector3 slideOpenOffset = new Vector3(0, 0, 1);

    [Header("Optional SFX")]
    public AudioSource audioSource;
    public AudioClip openClip;
    public AudioClip closeClip;

    bool _isOpen;
    bool _isMoving;
    Coroutine _anim;

    // cached state
    Quaternion _closedRot;
    Quaternion _openRot;
    Vector3 _closedPos;
    Vector3 _openPos;

    void Reset()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        door = transform;
        pivot = door;
    }

    void Awake()
    {
        if (door == null) door = transform;
        if (pivot == null) pivot = door;

        // Cache closed states
        _closedRot = pivot.localRotation;
        _closedPos = door.localPosition;

        // Compute open states based on chosen mode
        RecalculateTargets();

        // Apply initial state
        _isOpen = startOpen;
        if (_isOpen)
            Apply(1f);
        else
            Apply(0f);
    }

    void OnEnable()
    {
        if (!interactable) interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable)
        {
            interactable.selectEntered.AddListener(OnSelectEntered);
            interactable.selectExited.AddListener(OnSelectExited);
            interactable.activated.AddListener(OnActivated);
            interactable.deactivated.AddListener(OnDeactivated);
        }
    }

    void OnDisable()
    {
        if (interactable)
        {
            interactable.selectEntered.RemoveListener(OnSelectEntered);
            interactable.selectExited.RemoveListener(OnSelectExited);
            interactable.activated.RemoveListener(OnActivated);
            interactable.deactivated.RemoveListener(OnDeactivated);
        }
    }

    void OnValidate()
    {
        if (door == null) door = transform;
        if (pivot == null) pivot = door;
        if (duration < 0.01f) duration = 0.01f;
        if (Application.isPlaying) RecalculateTargets();
    }

    void RecalculateTargets()
    {
        // Rebuild open state using current closed (so you can tweak in-Editor)
        _closedRot = pivot.localRotation;
        _closedPos = door.localPosition;

        if (mode == DoorMode.Rotate)
        {
            // Build rotation from axis-angle around localAxis in pivot space
            if (localAxis == Vector3.zero) localAxis = Vector3.up;
            var axis = localAxis.normalized;
            _openRot = _closedRot * Quaternion.AngleAxis(openAngle, axis);
            _openPos = _closedPos; // unchanged for rotate
        }
        else // Slide
        {
            _openPos = _closedPos + slideOpenOffset;
            _openRot = _closedRot; // unchanged for slide
        }
    }

    void OnSelectEntered(SelectEnterEventArgs _)
    {
        if (!toggleOnSelect) return;
        Toggle();
    }

    void OnSelectExited(SelectExitEventArgs _) { /* no-op */ }

    void OnActivated(ActivateEventArgs _)
    {
        if (toggleOnSelect) return; // using select to toggle instead
        Open();
    }

    void OnDeactivated(DeactivateEventArgs _)
    {
        if (toggleOnSelect) return;
        Close();
    }

    public void Toggle()
    {
        if (_isMoving) return;
        if (_isOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (_isOpen || _isMoving) return;
        StartAnim(true);
        PlayClip(openClip);
    }

    public void Close()
    {
        if (!_isOpen || _isMoving) return;
        StartAnim(false);
        PlayClip(closeClip);
    }

    void StartAnim(bool open)
    {
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(Animate(open));
    }

    IEnumerator Animate(bool open)
    {
        _isMoving = true;
        float t = 0f;
        float dir = open ? 1f : -1f;
        float start = open ? 0f : 1f;
        float end = open ? 1f : 0f;

        // capture current state as baseline (supports interruption mid-anim)
        float current = EvaluateCurrentProgress();

        // blend from current to target
        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            float target = Mathf.Lerp(current, end, ease.Evaluate(u));
            Apply(target);
            yield return null;
        }

        Apply(end);
        _isOpen = open;
        _isMoving = false;
        _anim = null;
    }

    float EvaluateCurrentProgress()
    {
        // Returns an approximate 0..1 based on current transform
        if (mode == DoorMode.Rotate)
        {
            // Measure angle between closed and current in pivot space, relative to total
            float total = Mathf.Abs(openAngle);
            if (total < 0.001f) return _isOpen ? 1f : 0f;
            float ang = Quaternion.Angle(_closedRot, pivot.localRotation);
            return Mathf.Clamp01(ang / total) * (openAngle >= 0f ? 1f : 1f);
        }
        else
        {
            float total = slideOpenOffset.magnitude;
            if (total < 0.0001f) return _isOpen ? 1f : 0f;
            float dist = Vector3.Distance(_closedPos, door.localPosition);
            return Mathf.Clamp01(dist / total);
        }
    }

    void Apply(float progress01)
    {
        progress01 = Mathf.Clamp01(progress01);
        if (mode == DoorMode.Rotate)
        {
            pivot.localRotation = Quaternion.Slerp(_closedRot, _openRot, progress01);
            // position unchanged
            door.localPosition = _closedPos;
        }
        else
        {
            door.localPosition = Vector3.Lerp(_closedPos, _openPos, progress01);
            // rotation unchanged
            pivot.localRotation = _closedRot;
        }
    }

    void PlayClip(AudioClip clip)
    {
        if (audioSource && clip) audioSource.PlayOneShot(clip);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!door) door = transform;
        if (!pivot) pivot = door;

        Gizmos.color = new Color(0f, 0.7f, 1f, 0.5f);
        Gizmos.DrawWireSphere(pivot.position, 0.05f);

        if (mode == DoorMode.Rotate)
        {
            // draw axis
            var axisWorld = pivot.TransformDirection(localAxis.normalized);
            Gizmos.DrawLine(pivot.position, pivot.position + axisWorld * 0.3f);
            // draw open arc hint (editor-only approximation)
            UnityEditor.Handles.color = new Color(0f, 0.7f, 1f, 0.5f);
            UnityEditor.Handles.DrawWireArc(pivot.position, axisWorld, pivot.right, openAngle, 0.3f);
        }
        else
        {
            Gizmos.DrawLine(door.position, door.TransformPoint(slideOpenOffset));
        }
    }
#endif
}
