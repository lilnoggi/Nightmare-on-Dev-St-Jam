using UnityEngine;

public class playerAudio : MonoBehaviour
{
    void PlayRunEvent()
    {
        FMOD.Studio.EventInstance Run = FMODUnity.RuntimeManager.CreateInstance("event:/Run");
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(Run, gameObject, GetComponent<Rigidbody>());
        Run.start();
        Run.release();
    }
    void PlayWalkEvent()
    {
        FMOD.Studio.EventInstance Walk = FMODUnity.RuntimeManager.CreateInstance("event:/Walk");
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(Walk, gameObject, GetComponent<Rigidbody>());
        Walk.start();
        Walk.release();
    }
    void PlayTripEvent()
    {
        FMOD.Studio.EventInstance Trip = FMODUnity.RuntimeManager.CreateInstance("event:/Trip");
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(Trip, gameObject, GetComponent<Rigidbody>());
        Trip.start();
        Trip.release();
    }

}

