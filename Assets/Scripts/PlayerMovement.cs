using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float sprintMultiplier = 2f;
    public float jumpForce = 12f;

    [Header("Sprint")]
    public float sprintDuration = 3f;
    public float sprintCooldown = 5f;

    [Header("Ground Check")]
    public Transform checkGround;
    public float groundCheckDistance = 0.15f;
    public LayerMask groundLayer;

    [Header("Jump Feel")]
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;

    [Header("Wall Check")]
    public Transform checkWall;
    public float wallCheckDistance = 0.5f;
    public LayerMask wallLayer;

    [Header("UI")]
    public Text sprintText;

    private Rigidbody2D rb;

    private bool canJump;
    private bool canDJump;

    private float coyoteTimer;
    private float jumpBufferTimer;

    private float sprintTimer;
    private float sprintCooldownTimer;

    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        sprintTimer = sprintDuration;
    }

    void Update()
    {
        // -------------------------
        // MOVEMENT INPUT
        // -------------------------

        moveInput = Input.GetAxisRaw("Horizontal");

        // -------------------------
        // GROUND CHECK
        // -------------------------

        canJump = Physics2D.Raycast(
            checkGround.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        // Coyote time
        if (canJump)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        // -------------------------
        // JUMP BUFFER
        // -------------------------

        if (Input.GetKeyDown(KeyCode.W))
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        // -------------------------
        // JUMP
        // -------------------------

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            Jump();

            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        // -------------------------
        // SPRINT
        // -------------------------

        if (sprintCooldownTimer > 0f)
        {
            sprintCooldownTimer -= Time.deltaTime;
        }

        bool sprinting =
            Input.GetKey(KeyCode.Space) &&
            sprintTimer > 0f &&
            sprintCooldownTimer <= 0f;

        if (sprinting)
        {
            sprintTimer -= Time.deltaTime;
        }

        // Sprint released or depleted
        if (!Input.GetKey(KeyCode.Space) || sprintTimer <= 0f)
        {
            if (sprintTimer <= 0f)
            {
                sprintCooldownTimer = sprintCooldown;
            }

            sprintTimer = Mathf.Min(
                sprintTimer + Time.deltaTime,
                sprintDuration
            );
        }

        // -------------------------
        // UI
        // -------------------------

        if (sprintText != null)
        {
            if (sprintCooldownTimer > 0f)
            {
                sprintText.text =
                    "Sprint: " + sprintCooldownTimer.ToString("0.0");
            }
            else
            {
                sprintText.text = "Sprint Ready";
            }
        }
    }

    void FixedUpdate()
    {
        float speed = moveSpeed;

        bool sprinting =
            Input.GetKey(KeyCode.Space) &&
            sprintTimer > 0f &&
            sprintCooldownTimer <= 0f;

        if (sprinting)
        {
            speed *= sprintMultiplier;
        }

        rb.linearVelocity = new Vector2(
            moveInput * speed,
            rb.linearVelocity.y
        );
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );
    }
}