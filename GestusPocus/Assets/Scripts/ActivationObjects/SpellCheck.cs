using UnityEngine;

public class SpellCheck : MonoBehaviour
{
    [Header("Linked object")]
    public string spellRequired;
    public GameObject linkedGameObject;
    public LinkedObject linkedObject;

    [Header("Owl")]
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
