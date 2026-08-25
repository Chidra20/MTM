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

    [Header("Djump")]
    public Transform checkWall;                 // ← was missing
    public float djumpUpForce = 10f;
    public float djumpAwayForce = 12f;
    public float wallCheckDistance = 0.5f;
    public LayerMask djumpLayer;

    [Header("UI")]
    public Text sprintText;

    private Rigidbody2D rb;

    private bool canJump;
    private bool canDJump;
    private Vector2 djumpWallNormal;

    private float coyoteTimer;
    private float jumpBufferTimer;

    private float sprintTimer;
    private float sprintCooldownTimer;

    private float moveInput;
    private bool isSprinting;

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
        // GROUND CHECK + COYOTE
        // -------------------------
        canJump = Physics2D.Raycast(
            checkGround.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        if (canJump)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        // -------------------------
        // JUMP BUFFER
        // -------------------------
        if (Input.GetKeyDown(KeyCode.W))
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        // -------------------------
        // WALL CHECK (Djump)
        // -------------------------

        canDJump = false;

        if (checkWall != null)
        {
            RaycastHit2D wallHit = Physics2D.Raycast(
                checkWall.position,
                checkWall.right,
                wallCheckDistance,
                djumpLayer
            );

            if (wallHit.collider != null)
            {
                Debug.Log(
                    "Wall hit: " +
                    wallHit.collider.name +
                    " | Tag: " +
                    wallHit.collider.tag
                );

                if (!canJump && wallHit.collider.CompareTag("Djump"))
                {
                    canDJump = true;
                    djumpWallNormal = wallHit.normal;

                    Debug.Log("DJUMP WALL DETECTED!");
                }
            }

            Debug.DrawRay(
                checkWall.position,
                checkWall.right * wallCheckDistance,
                canDJump ? Color.green : Color.red
            );
        }

        // -------------------------
        // JUMP / DJUMP
        // -------------------------
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            Jump();
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }
        else if (Input.GetKeyDown(KeyCode.W) && canDJump)
        {
            Djump();
            canDJump = false;
        }

        // -------------------------
        // SPRINT
        // -------------------------
        if (sprintCooldownTimer > 0f)
            sprintCooldownTimer -= Time.deltaTime;

        isSprinting = Input.GetKey(KeyCode.Space) &&
                      sprintTimer > 0f &&
                      sprintCooldownTimer <= 0f;

        if (isSprinting)
        {
            sprintTimer -= Time.deltaTime;

            if (sprintTimer <= 0f)
            {
                sprintTimer = 0f;
                sprintCooldownTimer = sprintCooldown;
            }
        }
        else
        {
            // Regen only when not sprinting
            sprintTimer = Mathf.Min(sprintTimer + Time.deltaTime, sprintDuration);
        }

        // -------------------------
        // UI
        // -------------------------
        if (sprintText != null)
        {
            if (sprintCooldownTimer > 0f)
                sprintText.text = "Sprint: " + sprintCooldownTimer.ToString("0.0");
            else if (sprintTimer < sprintDuration)
                sprintText.text = "Sprint: " + sprintTimer.ToString("0.0");
            else
                sprintText.text = "Sprint Ready";
        }

        // -------------------------
        // FACING
        // -------------------------
        if (moveInput > 0f)
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        else if (moveInput < 0f)
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
    }

    void FixedUpdate()
    {
        float speed = moveSpeed;

        if (isSprinting)
            speed *= sprintMultiplier;

        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void Djump()
    {
        Vector2 force = new Vector2(
            djumpWallNormal.x * djumpAwayForce,
            djumpUpForce
        );

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);
    }
}