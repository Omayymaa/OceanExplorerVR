using UnityEngine;

public enum WasteType { Plastique, Verre, Metal, Papier }

public static class WasteInfo
{
    public static string Label(WasteType t)
    {
        switch (t)
        {
            case WasteType.Plastique: return "Plastique";
            case WasteType.Verre: return "Verre";
            case WasteType.Metal: return "Métal";
            default: return "Papier / Carton";
        }
    }

    public static Color GetColor(WasteType t)
    {
        switch (t)
        {
            case WasteType.Plastique: return new Color(1f, 0.85f, 0.1f);
            case WasteType.Verre: return new Color(0.15f, 0.7f, 0.25f);
            case WasteType.Metal: return new Color(0.65f, 0.67f, 0.72f);
            default: return new Color(0.2f, 0.45f, 0.95f);
        }
    }

    public static void Paint(GameObject o, Color c)
    {
        Renderer r = o.GetComponent<Renderer>();
        if (r != null) r.material.color = c;
    }
}