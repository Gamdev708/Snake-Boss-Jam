using System;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    private List<EventInstance> eventInstances;
    private EventInstance musicEventInstance;
    public static AudioManager instance {get; private set;}
    void Awake()
    {
        if (instance != null)
        {
            Debug.Log("More than one istance found");
        }
        instance = this;

        eventInstances = new List<EventInstance>();
    }

    private void Start()
    {
        InitializeMusic(FMODEvents.instance.bossMusic);
    }

    public EventInstance CreateEventInstance(EventReference eventReference)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
            eventInstances.Add(eventInstance);
            return eventInstance;
        }

    private void InitializeMusic(EventReference musicRef)
        {
            musicEventInstance = CreateEventInstance(musicRef);
            musicEventInstance.start();
        }

    public void UpdateEventInstanceParameter(EventInstance eventInstance, string parameterName, int value)
        {
            eventInstance.setParameterByName(parameterName, value);
        }

    public void PhaseTestChange(int value)
        {
            Debug.Log("Test!" + value);
            UpdateEventInstanceParameter(musicEventInstance, "Boss_State", value);
        }

    private void CleanUp()
        {
            foreach(EventInstance ei in eventInstances)
            {
                ei.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                ei.release();
            }
        }

    private void OnDestroy()
        {
            CleanUp();
        }
}
