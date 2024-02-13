using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dino : MonoBehaviour
{
    private CharacterController character;
    private Vector3 direction;
    private float gravity = 9.81f * 2f;
    private float jumpforce = 8f;

    private bool jumpRequested = false;

    private void Awake()
    {
        character = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        direction = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        direction += Vector3.down * gravity * Time.deltaTime;

        if (character.isGrounded)
        {
            direction = Vector3.down;

            if (jumpRequested)
            {
                direction = Vector3.up * jumpforce;
                jumpRequested = false;
            }
        }

        character.Move(direction * Time.deltaTime);
    }

    // Request the object to jump
    public void RequestJump()
    {
        jumpRequested = true;
    }

    // Move the object forward
    public void MoveForward()
    {
        // Adjust this part based on how you want the object to move forward
        // For example, you might want to change the position or apply a force
        transform.Translate(Vector3.forward * Time.deltaTime);
    }
}
