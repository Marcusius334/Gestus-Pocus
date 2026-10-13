using UnityEngine;

public class SpellCheck : MonoBehaviour
{
    public string spellRequired;
    public GameObject linkedGameObject;
    public LinkedObject linkedObject;

    void Awake(){
        linkedObject = linkedGameObject.GetComponent<LinkedObject>();
        Debug.Log("Linked -> "+linkedObject);
    }

    public void ActivateSpell(){
        linkedObject.ActivateSpell();
        Destroy(gameObject);
    }
    
}
