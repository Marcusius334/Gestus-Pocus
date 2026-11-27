using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Picks the sprite sheet for the current level: Level 1 = primavera, 2 = verano, 3 = otoño, 4 = invierno.
/// Used by MageAnimator and OwlAnimator.
/// </summary>
public static class LevelSheets
{
    /// <summary>
    /// Current level number: forceLevel if it is not 0, else the number in a scene called "Level N",
    /// else the level chosen in the menu (GameManager.actualLevel).
    /// </summary>
    public static int CurrentLevel(int forceLevel)
    {
        if (forceLevel != 0) return forceLevel;

        Match match = Regex.Match(SceneManager.GetActiveScene().name, @"^Level\s*(\d+)$");
        return match.Success ? int.Parse(match.Groups[1].Value) : GameManager.actualLevel;
    }

    /// <summary>Sheet for the current level (sheets[0] = Level 1). Null if there are no sheets.</summary>
    public static Texture2D Choose(Texture2D[] sheets, int forceLevel)
    {
        if (sheets == null || sheets.Length == 0) return null;

        int index = Mathf.Clamp(CurrentLevel(forceLevel), 1, sheets.Length) - 1;
        return sheets[index];
    }
}
