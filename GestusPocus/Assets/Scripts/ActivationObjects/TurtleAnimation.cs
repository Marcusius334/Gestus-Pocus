using UnityEngine;

public class TurtleAnimation : MonoBehaviour, LinkedObject
{
    public Vector3 endPosition;

    [Header("Managers")]
    public AudioManager audioManager;
    public PlayerMovement player;

    public void ActivateSpell(){
        audioManager.PlayOne(audioManager.soundDoor);
        Debug.Log("Activated");
        player.Animate(3, endPosition);
    }
}
