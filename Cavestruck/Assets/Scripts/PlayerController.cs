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
    public float speed = 6;
    public float jumppower = 6f;
    private bool isGrounded = true;
    private float jumpBufferTime = 0.2f;
    private float lastJumpTime = -1f;

    void Start()
    {
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
        if(Input.GetKeyDown(KeyCode.Space))
        {
            lastJumpTime = Time.time;
        }

        if(isGrounded && Time.time - lastJumpTime <= jumpBufferTime)
        {
            rb.AddForce(Vector3.up * jumppower, ForceMode.Impulse);
            isGrounded = false;
            lastJumpTime = -1f;
        }
    }

    private void FixedUpdate()
    {
        Vector3 targetVelocity = new Vector3(movementX * speed, rb.linearVelocity.y, 0.0f);
        rb.linearVelocity = targetVelocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}