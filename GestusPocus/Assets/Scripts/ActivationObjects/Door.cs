using UnityEngine;

public class Door : MonoBehaviour, LinkedObject
{
    [Header("Managers")]
    public AudioManager audioManager;

    public void ActivateSpell(){
        audioManager.PlayOne(audioManager.soundDoor);
        Debug.Log("Activated");
        Destroy(gameObject);
    }
}
