using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Transform cameraTransform;
    public float walkSpeed = 3.5f;
    public float runSpeed = 6f;
    public float mouseSensitivity = 2f;
    public float gravity = -20f;
    public bool canMove = true;

    CharacterController controller;
    float pitch;
    float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        LockCursor(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) LockCursor(false);
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Input.GetMouseButtonDown(0)) LockCursor(true);
            return;
        }

        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(0f, mx, 0f);
        pitch = Mathf.Clamp(pitch - my, -80f, 80f);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        if (!controller.enabled) return;

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        Vector3 move = transform.right * Input.GetAxis("Horizontal")
                     + transform.forward * Input.GetAxis("Vertical");
        move = Vector3.ClampMagnitude(move, 1f) * speed;
        if (!canMove) move = Vector3.zero;

        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}