using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieGenerator : MonoBehaviour
{
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private GameObject area;

    [SerializeField] private int initialZombies = 10;
    [SerializeField] private int increaseEachWaveBy = 30;
    
    private int maxAttempts = 100; // Max attempts to find a valid position

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool SpawnWave(int waveNumber)
    {
        int totalNumberOfZombies = initialZombies + (waveNumber - 1) * increaseEachWaveBy;
        for (int i = 0; i < totalNumberOfZombies; i++)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();
            if (spawnPosition != Vector3.zero)
            {
                Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("Couldn't find a valid spawn position for zombie.");
                return false;
            }
        }

        return true;
    }
    
    private Vector3 GetValidSpawnPosition()
    {
        Bounds areaBounds = area.GetComponent<Collider>().bounds;

        for (int i = 0; i < maxAttempts; i++)
        {
            // Generate a random position within the area bounds
            Vector3 randomPosition = new Vector3(
                Random.Range(areaBounds.min.x, areaBounds.max.x),
                areaBounds.min.y, // Assuming ground level, adjust if needed
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
