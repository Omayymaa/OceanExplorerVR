using System;
using UnityEngine;

namespace OceanExplorer.Common
{
    public class SimulationManager : MonoBehaviour
    {
        public static SimulationManager Instance { get; private set; }

        public event Action<int> OnTrashCollected;
        public event Action<string> OnSpeciesObserved;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void TrashCollected(int total) => OnTrashCollected?.Invoke(total);
        public void SpeciesObserved(string name) => OnSpeciesObserved?.Invoke(name);
    }
}