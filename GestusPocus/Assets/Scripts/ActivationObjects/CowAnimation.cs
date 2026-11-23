using UnityEngine;
using System.Collections;

public class CowAnimation : MonoBehaviour, LinkedObject
{
    public Vector3 endPosition;

    [Header("Managers")]
    public AudioManager audioManager;
    public PlayerMovement player;

    public void ActivateSpell(){
        audioManager.PlayOne(audioManager.soundDoor);
        Debug.Log("Activated");
        player.Animate(2, endPosition);
        StartCoroutine(DestroyTrees());
        GetComponent<Collider2D>().enabled = false;
    }

    IEnumerator DestroyTrees(){
         int totalHijos = transform.childCount;

        if (totalHijos == 0)
            yield break;

        float intervalo = 1f / totalHijos;

        for (int i = 0; i < totalHijos; i++)
        {
            Destroy(transform.GetChild(0).gameObject);

            yield return new WaitForSeconds(intervalo);
        }
    }
}
