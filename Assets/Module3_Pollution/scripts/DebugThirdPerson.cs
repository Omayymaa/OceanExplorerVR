using UnityEngine;

public class DebugThirdPerson : MonoBehaviour
{
    public Transform cameraTransform;
    public KeyCode toggleKey = KeyCode.V;
    public Vector3 offset = new Vector3(0f, 0.5f, -2.8f);

    Vector3 basePosition;
    bool third;

    void Start()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform != null)
            basePosition = cameraTransform.localPosition;
    }

    void Update()
    {
        if (cameraTransform == null || !Input.GetKeyDown(toggleKey)) return;

        third = !third;
        FirstPersonBody.ShowBody = third;
        cameraTransform.localPosition = third ? basePosition + offset : basePosition;
    }
}