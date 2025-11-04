using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TopDownMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    public Rigidbody rb;
    private Vector3 moveInput;

    void Update()
    {
        // Get input (WASD / Arrow Keys)
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.z = Input.GetAxisRaw("Vertical");

        // Normalize so diagonal isn't faster
        moveInput = moveInput.normalized;
    }

    void FixedUpdate()
    {
        if (moveInput != Vector3.zero)
        {
            // Rotate player to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveInput, Vector3.up);
            rb.MoveRotation(targetRotation);
        }

        // Apply movement with physics
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}
