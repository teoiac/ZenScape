using UnityEngine;
using UnityEngine.InputSystem;

public class XRBoatInput : MonoBehaviour
{
    public BoatController boat;
    [Tooltip("XRI Left Locomotion / Move (Vector2)")]
    public InputActionProperty moveAction;

    void Update()
    {
        if (boat == null || !boat.isControlled) return;

        Vector2 move = moveAction.action.ReadValue<Vector2>();
        boat.SetInput(move);
    }
}