using UnityEngine;
using UnityEngine.InputSystem;

public class VRCharacterController : MonoBehaviour
{
    [Header("References")]
    public CharacterController characterController;
    public Transform head;

    [Header("Movement")]
    public float speed = 2.0f;
    public float gravity = -9.81f;

    [Header("Input")]
    public InputActionProperty moveInput;

    private Vector3 velocity;

    void Update()
    {
        HandleMovement();
        ApplyGravity();
    }

    void HandleMovement()
    {
        Vector2 input = moveInput.action.ReadValue<Vector2>();

        // Dirección basada en la cabeza (look direction)
        Vector3 forward = head.forward;
        Vector3 right = head.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 direction = forward * input.y + right * input.x;

        characterController.Move(direction * speed * Time.deltaTime);
    }

    void ApplyGravity()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}