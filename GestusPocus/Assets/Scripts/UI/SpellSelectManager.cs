using UnityEngine;

public class SpellSelectManager : MonoBehaviour
{

    public int[] SpellLevelNumber;

    void Start()
    {
        int counter = 0;

        for(int i = 0; i < GameManager.levelsUnlocked; i++){
            counter += SpellLevelNumber[i];
        }

        Transform padre = transform;

        for (int i = 0; i < padre.childCount; i++)
        {
            Transform hijo = padre.GetChild(i);
            if (i >= counter)
            {
                hijo.gameObject.SetActive(false);
            }else{
                hijo.gameObject.SetActive(true);
            }
        }
    }
}
