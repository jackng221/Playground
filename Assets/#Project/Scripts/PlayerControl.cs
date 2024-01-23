using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    CharacterController characterController;
    Vector3 movement;
    public float speed = 1000;
    public float speedJump = 2;
    public float gravity = -9.81f;
    float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            movement += transform.forward;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            movement -= transform.forward;
        }
        
        if (Input.GetKey(KeyCode.D))
        {
            movement += transform.right;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            movement -= transform.right;
        }

        movement = movement.normalized * speed;

        if (characterController.isGrounded == false)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        else if (Input.GetKeyDown(KeyCode.Space))
        {
            verticalVelocity = Mathf.Sqrt(speedJump * gravity * -1);
        }

        movement.y = verticalVelocity;
        characterController.Move(movement * Time.deltaTime);
    }

    public int testFps = 999;
    private void OnValidate()
    {
        Application.targetFrameRate = testFps;
    }
}
