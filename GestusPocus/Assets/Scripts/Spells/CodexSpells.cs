using UnityEngine;

public class CodexSpells : MonoBehaviour
{
    [Header("Explanation animation")]
    [Tooltip("Sliced sprite sheet shown animated in the Explanation object when this spell is clicked. " +
             "If empty, the spell's own sprite is shown instead.")]
    [SerializeField] private Texture2D explanationSheet;
    [SerializeField] private float explanationFps = 12f;
    [Tooltip("Spanish name of the sign, shown in the book to the left of the animation. " +
             "If empty, it is taken from the sprite sheet name (e.g. caracol_sheet -> Caracol).")]
    // Frames taken from explanationSheet in the editor, saved so they also work in builds.
    [SerializeField, HideInInspector] private Sprite[] explanationFrames = new Sprite[0];
    [SerializeField, HideInInspector] private Texture2D loadedExplanationSheet;

    public Sprite[] ExplanationFrames => explanationFrames;
    public float ExplanationFps => explanationFps;
    public string SignName;/* => !string.IsNullOrWhiteSpace(signName)
        ? signName.Trim()
        : SignNames.FromSheet(explanationSheet, explanationFrames);*/

    [Header("Managers")]
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
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (explanationSheet == null)
        {
            explanationFrames = new Sprite[0];
            loadedExplanationSheet = null;
        }
        else if (explanationSheet != loadedExplanationSheet)
        {
            explanationFrames = SpriteSheetAnimator.LoadFramesFromTexture(explanationSheet, this);
            loadedExplanationSheet = explanationSheet;
        }
    }
#endif
}
