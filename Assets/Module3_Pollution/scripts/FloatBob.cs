using UnityEngine;

public class FloatBob : MonoBehaviour
{
    public float amplitude = 0.04f;
    public float speed = 1.2f;
    public float driftRange = 0.6f;

    Vector3 start;
    Quaternion baseRot;
    float phase;

    void Start()
    {
        start = transform.position;
        baseRot = transform.rotation;
        phase = Random.value * 10f;
    }

    void Update()
    {
        float t = Time.time * speed + phase;
        Vector3 p = start;
        p.x += Mathf.Sin(t * 0.2f) * driftRange;
        p.z += Mathf.Cos(t * 0.17f) * driftRange;
        p.y += Mathf.Sin(t) * amplitude;
        transform.position = p;

        transform.rotation = baseRot * Quaternion.Euler(Mathf.Sin(t * 0.8f) * 6f, 0, Mathf.Cos(t * 0.6f) * 6f);
    }
}