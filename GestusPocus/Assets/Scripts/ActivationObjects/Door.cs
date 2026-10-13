using UnityEngine;

public class Door : MonoBehaviour, LinkedObject
{
    public void ActivateSpell(){
        Debug.Log("Activated");
        Destroy(gameObject);
    }
}
