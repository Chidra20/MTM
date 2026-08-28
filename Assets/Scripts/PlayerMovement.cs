using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    // ========== KEYBINDS ==========
    // Easy to change jump & sprint keys in the Inspector.
    // Horizontal still uses Unity's "Horizontal" axis (A/D + arrows). Change in Project Settings → Input Manager if needed.
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.W;
    public KeyCode sprintKey = KeyCode.Space;

    // ========== MOVEMENT ==========
    // Core horizontal speed and jump height. Edit moveSpeed / jumpForce to change feel.
    // Remove sprintMultiplier if you don't want sprinting.
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float sprintMultiplier = 2f;
    public float jumpForce = 12f;

    // ========== SPRINT ==========
    // How long you can sprint and the cooldown after it empties.
    // Set sprintDuration very high + sprintCooldown to 0 for unlimited sprint.
    [Header("Sprint")]
    public float sprintDuration = 3f;
    public float sprintCooldown = 5f;

    // ========== GROUND CHECK ==========
    // Raycast that detects if the player is on the ground.
    // Assign checkGround (empty child under the player feet). Edit groundCheckDistance if the ray is too short/long.
    // Removing this breaks jumping and coyote time.
    [Header("Ground Check")]
    public Transform checkGround;
    public float groundCheckDistance = 0.15f;
    public LayerMask groundLayer;

    // ========== JUMP FEEL ==========
    // Coyote time = short grace period after leaving a platform where you can still jump.
    // Jump buffer = short window that remembers a jump press so it triggers when you land.
    // Set both to 0 to disable the extra forgiveness.
    [Header("Jump Feel")]
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;

    // ========== DJUMP (WALL JUMP) ==========
    // Detects walls tagged "Djump" and allows one upward boost.
    // djumpCooldown stops spamming / infinite climbing.
    // Assign checkWall (empty child roughly at torso height). Edit wallCheckDistance if needed.
    // Removing the cooldown lets the player spam and climb forever.
    [Header("Djump")]
    public Transform checkWall;
    public float djumpUpForce = 10f;
    public float wallCheckDistance = 0.5f;
    public LayerMask djumpLayer;
    public float djumpCooldown = 0.25f;

    // ========== UI ==========
    // Optional sprint status text. Leave empty if you don't want it.
    [Header("UI")]
    public Text sprintText;

    // Runtime variables (don't edit these)
    private Rigidbody2D rb;
    private bool canJump;
    private bool canDJump;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private float sprintTimer;
    private float sprintCooldownTimer;
    private float moveInput;
    private bool isSprinting;
    private float djumpCooldownTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprintTimer = sprintDuration;
    }

    void Update()
    {
        // ----- MOVEMENT INPUT -----
        // Reads left/right input every frame. Removing this stops all horizontal movement.
        moveInput = Input.GetAxisRaw("Horizontal");

        // ----- GROUND CHECK -----
        // Simple downward ray. Sets canJump. Required for normal jumps and resetting Djump.
        canJump = Physics2D.Raycast(
            checkGround.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        // ----- COYOTE TIME -----
        // Gives a tiny window after walking off a ledge to still jump. Feels more forgiving.
        // Removing it makes jumps feel stricter (must be fully grounded).
        if (canJump)
        {
            coyoteTimer = coyoteTime;
            canDJump = false; // Landing resets wall-jump availability
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        // ----- DJUMP COOLDOWN -----
        // Counts down after a wall jump so you can't instantly re-trigger it.
        // Removing this (or setting djumpCooldown = 0) allows infinite wall climbing.
        if (djumpCooldownTimer > 0f)
            djumpCooldownTimer -= Time.deltaTime;

        // ----- WALL CHECK -----
        // Rays left + right looking for objects tagged "Djump".
        // Only runs when airborne, not on cooldown, and Djump isn't already ready.
        // Removing this disables wall jumps entirely.
        if (!canJump && !canDJump && djumpCooldownTimer <= 0f)
        {
            if (checkWall != null)
            {
                RaycastHit2D wallHitRight = Physics2D.Raycast(checkWall.position, Vector2.right, wallCheckDistance, djumpLayer);
                RaycastHit2D wallHitLeft = Physics2D.Raycast(checkWall.position, Vector2.left, wallCheckDistance, djumpLayer);

                if ((wallHitRight.collider != null && wallHitRight.collider.CompareTag("Djump")) ||
                    (wallHitLeft.collider != null && wallHitLeft.collider.CompareTag("Djump")))
                {
                    canDJump = true;
                }

                // Visual debug (Scene view only)
                Debug.DrawRay(checkWall.position, Vector2.right * wallCheckDistance, Color.cyan);
                Debug.DrawRay(checkWall.position, Vector2.left * wallCheckDistance, Color.cyan);
            }
        }

        // ----- JUMP / DJUMP INPUT -----
        // Press jumpKey → do wall jump if available, otherwise buffer a normal jump.
        // Changing jumpKey in the Inspector is the only thing you need to edit here.
        if (Input.GetKeyDown(jumpKey))
        {
            if (canDJump)
            {
                Djump();
            }
            else
            {
                jumpBufferTimer = jumpBufferTime;
            }
        }

        // ----- JUMP BUFFER -----
        // Counts down the remembered jump press. Removing it means you must press jump exactly while grounded.
        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.deltaTime;

        // ----- NORMAL JUMP -----
        // Executes a regular jump when buffer + coyote are both active.
        // Removing this disables normal jumping.
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            Jump();
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        // ----- SPRINT -----
        // Hold sprintKey to run faster while the meter has charge.
        // Edit sprintDuration / sprintCooldown / sprintKey to tweak.
        // Removing the whole block disables sprinting.
        if (sprintCooldownTimer > 0f)
            sprintCooldownTimer -= Time.deltaTime;

        isSprinting = Input.GetKey(sprintKey) && sprintTimer > 0f && sprintCooldownTimer <= 0f;

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
            sprintTimer = Mathf.Min(sprintTimer + Time.deltaTime, sprintDuration);
        }

        // ----- UI -----
        // Updates the optional sprint text. Safe to delete if you don't use it.
        if (sprintText != null)
        {
            if (sprintCooldownTimer > 0f)
                sprintText.text = "Sprint: " + sprintCooldownTimer.ToString("0.0");
            else if (sprintTimer < sprintDuration)
                sprintText.text = "Sprint: " + sprintTimer.ToString("0.0");
            else
                sprintText.text = "Sprint Ready";
        }

        // ----- FACING -----
        // Flips the player sprite left/right. Removing it keeps the sprite facing one direction only.
        if (moveInput > 0f)
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        else if (moveInput < 0f)
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
    }

    void FixedUpdate()
    {
        // ----- APPLY HORIZONTAL MOVEMENT -----
        // Sets the actual physics velocity. Runs in FixedUpdate for consistency.
        // Removing this stops all left/right movement.
        float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        rb.linearVelocity = new Vector2(
            moveInput * speed,
            rb.linearVelocity.y
        );
    }

    // ----- NORMAL JUMP -----
    // Instantly sets upward velocity. Edit jumpForce to change height.
    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    // ----- DJUMP (WALL JUMP) -----
    // Simple upward boost only. Cooldown prevents spam / infinite climbing.
    // Edit djumpUpForce for height and djumpCooldown for how fast you can re-use it.
    void Djump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, djumpUpForce);

        canDJump = false;
        djumpCooldownTimer = djumpCooldown;

        Debug.Log("DJUMP!");
    }
}