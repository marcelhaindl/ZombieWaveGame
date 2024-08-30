using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    // Serialized Fields
    [SerializeField] private float velocity = 1f;
    [SerializeField] private float jumpForce = 100f;
    [SerializeField] private ForceMode forceMode = ForceMode.Force;
    [SerializeField] private float maxJumpHeight = 3f;
    [SerializeField] private float fallFactor = 0.9f;
    [SerializeField] private Animator animator;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float minDamage = 10f;
    [SerializeField] private float maxDamage = 40f;
    [SerializeField] private float probabilityCriticalHit = 0.1f;
    [SerializeField] private float healPerSecond = 0.5f;
    [SerializeField] private GameObject objectCrit;
    [SerializeField] private TextAsset jsonFile;

    // Local Variables
    private bool isDead = false;
    private bool isOnGround = false;
    private bool isJumping = false;
    private Vector3 movement;
    private Rigidbody rb;
    private float health;
    
    // Getter of Max Health
    public float MaxHealth => maxHealth;

    // Getter and Setter of Current Health
    public float Health
    {
        get => health;
        set
        {
            health = Mathf.Clamp(value, 0, maxHealth); // Ensure health stays within valid range
            if (health <= 0)
            {
                // If health < 0 -> Stop healing and die
                StopCoroutine(Heal());
                Die();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        objectCrit.SetActive(false); // Set UI Crit Symbol inactive
        Health = maxHealth; // Set health to max health
        rb = gameObject.GetComponent<Rigidbody>();
        
        // Set boundaries for crit probability
        if (probabilityCriticalHit > 1) probabilityCriticalHit = 1;
        else if (probabilityCriticalHit < 0) probabilityCriticalHit = 0;
        
        StartCoroutine(CheckForGround()); // Start coroutine for checking ground
        
        var data = JsonUtility.FromJson<GameConfig>(jsonFile.ToString()); // Read in the config json file
        if (data.autoHeal) // if autoHeal is true, Start autohealing the player regardless of the healpad
        {
            StartCoroutine(Heal());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead)
        {
            Move();
        }
    }

    // Move function
    void Move()
    {
        // If player is moving
        if (movement != Vector3.zero)
        {
            // Align player rotation with movement direction relative to camera
            Vector3 direction = Camera.main!.transform.TransformDirection(movement);
            direction.y = 0f; // Ignore vertical rotation
            
            // Adjust the player's rotation
            transform.rotation = Quaternion.LookRotation(-direction);

            // Calculate and set the movement
            float movementStrength = Vector3.Magnitude(movement);
            transform.Translate(new Vector3(0, 0, 1) * (velocity * movementStrength));
            animator.SetBool("isWalking", true); // Set the variable for animation
        }

        // If player is not moving
        if (movement == Vector3.zero)
        {
            animator.SetBool("isWalking", false); // Stop animation
        }
    }

    // On Movement function
    void OnMovement(InputValue inputValue)
    {
        Vector2 movementInput = inputValue.Get<Vector2>(); // Get the input vector
        movement = new Vector3(-movementInput.x, 0f, -movementInput.y); // Set the movement variable
    }

    // On Jump function
    void OnJump(InputValue inputValue)
    {
        animator.SetBool("isJumping", true); // Set the variable for animation

        // If player is currently jumping and is not on the ground, then don't perform a jump again
        if (isJumping || !isOnGround)
            return;

        // Start jumping coroutine
        StartCoroutine(JumpControlFlow());
    }

    // Jumping Coroutine
    private IEnumerator JumpControlFlow()
    {
        isJumping = true; // Set flag
        var jumpHeight = transform.position.y + maxJumpHeight; // Set jump height

        // Jump by adding force to the palyer
        rb.AddForce(Vector3.up * jumpForce, forceMode);
        while (transform.position.y < jumpHeight)
        {
            yield return null;
        }

        // If it reached the top, let the player fall down
        rb.AddForce(Vector3.up * (jumpForce * -1 * fallFactor), ForceMode.Force);
        isJumping = false; // Set flag
    }

    // Checking for ground coroutine
    private IEnumerator CheckForGround()
    {
        RaycastHit hit;
        while (true)
        {
            // Raycast to the bottom
            bool raycastSuccess = Physics.Raycast(transform.position, transform.up * -1, out hit);
            // If raycast hits ground or layer "object" and is less than 0.100001 distance then ground is touched
            if (raycastSuccess &&
                (hit.collider.gameObject.CompareTag("Ground") ||
                 hit.collider.gameObject.layer == LayerMask.NameToLayer("Object")) && hit.distance <= 0.100001)
            {
                if (!isOnGround)
                {
                    animator.SetBool("isJumping", false);
                }

                isJumping = false;
                StopCoroutine(JumpControlFlow());
                isOnGround = true;
            }
            else
            {
                isOnGround = false;
            }

            yield return null;
        }
    }

    // Heal coroutine
    private IEnumerator Heal()
    {
        // Heal the player every second by a certain amount
        while (true)
        {
            if (health < 100)
            {
                Health += healPerSecond;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    // On Attack function
    void OnAttack(InputValue inputValue)
    {
        if (!isDead)
        {
            // Select a random attack animation
            var randomNumber = Random.Range(1, 4);
            animator.SetTrigger("attack" + randomNumber);

            RaycastHit hit;
            // Raycast forward
            var raycastSuccess = Physics.Raycast(transform.position, transform.forward, out hit);
            // If raycast hits zombie within a distance of 1.4f
            if (raycastSuccess && hit.collider.gameObject.CompareTag("Zombie") && hit.distance <= 1.4f)
            {
                // If zombie is not dead
                if (!hit.collider.gameObject.GetComponent<ZombieController>().isDead)
                {
                    // Calculate damage
                    var damage = Random.Range(minDamage, maxDamage);
                    // Check if it is a critical hit
                    var criticalHitVariable = Random.Range(0, 100);
                    if (criticalHitVariable < probabilityCriticalHit * 100)
                    {
                        damage *= 2; // Double the damage
                        // Stop and start coroutine
                        StopCoroutine(ShowAndFadeObjectCrit());
                        StartCoroutine(ShowAndFadeObjectCrit());
                    }

                    // Deduct health from zombie
                    hit.collider.gameObject.GetComponent<ZombieController>().Health -= damage;
                }
            }
        }
    }

    // Show and Fade Crit UI Symbol
    private IEnumerator ShowAndFadeObjectCrit()
    {
        objectCrit.SetActive(true); // Show ObjectCrit
        // Get the canvas group component
        CanvasGroup canvasGroup = objectCrit.GetComponent<CanvasGroup>();
        if (!canvasGroup)
        {
            canvasGroup = objectCrit.AddComponent<CanvasGroup>(); // Add CanvasGroup if not present
        }
        
        canvasGroup.alpha = 1f; // Set the alpha to 1f -> fully opaque at start
        float duration = 3f; // Set duration to 3 seconds
        float elapsedTime = 0f; // Time passed

        // Gradually fade out
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            yield return null;
        }

        objectCrit.SetActive(false); // Hide ObjectCrit after fading out
    }

    // Die Function
    private void Die()
    {
        isDead = true;
        // Stop coroutines
        StopCoroutine(JumpControlFlow());
        StopCoroutine(CheckForGround());
        // Start dying animation and stop all other animations
        animator.SetBool("isDying", true);
        animator.SetBool("isJumping", false);
        animator.SetBool("isWalking", false);
        
    }
}