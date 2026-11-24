using UnityEngine;
using System.Collections;

public class TruckAnimation : MonoBehaviour, LinkedObject
{
    public Vector3 endPosition;

    [Header("Managers")]
    public AudioManager audioManager;
    public PlayerMovement player;

    public void ActivateSpell(){
        audioManager.PlayOne(audioManager.soundDoor);
        Debug.Log("Activated");
        player.Animate(6, endPosition);
        StartCoroutine(DestroyWall());
    }

    IEnumerator DestroyWall(){

        float duration = 0.5f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}