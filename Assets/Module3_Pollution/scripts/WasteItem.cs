using UnityEngine;

public class WasteItem : MonoBehaviour
{
    public WasteType type;
    public string displayName = "Déchet";
    public bool inSea;
    [HideInInspector] public bool sorted;

    public static int Alive;

    void OnEnable() { Alive++; }
    void OnDisable() { Alive--; }
}