using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
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

    private bool isDead = false;

    private bool isOnGround = false;

    private bool isJumping = false;

    private Vector3 movement;

    private Rigidbody rb;

    private float health;
    public float MaxHealth => maxHealth;

    public float Health
    {
        get => health;
        set
        {
            health = Mathf.Clamp(value, 0, maxHealth); // Ensure health stays within valid range
            if (health <= 0)
            {
                StopCoroutine(Heal());
                Die();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        var data = JsonUtility.FromJson<GameConfig>(jsonFile.ToString());
        objectCrit.SetActive(false);
        Health = maxHealth;
        rb = gameObject.GetComponent<Rigidbody>();
        if (probabilityCriticalHit > 1) probabilityCriticalHit = 1;
        else if (probabilityCriticalHit < 0) probabilityCriticalHit = 0;
        StartCoroutine(CheckForGround());
        if (data.autoHeal)
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

    void Move()
    {
        if (movement != Vector3.zero)
        {
            // Align player rotation with movement direction relative to camera
            Vector3 direction = Camera.main!.transform.TransformDirection(movement);
            direction.y = 0f; // Ignore vertical rotation

            transform.rotation = Quaternion.LookRotation(-direction);

            float movementStrength = Vector3.Magnitude(movement);
            transform.Translate(new Vector3(0, 0, 1) * (velocity * movementStrength));
            animator.SetBool("isWalking", true);
        }

        if (movement == Vector3.zero)
        {
            animator.SetBool("isWalking", false);
        }
    }

    void OnMovement(InputValue inputValue)
    {
        Vector2 movementInput = inputValue.Get<Vector2>();
        movement = new Vector3(-movementInput.x, 0f, -movementInput.y);
    }

    void OnJump(InputValue inputValue)
    {
        animator.SetBool("isJumping", true);

        if (isJumping || !isOnGround)
            return;

        StartCoroutine(JumpControlFlow());
    }

    private IEnumerator JumpControlFlow()
    {
        isJumping = true;
        float jumpHeight = transform.position.y + maxJumpHeight;

        rb.AddForce(Vector3.up * jumpForce, forceMode);
        while (transform.position.y < jumpHeight)
        {
            yield return null;
        }

        rb.AddForce(Vector3.up * (jumpForce * -1 * fallFactor), ForceMode.Force);
        isJumping = false;
    }

    private IEnumerator CheckForGround()
    {
        RaycastHit hit;
        while (true)
        {
            bool raycastSuccess = Physics.Raycast(transform.position, transform.up * -1, out hit);
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

    private IEnumerator Heal()
    {
        while (true)
        {
            if (health < 100)
            {
                Health += healPerSecond;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    void OnAttack(InputValue inputValue)
    {
        if (!isDead)
        {
            var randomNumber = Random.Range(1, 4);
            animator.SetTrigger("attack" + randomNumber);

            RaycastHit hit;
            var raycastSuccess = Physics.Raycast(transform.position, transform.forward, out hit);
            if (raycastSuccess && hit.collider.gameObject.CompareTag("Zombie") && hit.distance <= 1.4f)
            {
                if (!hit.collider.gameObject.GetComponent<ZombieController>().isDead)
                {
                    // Deduct health from zombie
                    var damage = Random.Range(minDamage, maxDamage);
                    var criticalHitVariable = Random.Range(0, 100);
                    if (criticalHitVariable < probabilityCriticalHit * 100)
                    {
                        damage *= 2;
                        StopCoroutine(ShowAndFadeObjectCrit());
                        StartCoroutine(ShowAndFadeObjectCrit());
                    }

                    hit.collider.gameObject.GetComponent<ZombieController>().Health -= damage;
                }
            }
        }
    }

    private IEnumerator ShowAndFadeObjectCrit()
    {
        objectCrit.SetActive(true); // Show ObjectCrit
        CanvasGroup canvasGroup = objectCrit.GetComponent<CanvasGroup>();
        if (!canvasGroup)
        {
            canvasGroup = objectCrit.AddComponent<CanvasGroup>(); // Add CanvasGroup if not present
        }

        canvasGroup.alpha = 1f; // Make sure the alpha is fully opaque at the start

        float duration = 3f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration); // Gradually fade out
            yield return null;
        }

        objectCrit.SetActive(false); // Hide ObjectCrit after fading out
    }

    private void Die()
    {
        isDead = true;
        StopCoroutine(JumpControlFlow());
        StopCoroutine(CheckForGround());
        animator.SetBool("isDying", true);
        animator.SetBool("isJumping", false);
        animator.SetBool("isWalking", false);
    }
}