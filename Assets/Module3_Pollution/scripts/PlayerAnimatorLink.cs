using UnityEngine;

public class PlayerAnimatorLink : MonoBehaviour
{
    public Animator animator;
    public CharacterController controller;
    public float smoothing = 10f;

    float speed;

    void Start()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (animator == null || controller == null) return;

        Vector3 v = controller.velocity;
        v.y = 0f;

        speed = Mathf.Lerp(speed, v.magnitude, Time.deltaTime * smoothing);

        animator.SetFloat("Speed", speed);
        animator.SetBool("Running", Input.GetKey(KeyCode.LeftShift) && speed > 0.1f);
    }
}