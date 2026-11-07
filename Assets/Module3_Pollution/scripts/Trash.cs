using UnityEngine;

public class Trash : MonoBehaviour
{
    public float pollutionValue = 10f;   // Pollution générée
    public float spinSpeed = 30f;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (PollutionManager.Instance != null)
            PollutionManager.Instance.AddPollution(pollutionValue);
    }

    void Update()
    {
        // Rotation lente sur place pour un effet visuel
        transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime);
    }

    void OnDestroy()
    {
        if (PollutionManager.Instance != null)
            PollutionManager.Instance.RemovePollution(pollutionValue);
    }
}