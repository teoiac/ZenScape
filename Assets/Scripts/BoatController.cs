using UnityEngine;

public class BoatController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float turnSpeed = 50f;

    [HideInInspector] public bool isControlled = false;

    Vector2 currentInput;

    // Called from XRBoatInput
    public void SetInput(Vector2 input)
    {
        currentInput = input;
    }

    void Update()
    {
        if (!isControlled) return;

        float move = currentInput.y;  // forward/back
        float turn = currentInput.x;  // left/right

        // Move forward/backward
        transform.position += transform.forward * (move * moveSpeed * Time.deltaTime);

        // Rotate around Y
        transform.Rotate(0f, turn * turnSpeed * Time.deltaTime, 0f);
    }
}
