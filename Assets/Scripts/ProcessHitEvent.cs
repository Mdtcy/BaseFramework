using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

public class ProcessHitEvent : MonoBehaviour , MMEventListener<HitEvent>
{
    protected void OnEnable()
    {
        this.MMEventStartListening<HitEvent>();
    }

    protected void OnDisable()
    {
        this.MMEventStopListening<HitEvent>();
    }

    public void OnMMEvent(HitEvent eEvent)
    {
        Debug.Log($"VAR {eEvent.Position}");
    }
}
