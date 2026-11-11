using System.Collections.Generic;
using UnityEngine;

namespace OceanExplorer.Module2
{
    // Un type d'objet à disperser (coquillage, étoile, roche...)
    [System.Serializable]
    public class ScatterItem
    {
        public GameObject prefab;      // Le prefab à placer
        public int count = 10;         // Combien d'exemplaires
        public float baseScale = 1f;   // Taille de base
        public float randomScale = 0.2f; // Variation aléatoire de la taille
        public float yOffset = 0f;     // Décalage vertical (négatif = enfoncé dans le sol)
    }

    // Disperse des objets de décor sur toute la carte, posés sur le sol
    public class M2_DecorScatter : MonoBehaviour
    {
        public ScatterItem[] items;    // Liste des objets à disperser
        public int gridSize = 8;       // Nombre de cases par côté (la carte est découpée en grille)
        public float mapMin = 8f;      // Début de la zone (X et Z)
        public float mapMax = 92f;     // Fin de la zone (X et Z)
        public Vector2 keepFreeCenter = new Vector2(50f, 15f);   // Zone laissée vide (X, Z)
        public float keepFreeRadius = 5f;                        // Rayon de la zone vide

        void Start()
        {
            float cell = (mapMax - mapMin) / gridSize;   // Taille d'une case

            // Liste de toutes les cases de la grille
            List<Vector2Int> cells = new List<Vector2Int>();
            for (int x = 0; x < gridSize; x++)
                for (int z = 0; z < gridSize; z++)
                    cells.Add(new Vector2Int(x, z));

            foreach (ScatterItem item in items)
            {
                if (item.prefab == null) continue;

                // Mélange les cases : chaque type d'objet visite des cases dans un ordre différent
                for (int i = cells.Count - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    Vector2Int t = cells[i]; cells[i] = cells[j]; cells[j] = t;
                }

                for (int n = 0; n < item.count; n++)
                {
                    // Une case après l'autre, position aléatoire dans la case
                    Vector2Int c = cells[n % cells.Count];
                    float x = mapMin + (c.x + Random.value) * cell;
                    float z = mapMin + (c.y + Random.value) * cell;

                    // Ignore la zone gardée libre (départ du plongeur)
                    if (Vector2.Distance(new Vector2(x, z), keepFreeCenter) < keepFreeRadius) continue;

                    // Hauteur du sol à CE point, sur le bon terrain
                    if (!TryGetGroundHeight(x, z, out float y)) continue;

                    Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    GameObject go = Instantiate(item.prefab, new Vector3(x, y + item.yOffset, z), rot, transform);

                    // Taille aléatoire
                    float s = item.baseScale * (1f + Random.Range(-item.randomScale, item.randomScale));
                    go.transform.localScale = Vector3.one * s;
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