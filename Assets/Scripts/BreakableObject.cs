using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
public class BreakableObject : MonoBehaviour
{
    [Header("Breaking Settings")]
    [Tooltip("Force required to break the object.")]
    public float breakForce = 2.0f;
    
    [Tooltip("Optional: Prefab of broken pieces to spawn.")]
    public GameObject fracturedPrefab;
    
    [Tooltip("Optional: Sound to play on break.")]
    public AudioClip breakSound;
    
    [Tooltip("Optional: Particle effect to spawn.")]
    public ParticleSystem breakParticles;

    [Header("Respawn Settings")]
    public bool autoRespawn = true;
    public float respawnDelay = 5.0f;

    private Vector3 originalPos;
    private Quaternion originalRot;
    private Rigidbody rb;
    private MeshRenderer meshRenderer;
    private Collider col;
    private bool isBroken = false;
    private GameObject currentFracturedInstance;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        col = GetComponent<Collider>();
        originalPos = transform.position;
        originalRot = transform.rotation;
    }

    [Tooltip("Y Height at which the object is destroyed.")]
    public float voidHeight = -5.0f; // Changed default to -5 for faster testing

    void Update()
    {
        // Safety Clean-up: If object falls into the void (sea), destroy it
        if (transform.position.y < voidHeight)
        {
            Debug.Log($"[Breakable] {gameObject.name} fell into void at Y={transform.position.y}. Destroying!");
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isBroken) return;

        // Ignore collisions with the player's hands to prevent accidental breaking while holding
        // (Simple check: if velocity is very low, it might just be a grab)
        
        // Check impact force (Relative Velocity Magnitude)
        if (collision.relativeVelocity.magnitude >= breakForce)
        {
            // Debug.Log($"Impact Force: {collision.relativeVelocity.magnitude}"); // Uncomment to tune force
            Break();
        }
    }

    public void Break()
    {
        isBroken = true;

        // Play Sound
        if (breakSound) AudioSource.PlayClipAtPoint(breakSound, transform.position);

        // Spawn Particles
        if (breakParticles) Instantiate(breakParticles, transform.position, Quaternion.identity);

        // Spawn Fractured Pieces (if any)
        if (fracturedPrefab)
        {
            currentFracturedInstance = Instantiate(fracturedPrefab, transform.position, transform.rotation);
            // Optional: Apply explosion force to pieces?
            // Rigidbody[] pieces = currentFracturedInstance.GetComponentsInChildren<Rigidbody>();
            // foreach(var p in pieces) p.AddExplosionForce(2.0f, transform.position, 1.0f);
        }

        // Hide main object
        if (meshRenderer) meshRenderer.enabled = false;
        if (col) col.enabled = false;
        rb.isKinematic = true;

        if (autoRespawn)
        {
            Invoke(nameof(Respawn), respawnDelay);
        }
        else
        {
            Destroy(gameObject, 0.1f);
        }
    }

    void Respawn()
    {
        // Cleanup old fractured pieces
        if (currentFracturedInstance != null) Destroy(currentFracturedInstance);

        isBroken = false;
        
        // Reset Physics
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Reset Transform
        transform.position = originalPos;
        transform.rotation = originalRot;

        // Show Object
        if (meshRenderer) meshRenderer.enabled = true;
        if (col) col.enabled = true;
    }
}
