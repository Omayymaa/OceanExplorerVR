using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrashSpawner : MonoBehaviour
{
    public GameObject[] trashPrefabs;
    public Transform robot;
    public Transform bin;
    public Transform affiche;

    public Vector2 areaSize = new Vector2(24, 24);
    public float minDistance = 3f;
    public float spawnInterval = 3f;
    public float minHeight = 12f;
    public float maxHeight = 18f;
    public int initialCount = 6;
    public float minScale = 0.8f;
    public float maxScale = 1.4f;

    [Header("Zones interdites (rayon en mètres)")]
    public float rayonAffiche = 4f;
    public float rayonPoubelle = 2.5f;

    List<Vector3> recent = new List<Vector3>();
    int count;

    void Start()
    {
        if (trashPrefabs == null || trashPrefabs.Length == 0)
        {
            Debug.LogWarning("TrashSpawner : aucun prefab assigné.");
            return;
        }
        StartCoroutine(SpawnLoop());
    }

    Vector3 GetCenter()
    {
        Vector3 sum = Vector3.zero;
        int n = 0;

        if (robot != null) { sum += robot.position; n++; }
        if (bin != null) { sum += bin.position; n++; }
        if (affiche != null) { sum += affiche.position; n++; }

        if (n == 0) return transform.position;
        return sum / n;
    }

    float Flat(Vector3 a, Vector3 b)
    {
        return Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }

    bool PositionValide(Vector3 pos)
    {
        // Pas dans l'affiche
        if (affiche != null && Flat(pos, affiche.position) < rayonAffiche) return false;

        // Pas dans la poubelle
        if (bin != null && Flat(pos, bin.position) < rayonPoubelle) return false;

        // Pas trop près des derniers déchets
        foreach (Vector3 p in recent)
            if (Flat(pos, p) < minDistance) return false;

        return true;
    }

    Vector3 FindPosition()
    {
        Vector3 c = GetCenter();
        Vector3 pos = c;

        for (int i = 0; i < 40; i++)
        {
            pos = new Vector3(
                c.x + Random.Range(-areaSize.x / 2, areaSize.x / 2),
                Random.Range(minHeight, maxHeight),
                c.z + Random.Range(-areaSize.y / 2, areaSize.y / 2));

            if (PositionValide(pos)) break;
        }

        recent.Add(pos);
        if (recent.Count > 8) recent.RemoveAt(0);
        return pos;
    }

    void SpawnOne()
    {
        GameObject prefab = trashPrefabs[count % trashPrefabs.Length];
        GameObject o = Instantiate(prefab, FindPosition(), Random.rotation, transform);
        o.transform.localScale *= Random.Range(minScale, maxScale);
        count++;
    }

    IEnumerator SpawnLoop()
    {
        for (int i = 0; i < initialCount; i++)
        {
            SpawnOne();
            yield return new WaitForSeconds(0.4f);
        }

        while (true)
        {
            SpawnOne();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 c = GetCenter();
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(c.x, maxHeight, c.z), new Vector3(areaSize.x, 0.5f, areaSize.y));

        if (affiche != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(affiche.position, rayonAffiche);
        }
        if (bin != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(bin.position, rayonPoubelle);
        }
    }
}