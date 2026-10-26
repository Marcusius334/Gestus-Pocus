using TMPro;
using UnityEngine;
using System.Collections;

public class Owl : MonoBehaviour
{
    [Header("Number of clues")]
    public string[] levelClues;
    public int clueCounter = 0;

    [Header("Clues text")]
    [SerializeField] private GameObject clue;
    [SerializeField] private TMP_Text cluesText;
    [SerializeField] private float clueTimer = 5.0f;
    private bool usingClue;

    [Header("Managers")]
    public ErrorsManager errorManager;
    public AudioManager audioManager;
    
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
        audioManager.PlayOne(audioManager.soundOwl);
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

    public void disableClueBook()
    {
        usingClue = false;
        clue.SetActive(false);
    }
}
