using UnityEngine;

public class FiletsGenerateur : MonoBehaviour
{
    public GameObject cordePrefab;
    public int nombre = 200;
    public Vector3 zone = new Vector3(1.2f, 0.5f, 1.6f);

    void Start()
    {
        if (cordePrefab == null)
        {
            enabled = false;   // pas de prefab : le script s'arrête sans erreur
            return;
        }

        for (int i = 0; i < nombre; i++)
        {
            Vector3 pos = transform.position + new Vector3(
                Random.Range(-zone.x / 2, zone.x / 2),
                Random.Range(-zone.y / 2, zone.y / 2),
                Random.Range(-zone.z / 2, zone.z / 2));

            GameObject c = Instantiate(cordePrefab, pos, Random.rotation, transform);
            c.transform.localScale = new Vector3(0.015f, Random.Range(0.15f, 0.4f), 0.015f);
        }
    }
}