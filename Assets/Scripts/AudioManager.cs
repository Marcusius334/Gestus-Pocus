using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio sources")]
    public AudioSource audioSource;
    public AudioSource loopAudioSource;

    [Header("General sounds")]
    public AudioClip soundCorrect;
    public AudioClip soundWrong;
    public AudioClip soundBook;
    public AudioClip soundOwl;
    public AudioClip soundJump;

    [Header("Loop sounds")]
    public AudioClip soundMove;

    [Header("Activation object sounds")]
    public AudioClip soundDoor;

    public void PlayOne(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    public void PlayLoop(AudioClip clip){
        loopAudioSource.clip = clip;
        loopAudioSource.Play();
    }

    public void StopLoop(){
        loopAudioSource.Stop();
    }
}
