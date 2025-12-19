using UnityEngine;
using TMPro;

public class AngerRoomTrigger : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("The TextMeshPro UI component to display the message.")]
    public TextMeshProUGUI messageText;
    
    [TextArea]
    [Tooltip("The message to display when entering the Anger Room.")]
    public string angerMessage = "RELEASE YOUR ANGER.\nBREAK EVERYTHING.";

    [Header("Door Settings")]
    [Tooltip("The door script (SimpleDoorToggle) to control.")]
    public SimpleDoorToggle entryDoor;
    
    [Tooltip("If true, the door will close automatically when entering.")]
    public bool closeDoorOnEntry = true;

    [Header("Trigger Settings")]
    [Tooltip("Tag of the player object.")]
    public string playerTag = "Player";

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[AngerTrigger] Something entered: {other.name} (Tag: {other.tag})");

        // Check if it's the player
        if (other.CompareTag(playerTag) || other.name.ToLower().Contains("player") || other.name.ToLower().Contains("rig"))
        {
            if (hasTriggered) return; // Optional: Only trigger once? Or every time?
            // Let's trigger every time for now unless specified otherwise.
            
            // 1. Show Message
            if (messageText != null)
            {
                messageText.text = angerMessage;
                messageText.gameObject.SetActive(true);
                // Auto-hide after 7 seconds
                Invoke(nameof(HideText), 5.0f);
            }

            // 2. Close Door
            if (closeDoorOnEntry && entryDoor != null)
            {
                // Force close via the script
                entryDoor.SetState(false); 
            }
            
            hasTriggered = true;
        }
    }

    void HideText()
    {
        if (messageText) messageText.gameObject.SetActive(false);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag) || other.name.ToLower().Contains("player"))
        {
            // Optional: Hide message when leaving?
            // if (messageText != null) messageText.gameObject.SetActive(false);
        }
    }
}
