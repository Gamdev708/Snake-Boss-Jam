using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] InputActionReference moveAction;
    [SerializeField] InputActionReference dashAction;
    [SerializeField] InputActionReference attackAction;
    [SerializeField] InputActionReference jumpAction;

    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float dashSpeed = 10f;
    [SerializeField] float dashDuration = 0.5f;
    [SerializeField] float dashDistance = 1f;

    Rigidbody2D rb;
    CapsuleCollider2D capsuleCollider;
    Vector2 moveDirection;
    Vector2 jumpDirection;

    private void OnEnable()
    {
        dashAction.action.performed += OnDash;
        attackAction.action.started += OnAttack;
        jumpAction.action.started += OnJump;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, rb.linearVelocity.y);
    }

    private void OnDisable()
    {
        dashAction.action.performed -= OnDash;
        attackAction.action.started -= OnAttack;
        jumpAction.action.started -= OnJump;
    }
}
