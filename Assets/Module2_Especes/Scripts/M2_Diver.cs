using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using OceanExplorer.Common;

namespace OceanExplorer.Module2
{
    public class M2_Diver : MonoBehaviour
    {
        public float speed = 4f;
        public float minY = 11f;
        public float maxY = 30f;
        public GameObject infoPanel;
        public TMP_Text infoText;
        public float displayTime = 4f;

        void Start()
        {
            if (infoPanel != null) infoPanel.SetActive(false);
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            float x = (kb.rightArrowKey.isPressed ? 1 : 0) - (kb.leftArrowKey.isPressed ? 1 : 0);
            float z = (kb.upArrowKey.isPressed ? 1 : 0) - (kb.downArrowKey.isPressed ? 1 : 0);
            float y = (kb.spaceKey.isPressed ? 1 : 0) - (kb.leftShiftKey.isPressed ? 1 : 0);

            Vector3 pos = transform.position + new Vector3(x, y, z) * speed * Time.deltaTime;
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.position = pos;
        }

        void OnTriggerEnter(Collider other)
        {
            Debug.Log("Contact avec " + other.name);

            SpeciesData data = other.GetComponent<SpeciesData>();
            if (data == null) return;

            infoText.text = data.nom + "\nTaille : " + data.taille + " cm\nEspèce protégée : " + (data.protege ? "oui" : "non");
            StopAllCoroutines();
            StartCoroutine(ShowPanel());

            if (SimulationManager.Instance != null)
                SimulationManager.Instance.SpeciesObserved(data.nom);
        }

        IEnumerator ShowPanel()
        {
            infoPanel.SetActive(true);
            yield return new WaitForSeconds(displayTime);
            infoPanel.SetActive(false);
        }
    }
}