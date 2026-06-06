using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
    [Header("Input"), Space]
    [SerializeField] InputActionReference moveAction;
    [SerializeField] InputActionReference dashAction;
    [SerializeField] InputActionReference attackAction;
    [SerializeField] InputActionReference jumpAction;

    [Header("Player Properties"), Space]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float dashSpeed = 10f;
    [SerializeField] float dashDuration = 0.5f;
    [SerializeField] float dashDistance = 1f;
    [SerializeField] float fallMultiplier = 2f;

    [Header("Ground Check"), Space]
    [SerializeField] Transform groundDetector;
    [SerializeField] float groundCheckDistance = 0.2f;
    [SerializeField] LayerMask groundLayer;

    Rigidbody2D rb;
    CapsuleCollider2D capsuleCollider;
    Vector2 moveDirection;
    Vector2 jumpDirection;
    bool isGrounded;

    private void OnEnable()
    {
        dashAction.action.performed += OnDash;
        attackAction.action.started += OnAttack;
        jumpAction.action.started += OnJump;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    private void Update()
    {
        moveDirection.x = moveAction.action.ReadValue<Vector2>().x;
        CheckIsGrounded();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, rb.linearVelocity.y);
        HandleBetterFall();
    }

    private void CheckIsGrounded()
    {
        RaycastHit2D raycastHit2D = Physics2D.Raycast(groundDetector.position, Vector2.down, groundCheckDistance, groundLayer);
        if (raycastHit2D.collider is TilemapCollider2D)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
        //isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (dashAction.action.IsPressed())
        {
            Debug.Log("Attacking while dashing!");
        }
        else
        {
            Debug.Log("Attacking normally.");
        }
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        Vector2 dashPosition = rb.position + moveDirection * dashSpeed;
        rb.MovePosition(dashPosition);
        //StartCoroutine(Dash());
    }

    private IEnumerator Dash()
    {
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;
        rb.linearVelocityX = dashSpeed * moveDirection.x;
        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;

    }

    private void HandleBetterFall()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * fallMultiplier * Time.fixedDeltaTime;
        }
        else
        {
            rb.gravityScale = 1f; // Normal gravity when rising
        }
    }
    private void OnDisable()
    {
        dashAction.action.performed -= OnDash;
        attackAction.action.started -= OnAttack;
        jumpAction.action.started -= OnJump;
    }


}
