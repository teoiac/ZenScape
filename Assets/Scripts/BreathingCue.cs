using UnityEngine;
using TMPro;

public class BreathingCueSimple : MonoBehaviour
{
    [Header("Zone detection")]
    [Tooltip("Tag on the trigger collider (the floor/room).")]
    public string breathingZoneTag = "BreathingZone";

    [Header("UI")]
    [Tooltip("Text object that will display the general instructions.")]
    public TextMeshProUGUI instructionText;

    [Header("Message")]
    [TextArea(3, 6)]
    public string generalInstructions =
        "Follow the bulb:\n" +
        "• Inhale as it expands (~3s)\n" +
        "• Exhale as it contracts (~3s)\n" +
        "Breathe naturally and comfortably.";

    [Header("Visibility")]
    [Tooltip("If true, instructions stay visible even after leaving the zone (until hidden by other logic).")]
    public bool keepVisibleAfterExit = false;

    [Header("Door")]
    [Tooltip("The door GameObject that should rotate open/closed.")]
    public Transform doorTransform;

    [Tooltip("Y rotation when the door is CLOSED.")]
    public float doorClosedY = -90f;

    [Tooltip("Y rotation when the door is OPEN.")]
    public float doorOpenY = -8f;

    [Tooltip("How fast the door rotates toward its target (degrees per second).")]
    public float doorRotateSpeed = 180f;

    [Header("Breathing Session")]
    [Tooltip("Inhale duration (seconds).")]
    public float inhaleDuration = 3f;

    [Tooltip("Exhale duration (seconds).")]
    public float exhaleDuration = 3f;

    [Tooltip("How many full cycles (inhale + exhale) must be completed to open the door.")]
    public int cyclesToComplete = 4;

    bool inZone;
    bool breathingInProgress;
    Quaternion targetDoorRotation;
    Coroutine breathingRoutine;

    void Awake()
    {
        ApplyText(generalInstructions);
        SetVisible(false);

        // Ensure the door starts open (optional)
        if (doorTransform != null)
        {
            Vector3 e = doorTransform.localEulerAngles;
            doorTransform.localRotation = Quaternion.Euler(e.x, doorOpenY, e.z);
            targetDoorRotation = doorTransform.localRotation;
        }
    }

    void Update()
    {
        // Debug Input: Shift + G to toggle (New Input System support)
        if (UnityEngine.InputSystem.Keyboard.current != null && 
            UnityEngine.InputSystem.Keyboard.current.leftShiftKey.isPressed && 
            UnityEngine.InputSystem.Keyboard.current.gKey.wasPressedThisFrame)
        {
             // Toggle between closed and open
             bool isClosed = Mathf.Abs(targetDoorRotation.eulerAngles.y - doorClosedY) < 1.0f; 
             // Note: eulerAngles 0-360 vs +/- values can be tricky, but for a toggle we can just flip state
             // Simplified toggle: if not explicitly 'open', set to open.
             
             // Let's just swap target based on current target
             float currentTargetY = targetDoorRotation.eulerAngles.y;
             // Unity handles euler conversion, but safest is to just set to Open if we aren't sure, or toggle.
             // Let's assume if it looks closed, open it.
             
             // Better logic: use a specific flag or just toggle against doorOpenY
             SetDoorTargetAngle(doorOpenY); // For now, let's at least ensure Shift+G OPENS it. 
             // If user wants toggle, we need state. But BreathingCue controls door via game logic.
             // Forcing it OPEN is usually what debuggers want.
        }

        // Smoothly move the door toward its target rotation
        if (doorTransform != null)
        {
            doorTransform.localRotation = Quaternion.RotateTowards(
                doorTransform.localRotation,
                targetDoorRotation,
                doorRotateSpeed * Time.deltaTime
            );
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(breathingZoneTag)) return;

        inZone = true;
        ApplyText(generalInstructions);
        SetVisible(true);

        // Start breathing session and close door
        StartBreathingSession();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(breathingZoneTag)) return;

        inZone = false;
        if (!keepVisibleAfterExit)
            SetVisible(false);

        // Stop breathing session if player leaves
        StopBreathingSession();
        // Optional: open door again when leaving early
        SetDoorTargetAngle(doorOpenY);
    }

    void ApplyText(string msg)
    {
        if (instructionText) instructionText.text = msg;
    }

    void SetVisible(bool visible)
    {
        if (instructionText) instructionText.gameObject.SetActive(visible);
    }

    // ----- Door helpers -----

    void SetDoorTargetAngle(float yAngle)
    {
        if (doorTransform == null) return;

        Vector3 e = doorTransform.localEulerAngles;
        targetDoorRotation = Quaternion.Euler(e.x, yAngle, e.z);
    }

    // ----- Breathing logic -----

    void StartBreathingSession()
    {
        if (breathingInProgress) return;
        breathingInProgress = true;

        // Close the door immediately when session starts
        SetDoorTargetAngle(doorClosedY);

        breathingRoutine = StartCoroutine(BreathingRoutine());
    }

    void StopBreathingSession()
    {
        if (!breathingInProgress) return;

        breathingInProgress = false;
        if (breathingRoutine != null)
        {
            StopCoroutine(breathingRoutine);
            breathingRoutine = null;
        }
    }

    System.Collections.IEnumerator BreathingRoutine()
    {
        int currentCycle = 0;

        while (currentCycle < cyclesToComplete)
        {
            // Inhale phase
            yield return new WaitForSeconds(inhaleDuration);

            // Exhale phase
            yield return new WaitForSeconds(exhaleDuration);

            currentCycle++;

            // (Optional) You could update the UI here:
            // ApplyText($"Breathing... ({currentCycle}/{cyclesToComplete})");
        }

        // Finished all cycles: open the door
        SetDoorTargetAngle(doorOpenY);

        breathingInProgress = false;
        breathingRoutine = null;
    }
}
