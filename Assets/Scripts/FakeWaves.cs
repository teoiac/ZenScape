using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TinyBob : MonoBehaviour
{
    public float amplitude = 0.03f;   // ~3 cm
    public float frequency = 0.2f;    // cycles per second (0.2 = every 5s)
    public float follow = 5f;         // smoothing so it’s subtle

    Rigidbody rb;
    float baseY;
    float phase;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        phase = Random.value * 10f;   // avoid perfect sync if multiple boats
    }

    void Start()
    {
        // Use the current settled height as the base
        baseY = transform.position.y;
    }

    void FixedUpdate()
    {
        float t = Time.time + phase;
        float targetY = baseY + Mathf.Sin(t * Mathf.PI * 2f * frequency) * amplitude;

        // Smoothly move only on Y, no rotation changes
        Vector3 pos = rb.position;
        pos.y = Mathf.Lerp(pos.y, targetY, follow * Time.fixedDeltaTime);
        rb.MovePosition(pos);
    }
}