using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TimeController : MonoBehaviour
{
    public delegate void TimeStatesDelegate();
    public TimeStatesDelegate eventStartTime;
    public TimeStatesDelegate eventEndTime;

    public delegate void TimeChangeDelegate(float newTime);
    public TimeChangeDelegate eventTimeModified;

    protected float currentTime;
    protected bool activated = false;

    public virtual void Initiate(float time)
    {
        eventStartTime?.Invoke();
        currentTime = time;
        activated = true;
    }

    public void Stop()
    {
        activated = false;
    }

    public float GetTime()
    {
        return currentTime;
    }

    public abstract void Reboot();

    public abstract void End();

}

