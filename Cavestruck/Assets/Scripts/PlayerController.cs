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

    private int jumpCount = 0;
    private int extraJumpsAvailable = 0;

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

        if (Time.time - lastJumpTime <= jumpBufferTime &&
            (jumpCount == 0 || (jumpCount > 0 && extraJumpsAvailable > 0)))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumppower, ForceMode.Impulse);
            jumpCount++;

            // Si est� usando un salto extra
            if (jumpCount > 1)
            {
                extraJumpsAvailable--;
            }

            lastJumpTime = -1f;
        }
    }

    private void FixedUpdate()
    {
        Vector3 targetVelocity = new Vector3(movementX * speed, rb.linearVelocity.y, 0.0f);

        if (movementX != 0)
        {
            animator.SetBool("isMoving", true);
            transform.rotation = Quaternion.Euler(0, movementX > 0 ? 90 : -90, 0);
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
            jumpCount = 0;
            extraJumpsAvailable = 0;
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Die();
        }
    }

    public bool JumpedThisFrame()
    {
        return Input.GetKeyDown(KeyCode.Space) ||
               (lastJumpTime != -1f && Time.time - lastJumpTime <= jumpBufferTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PowerUp"))
        {
            extraJumpsAvailable = 1; // reinicia a 1, incluso si ya tiene un salto extra
            Debug.Log("�Power-up de salto extra obtenido! (Un solo uso)");
        }
    }

    public void Die()
    {
        Debug.Log("Jugador muerto");
        transform.position = RespawnManager.Instance.GetCheckpoint();
        rb.linearVelocity = Vector3.zero;
    }
}
