using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ZombieController : MonoBehaviour
{
    [SerializeField] private GameObject player;

    [SerializeField] private Animator animator;

    [SerializeField] private float velocity = 0.0015f;

    [SerializeField] private float maxHealth = 100f;

    [SerializeField] private RectTransform zombieHealthBar;

    private float damage;

    private float width, height;
    
    private float distance;

    private bool isDead = false;

    private float health;

    public float Health
    {
        get => health;
        set
        {
            health = Mathf.Clamp(value, 0, maxHealth); // Ensure health stays within valid range
            var newWidth = (float)health / (float)maxHealth * width;
            zombieHealthBar.sizeDelta = new Vector2(newWidth, height);
            if (health <= 0)
            {
                Die();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        width = zombieHealthBar.sizeDelta.x;
        height = zombieHealthBar.sizeDelta.y;
        var newWidth = (float)health / (float)maxHealth * width;
        zombieHealthBar.sizeDelta = new Vector2(newWidth, height);

        damage = UnityEngine.Random.Range(1f, 5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead)
        {
            distance = Vector3.Distance(gameObject.transform.position, player.transform.position);

            switch (distance)
            {
                case < 1.2f:
                    gameObject.GetComponent<Transform>()
                        .LookAt(new Vector3(player.transform.position.x, 0, player.transform.position.z));
                    animator.SetBool("isWalking", false);
                    animator.SetBool("isAttacking", true);
                    break;
                case < 15f:
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
    void OnAttackHit()
    {
        player.GetComponent<PlayerController>().Health -= damage;
        player.GetComponent<Animator>().SetTrigger("gotHit");
    }

    private void Die()
    {
        isDead = true;
        
        animator.SetBool("isDying", true);
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", false);

        // Start the coroutine to wait for 5 seconds before removing the zombie
        StartCoroutine(RemoveZombieAfterDelay(5f));
    }

    private IEnumerator RemoveZombieAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject); // Remove the zombie after the delay
    }
}