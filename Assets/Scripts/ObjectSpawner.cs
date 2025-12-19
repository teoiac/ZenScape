using UnityEngine;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("The Breakable Object Prefab to spawn.")]
    public GameObject objectToSpawn;
    
    [Tooltip("List of Transform points where objects will spawn.")]
    public List<Transform> spawnPoints;

    [Header("Timing")]
    [Tooltip("Time to wait before checking for empty spots.")]
    public float checkInterval = 1.0f;
    [Tooltip("Delay AFTER a spot is found empty before spawning a new item.")]
    public float respawnDelay = 3.0f;

    [Header("Performance Limits")]
    [Tooltip("Maximum total objects allowed in the room.")]
    public int maxObjects = 10;

    private List<GameObject> activeObjects = new List<GameObject>();
    // Track which points are currently "pending" a respawn so we don't double-queue
    private HashSet<Transform> pendingRespawn = new HashSet<Transform>();

    void Start()
    {
        Debug.Log($"[Spawner] Started. Manager: {gameObject.name}. Points: {spawnPoints.Count}");
        SpawnAll();
        // Check frequently so we catch the empty spot quickly, then apply the specific delay
        InvokeRepeating(nameof(CheckAndRespawn), 1.0f, checkInterval);
    }

    void SpawnAll()
    {
        foreach (var point in spawnPoints)
        {
            if (point != null && point.childCount == 0) SpawnObject(point);
        }
    }

    void CheckAndRespawn()
    {
        // 1. Cleanup formatted list (remove destroyed objects)
        activeObjects.RemoveAll(item => item == null);

        // 2. Check points
        foreach (var point in spawnPoints)
        {
            if (point == null) continue;

            // If point is empty AND not already waiting for a new one...
            if (point.childCount == 0 && !pendingRespawn.Contains(point))
            {
                // Start the delayed spawn routine
                StartCoroutine(SpawnRoutine(point));
            }
        }
    }

    System.Collections.IEnumerator SpawnRoutine(Transform point)
    {
        // Mark as pending
        pendingRespawn.Add(point);
        Debug.Log($"[Spawner] Spot '{point.name}' is empty. Waiting {respawnDelay}s...");

        // Wait
        yield return new WaitForSeconds(respawnDelay);

        // Debug Status
        Debug.Log($"[Spawner] Wait done. Checks: ChildCount={point.childCount}, Active={activeObjects.Count}/{maxObjects}");

        // Double check
        if (point.childCount == 0 && activeObjects.Count < maxObjects)
        {
            Debug.Log($"[Spawner] SPAM! Spawning new object at '{point.name}'");
            SpawnObject(point);
        }
        else
        {
             if (point.childCount > 0) 
             {
                 Debug.Log($"[Spawner] Cancelled at '{point.name}'. Blocked by: '{point.GetChild(0).name}'");
             }
             if (activeObjects.Count >= maxObjects) Debug.Log($"[Spawner] Cancelled: Max objects active.");
        }

        // Done
        pendingRespawn.Remove(point);
    }

    void SpawnObject(Transform point)
    {
        if (objectToSpawn != null)
        {
            GameObject obj = Instantiate(objectToSpawn, point.position, point.rotation);
            obj.transform.SetParent(point);
            activeObjects.Add(obj);
        }
    }
}
