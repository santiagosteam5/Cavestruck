using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private float movementX;
    private float movementY;
    public float speed = 5f;
    public float jumppower = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    void Update()
    {
        jumpBro();
    }

    private void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    private void jumpBro()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumppower, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(movementX, movementY, 0.0f);
        rb.AddForce(movement * speed);
    }
}