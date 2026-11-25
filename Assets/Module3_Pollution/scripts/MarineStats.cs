using UnityEngine;

public enum CollectSystem { None, Claw, Conveyor }

public static class MarineStats
{
    public static int Collected;
    public static int Capacity = 30;
    public static CollectSystem Active = CollectSystem.None;

    public static bool Full { get { return Collected >= Capacity; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset()
    {
        Collected = 0;
        Active = CollectSystem.None;
    }

    public static bool Register(WasteItem w)
    {
        if (w == null || w.sorted || !w.inSea) return false;
        if (Full) return false;

        w.sorted = true;
        Collected++;
        return true;
    }

    public static bool TryCollect(WasteItem w)
    {
        if (!Register(w)) return false;

        Object.Destroy(w.gameObject);
        return true;
    }
}