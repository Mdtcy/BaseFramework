using MoreMountains.Tools;
using UnityEngine;

public struct HitEvent
{
    public Vector3 Position;
    public HitEvent(Vector3 pos)
    {
        Position = pos;
    }

    private static HitEvent e;

    public static void Trigger(Vector3 pos)
    {
        e.Position = pos;
        MMEventManager.TriggerEvent(e);
    }
}
