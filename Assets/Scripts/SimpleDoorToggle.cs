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