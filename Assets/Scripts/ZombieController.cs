using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ZombieController : MonoBehaviour
{
    // Serialize Fields
    [SerializeField] private Animator animator;
    [SerializeField] private float velocity = 0.0015f;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private RectTransform zombieHealthBar;

    // Local Variables
    private GameObject player;
    private GameObject gameManager;
    private const float AttackingDistance = 1.2f;
    private const float FocusingDistance = 100f;
    private float damage;
    private float width, height;
    private float distance;
    private float health;
    
    // Public variables
    public bool isDead = false;

    // Getter and setter for Health variable
    public float Health
    {
        get => health;
        set
        {
            health = Mathf.Clamp(value, 0, maxHealth); // Ensure health stays within valid range
            var newWidth = (float)health / (float)maxHealth * width; // Calculate new width
            zombieHealthBar.sizeDelta = new Vector2(newWidth, height);
            if (health <= 0)
            {
                Die(); // If health < 0 -> Die
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get player and game manager
        player = GameObject.FindGameObjectWithTag("Player");
        gameManager = GameObject.FindGameObjectWithTag("GameManager");
        health = maxHealth; // Set health to max health
        
        // Health bar of zombie
        width = zombieHealthBar.sizeDelta.x;
        height = zombieHealthBar.sizeDelta.y;
        var newWidth = (float)health / (float)maxHealth * width;
        zombieHealthBar.sizeDelta = new Vector2(newWidth, height);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead)
        {
            // calculate the distance between player and zombie
            // Can be adjusted so zombies only follow player when they are nearer than 5m for example
            // For now it is set to a very high number so the zombies always try to walk towards the players position
            distance = Vector3.Distance(gameObject.transform.position, player.transform.position);

            switch (distance)
            {
                case < AttackingDistance: // Only let zombies attack when they are nearer than a certain distance
                    gameObject.GetComponent<Transform>()
                        .LookAt(new Vector3(player.transform.position.x, 0, player.transform.position.z));
                    animator.SetBool("isWalking", false);
                    animator.SetBool("isAttacking", true);
                    break;
                case < FocusingDistance: // Only let zombies follow player when they are nearer than a certain distance
                    gameObject.GetComponent<Transform>()
                        .LookAt(new Vector3(player.transform.position.x, 0, player.transform.position.z));
                    transform.Translate(new Vector3(0, 0, 1) * velocity);
                    animator.SetBool("isWalking", true);
                    animator.SetBool("isAttacking", false);
                    break;
                default:
                    animator.SetBool("isWalking", false);
                    animator.SetBool("isAttacking", false);
                    break;
            }
        }
    }

    // Is called inside the zombie attack animation with an event
    // When player got hit by zombie -> run animation
    void OnAttackHit()
    {
        damage = UnityEngine.Random.Range(1f, 5f);
        player.GetComponent<PlayerController>().Health -= damage;
        player.GetComponent<Animator>().SetTrigger("gotHit");
    }

    // Die function
    private void Die()
    {
        isDead = true;
        
        // Activate dying animation and disable all other animations
        animator.SetBool("isDying", true);
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", false);

        // Start the coroutine to wait for 5 seconds before removing the zombie
        StartCoroutine(RemoveZombieAfterDelay(5f));
    }

    // Removing Zombie After Delay coroutine
    private IEnumerator RemoveZombieAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject); // Remove the zombie after the delay
        gameManager.GetComponent<GameManager>().CurrentNumberOfZombies--;
    }
}