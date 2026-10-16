using TMPro;
using UnityEngine;

public class ErrorsManager : MonoBehaviour
{
    public TMP_Text spellErrorsText;
    public TMP_Text bookUsesText;
    public SceneLoader sceneLoader;

    public int spellErrors = 10;
    public int bookUses = 4;

    void Start(){
        spellErrorsText.text = "SpellErrors = " + spellErrors;
        bookUsesText.text = "BookUses = " + (bookUses-1);
    }

    public void SpellFailed(){
        spellErrors--;
        spellErrorsText.text = "SpellErrors = " + spellErrors;
        if (spellErrors <= 0){
            sceneLoader.LoadScene("Fail");
        }
    }

    public void BookUsed(){
        bookUses--;
        bookUsesText.text = "BookUses = " + bookUses;
    }
}
