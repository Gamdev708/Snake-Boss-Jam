using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] InputActionReference moveAction;
    [SerializeField] InputActionReference dashAction;
    [SerializeField] InputActionReference attackAction;

    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float dashSpeed = 10f;
    [SerializeField] float dashDuration = 0.5f;
    [SerializeField] float dashDistance = 1f;

    Rigidbody2D rb;
    Vector2 moveDirection;
    Vector2 jumpDirection;

    private void OnEnable()
    {
        dashAction.action.performed += OnDash;
        attackAction.action.performed += OnAttack;
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if(dashAction.action.IsPressed())
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
    }

    private void Update()
    {
        moveDirection.x = moveAction.action.ReadValue<Vector2>().x;
        jumpDirection.y = moveAction.action.ReadValue<Vector2>().y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
        rb.AddForceAtPosition(jumpDirection * jumpForce, -rb.transform.up,ForceMode2D.Impulse);
    }

    private void OnDisable()
    {
        dashAction.action.performed -= OnDash;
        attackAction.action.performed -= OnAttack;
    }
}
