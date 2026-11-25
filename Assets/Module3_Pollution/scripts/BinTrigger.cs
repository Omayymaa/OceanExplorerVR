using UnityEngine;

public class BinTrigger : MonoBehaviour
{
    public SortingBin bin;

    void OnTriggerEnter(Collider other)
    {
        if (bin == null) return;

        WasteItem w = other.GetComponentInParent<WasteItem>();
        if (w != null) bin.OnItemEntered(w);
    }
}