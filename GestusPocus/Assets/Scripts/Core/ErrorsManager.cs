using TMPro;
using UnityEngine;

public class ErrorsManager : MonoBehaviour
{
    [Header("Texts")]
    public TMP_Text spellErrorsText;
    public TMP_Text cluesText;
    public TMP_Text bookUsesText;

    [Header("Tries")]
    public int spellErrors = 10;
    public int clues = 5;
    public int bookUses = 4;

    [Header("Managers")]
    public SceneLoader sceneLoader;

    void Start(){
        spellErrorsText.text = "Fallos = " + spellErrors;
        cluesText.text = "Pistas = " + clues;
        bookUsesText.text = "Usos libro = " + (bookUses-1);
    }

    public void SpellFailed(){
        spellErrors--;
        spellErrorsText.text = "Fallos = " + spellErrors;
        if (spellErrors <= 0){
            sceneLoader.LoadScene("Fail");
        }
    }

    public void ClueUsed(){
        clues--;
        cluesText.text = "Pistas = " + clues;
    }

    public void BookUsed(){
        bookUses--;
        bookUsesText.text = "Usos libro = " + bookUses;
    }
}
