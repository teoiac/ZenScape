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

    bool inZone;

    void Awake()
    {
        ApplyText(generalInstructions);
        SetVisible(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(breathingZoneTag)) return;
        inZone = true;
        ApplyText(generalInstructions);
        SetVisible(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(breathingZoneTag)) return;
        inZone = false;
        if (!keepVisibleAfterExit) SetVisible(false);
    }

    void ApplyText(string msg)
    {
        if (instructionText) instructionText.text = msg;
    }

    void SetVisible(bool visible)
    {
        if (instructionText) instructionText.gameObject.SetActive(visible);
    }
}