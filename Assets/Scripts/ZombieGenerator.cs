using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieGenerator : MonoBehaviour
{
    // Serialize Fields
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private GameObject area;
    [SerializeField] private int initialZombies = 10;
    [SerializeField] private int increaseEachWaveBy = 10;
    
    // Local Variables
    private int maxAttempts = 100; // Max attempts to find a valid position

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Function to spawn a wave
    public bool SpawnWave(int waveNumber)
    {
        // Calculate the total number of zombies for this wave
        int totalNumberOfZombies = initialZombies + (waveNumber - 1) * increaseEachWaveBy;
        for (int i = 0; i < totalNumberOfZombies; i++)
        {
            Vector3 spawnPosition = GetValidSpawnPosition(); // Get random position on field
            if (spawnPosition != Vector3.zero)
            {
                // Create zombie
                Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                // If it failed finding a valid position the log the warning but do nothing else
                Debug.LogWarning("Couldn't find a valid spawn position for zombie.");
                return false;
            }
        }

        return true;
    }
    
    // Get a valid spawn position function
    private Vector3 GetValidSpawnPosition()
    {
        // Get the bounds of the ground area
        Bounds areaBounds = area.GetComponent<Collider>().bounds;

        // Try 100 (maxAttempts) times to find a valid position
        for (int i = 0; i < maxAttempts; i++)
        {
            // Generate a random position within the area bounds
            Vector3 randomPosition = new Vector3(
                Random.Range(areaBounds.min.x, areaBounds.max.x),
                areaBounds.min.y,
                Random.Range(areaBounds.min.z, areaBounds.max.z)
            );

            // Check if there's an object at this position
            if (!Physics.CheckSphere(randomPosition, 1f, LayerMask.GetMask("Object")))
            {
                return randomPosition;
            }
        }

        // If a valid position is not found after maxAttempts, return Vector3.zero
        return Vector3.zero;
    }
}
