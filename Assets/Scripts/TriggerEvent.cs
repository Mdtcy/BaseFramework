using Sirenix.OdinInspector;
using UnityEngine;

public class TriggerEvent : MonoBehaviour
{
    [Button]
    public void TriggerHitEvent()
    {
        HitEvent.Trigger(Vector3.down);
    }
}
