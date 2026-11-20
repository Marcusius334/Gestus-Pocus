using UnityEngine;

public static class GameManager
{
    public static int levelsUnlocked = 1;
    public static int actualLevel = 1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetearVariables()
    {
        levelsUnlocked = 1;
        actualLevel = 1;
    }

    public static void SetLevel(int newLevel){
        actualLevel = newLevel;
    }

    public static void NextLevel(){
        actualLevel++;
    }

    public static void UnlockLevel(){
        levelsUnlocked++;
    }

    public static string StringLevel(){
        switch (actualLevel){
            case 1:
                return "Level 1 Cutscene";
            
            case 2:
                return "Level 2";
            
            case 3:
                return "Level 3";

            default:
                Debug.Log("Unknown level");
                return "Start";
        }
    }
}
