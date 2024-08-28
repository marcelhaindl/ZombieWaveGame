using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HealpadController : MonoBehaviour
{
    private Coroutine healCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.gameObject.GetComponent<PlayerController>();
            if (healCoroutine == null) // Check if it's not already running
            {
                healCoroutine = StartCoroutine(Heal(playerController));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (healCoroutine != null)
            {
                StopCoroutine(healCoroutine);
                healCoroutine = null; // Reset the reference
            }
        }
    }

    private IEnumerator Heal(PlayerController playerController)
    {
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