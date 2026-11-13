using UnityEngine;

public class CodexSpells : MonoBehaviour
{
    public Explanation explanation;
    public bool explaining;

    void Awake()
    {
        explaining = false;
    }

    private void OnMouseDown()
    {
        //Poner explicación
        if(explaining == false){
            explaining = true;
            explanation.ExplainCodex(gameObject);
        }
    }
}
