using TMPro;
using UnityEngine;

public class ErrorsManager : MonoBehaviour
{
    public TMP_Text spellErrorsText;
    public TMP_Text bookUsesText;

    public int spellErrors = 10;
    public int bookUses = 3;

    void Start(){
        spellErrorsText.text = "SpellErrors = " + spellErrors;
        bookUsesText.text = "BookUses = " + bookUses;
    }

    public void SpellFailed(){
        spellErrors--;
        spellErrorsText.text = "SpellErrors = " + spellErrors;
        if (spellErrors <= 0){
            Debug.Log("Failed level");
        }
    }

    public void BookUsed(){
        bookUses--;
        bookUsesText.text = "BookUses = " + bookUses;
    }
}
