using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OceanExplorer.Module2
{
    public class M2_SpeciesHud : MonoBehaviour
    {
        public TMP_Text countText;
        public M2_SpeciesSpawner spawner;
        float timer;

        void Update()
        {
            timer += Time.deltaTime;
            if (timer > 0.5f)
            {
                timer = 0f;
                int fish = GameObject.FindGameObjectsWithTag("Fish").Length;
                int others = GameObject.FindGameObjectsWithTag("Species").Length;
                countText.text = "Poissons : " + fish + "\nCrabes et tortues : " + others;
            }

            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                foreach (Transform child in spawner.transform)
                    Destroy(child.gameObject);
                spawner.Respawn();
            }
        }
    }
}