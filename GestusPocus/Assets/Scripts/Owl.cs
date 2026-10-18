using TMPro;
using UnityEngine;
using System.Collections;

public class Owl : MonoBehaviour
{
    public ErrorsManager errorManager;
    public string[] levelClues;
    public int clueCounter = 0;

    [SerializeField] private GameObject clue;
    [SerializeField] private TMP_Text cluesText;
    [SerializeField] private float clueTimer = 5.0f;
    private bool usingClue;
    
    void Start(){
        usingClue = false;
    }

    private void OnMouseDown()
    {
        if(clueCounter >= levelClues.Length){
            clueCounter = levelClues.Length-1;
        }
        
        if(errorManager.clues > 0 && !usingClue){
            usingClue = true;
            errorManager.ClueUsed();
            Clue();
        }
    }

    private void Clue(){
        clue.SetActive(true);
        cluesText.text = levelClues[clueCounter];
        StartCoroutine(disableClue());
    }

    IEnumerator disableClue()
    {
        yield return new WaitForSeconds(clueTimer);

        usingClue = false;
        clue.SetActive(false);
    }
}
