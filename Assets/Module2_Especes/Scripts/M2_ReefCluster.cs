using System.Collections.Generic;
using UnityEngine;

namespace OceanExplorer.Module2
{
    // Place des taches d'objets (coraux, algues...) sur le sol, à la hauteur exacte du terrain
    public class M2_ReefCluster : MonoBehaviour
    {
        public GameObject[] prefabs;          // Prefabs à placer (choisis au hasard)
        public int gridSize = 4;              // Nombre de cases par côté (gridSize x gridSize taches)
        public int itemsPerPatch = 25;        // Nombre d'objets par tache
        public float patchRadius = 7f;        // Rayon d'une tache (mètres)
        public float baseScale = 40f;         // Taille de base
        public float scaleVariation = 0.4f;   // Variation aléatoire de la taille
        public float yOffset = 0f;            // Décalage vertical (négatif = enfoncé dans le sol)
        public float mapMin = 10f;            // Début de la zone (X et Z)
        public float mapMax = 90f;            // Fin de la zone (X et Z)
        public Vector2 keepFreeCenter = new Vector2(50f, 15f);   // Zone laissée vide (X, Z)
        public float keepFreeRadius = 6f;     // Rayon de la zone vide

        void Start()
        {
            if (prefabs == null || prefabs.Length == 0) return;
            float cell = (mapMax - mapMin) / gridSize;   // Taille d'une case

            for (int gx = 0; gx < gridSize; gx++)
            {
                for (int gz = 0; gz < gridSize; gz++)
                {
                    // Centre de la case, avec un léger décalage aléatoire
                    Vector2 center = new Vector2(
                        mapMin + (gx + 0.5f) * cell + Random.Range(-cell * 0.2f, cell * 0.2f),
                        mapMin + (gz + 0.5f) * cell + Random.Range(-cell * 0.2f, cell * 0.2f));

                    for (int i = 0; i < itemsPerPatch; i++)
                    {
                        // Point aléatoire dans la tache
                        Vector2 off = Random.insideUnitCircle * patchRadius;
                        float x = center.x + off.x;
                        float z = center.y + off.y;

                        // Ignore la zone gardée libre (départ du plongeur)
                        if (Vector2.Distance(new Vector2(x, z), keepFreeCenter) < keepFreeRadius) continue;

                        // Hauteur du sol à CE point, sur le bon terrain
                        if (!TryGetGroundHeight(x, z, out float y)) continue;

                        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                        if (prefab == null) continue;

                        Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                        GameObject go = Instantiate(prefab, new Vector3(x, y + yOffset, z), rot, transform);

                        // Taille aléatoire
                        float s = baseScale * (1f + Random.Range(-scaleVariation, scaleVariation));
                        go.transform.localScale = Vector3.one * s;
                    }
                }
            }
        }

        // Cherche, parmi tous les terrains, celui qui contient le point (X, Z) et renvoie la hauteur du sol
        bool TryGetGroundHeight(float x, float z, out float y)
        {
            foreach (Terrain t in Terrain.activeTerrains)
            {
                Vector3 pos = t.transform.position;
                Vector3 size = t.terrainData.size;

                if (x >= pos.x && x <= pos.x + size.x && z >= pos.z && z <= pos.z + size.z)
                {
                    y = t.SampleHeight(new Vector3(x, 0f, z)) + pos.y;
                    return true;
                }
            }
            y = 0f;
            return false;
        }
    }
}