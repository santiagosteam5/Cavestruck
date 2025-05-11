using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private float movementX;
    private float movementY;
    public float speed = 8;
    public float jumppower = 12f;
    private float jumpBufferTime = 0.2f;
    private float lastJumpTime = -1f;

    private int maxJumps = 2;
    private int jumpCount = 0;

    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.mass = 1f;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
    }

    void Update()
    {
        jumpBro();
    }

    private void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        Debug.Log($"MovementX: {movementX}");
    }

    private void jumpBro()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            lastJumpTime = Time.time;
            animator.SetBool("isJumping", true);
        }

        if (Time.time - lastJumpTime <= jumpBufferTime && jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // Reset Y velocity before jumping
            rb.AddForce(Vector3.up * jumppower, ForceMode.Impulse);
            jumpCount++;
            lastJumpTime = -1f;
        }
    }

    private void FixedUpdate()
    {
        Vector3 targetVelocity = new Vector3(movementX * speed, rb.linearVelocity.y, 0.0f);

        if (movementX != 0)
        {
            animator.SetBool("isMoving", true);
            if (movementX > 0)
            {
                transform.rotation = Quaternion.Euler(0, 90, 0);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, -90, 0);
            }
        }
        else
        {
            animator.SetBool("isMoving", false);
        }

        rb.linearVelocity = targetVelocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            animator.SetBool("isJumping", false);
            jumpCount = 0; // Reset jumps on touching ground
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Lógica para recibir daño o morir
            Die();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
    }

    public void Die()
    {
        Debug.Log("Jugador muerto");
        transform.position = RespawnManager.Instance.GetCheckpoint();
        rb.linearVelocity = Vector3.zero; // Resetea la velocidad
    }
}
