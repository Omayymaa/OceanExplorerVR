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
        public float turnSpeed = 6f;
        public float minY = 11f;
        public float maxY = 30f;
        public float minX = 5f;
        public float maxX = 95f;
        public float minZ = 5f;
        public float maxZ = 95f;
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

            Vector3 move = new Vector3(x, y, z);
            Vector3 pos = transform.position + move * speed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
            transform.position = pos;

            Vector3 flat = new Vector3(x, 0, z);
            if (flat.sqrMagnitude > 0.01f)
            {
                Quaternion look = Quaternion.LookRotation(flat);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            // Cherche SpeciesData sur l'objet touché, puis sur ses parents
            SpeciesData data = other.GetComponentInParent<SpeciesData>();
            if (data == null) return;
            if (infoPanel == null || infoText == null) return;

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