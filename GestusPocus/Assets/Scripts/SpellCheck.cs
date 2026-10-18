using UnityEngine;

public class SpellCheck : MonoBehaviour
{
    public string spellRequired;
    public GameObject linkedGameObject;
    public LinkedObject linkedObject;
    public Owl owl;

    void Awake(){
        linkedObject = linkedGameObject.GetComponent<LinkedObject>();
        Debug.Log("Linked -> "+linkedObject);
    }

    public void ActivateSpell(){
        owl.clueCounter++;
        linkedObject.ActivateSpell();
        Destroy(gameObject);
    }
    
}
