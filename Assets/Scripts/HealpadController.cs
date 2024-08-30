using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HealpadController : MonoBehaviour
{
    // Local Variables
    private Coroutine healCoroutine;

    // Function when someone collides with the healpad
    private void OnTriggerEnter(Collider other)
    {
        // If player collides with healpad
        if (other.CompareTag("Player"))
        {
            // Get the player
            var playerController = other.gameObject.GetComponent<PlayerController>();
            // Start the healing coroutine if not already started
            if (healCoroutine == null)
            {
                healCoroutine = StartCoroutine(Heal(playerController));
            }
        }
    }

    // Function when someone leaves the healpad
    private void OnTriggerExit(Collider other)
    {
        // If player leaves the healpad
        if (other.CompareTag("Player"))
        {
            // If coroutine is already running
            if (healCoroutine != null)
            {
                // Stop coroutine
                StopCoroutine(healCoroutine);
                healCoroutine = null; // Reset the reference
            }
        }
    }

    // Heal Coroutine
    private IEnumerator Heal(PlayerController playerController)
    {
        // Heal the player every second by 0.5f
        while (true)
        {
            if (playerController.Health < 100)
            {
                playerController.Health += 0.5f;
            }
            
            yield return new WaitForSeconds(1f);
        }
    }
}