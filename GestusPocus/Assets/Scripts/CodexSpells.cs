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
        // Clicks on the music volume slider are not for this object.
        if (MusicVolumeSlider.IsPointerOver()) return;

        //Poner explicación
        if(explaining == false){
            explaining = true;
            explanation.ExplainCodex(gameObject);
        }
    }
}
