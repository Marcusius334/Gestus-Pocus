using System;
using TMPro;
using UnityEngine;

public class Explanation : MonoBehaviour
{
    [Header("Gesture animation size")]
    [Tooltip("How much of the explanation area the gesture animation fills (1 = touches the edges of the area). " +
             "The animation keeps its proportions, so it is not stretched.")]
    [SerializeField, Range(0.1f, 1f)] private float animationFill = 0.9f;

    [Header("Sign text (left of the animation)")]
    [Tooltip("Text that says in Spanish which sign is shown. Leave empty to create it automatically " +
             "to the left of the animation. If you assign your own text, it is not moved.")]
    [SerializeField] private TMP_Text signText;
    [Tooltip("Width of the automatic text box, in world units.")]
    [SerializeField, Min(0.5f)] private float signTextWidth = 5f;
    [Tooltip("Space between the text and the animation, in world units.")]
    [SerializeField, Min(0f)] private float signTextGap = 0.3f;
    [SerializeField] private Color signTextColor = new Color(0.15f, 0.08f, 0.02f, 1f);
    private bool signTextCreatedHere;

    private GameObject previousGameObject;

    // Size of the explanation area, taken from how the object is set up in the scene/prefab.
    private bool initialized;
    private Vector3 originalScale;
    private Vector2 areaSpriteSize;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (initialized) return;
        initialized = true;

        originalScale = transform.localScale;

        SpriteRenderer thisSprite = GetComponent<SpriteRenderer>();
        areaSpriteSize = (thisSprite != null && thisSprite.sprite != null)
            ? (Vector2)thisSprite.sprite.bounds.size
            : Vector2.one;
    }

    public void Explain(GameObject gameObject){
        Init();

        if (previousGameObject != null){
            SpellMovement script = previousGameObject.GetComponent<SpellMovement>();
            script.explaining = false;
        }

        previousGameObject = gameObject;

        SpriteRenderer thisSprite = this.GetComponent<SpriteRenderer>();
        SpellMovement spell = gameObject.GetComponent<SpellMovement>();

        // Spell has a sprite sheet: play it animated.
        if (spell != null && spell.ExplanationFrames != null && spell.ExplanationFrames.Length > 0)
        {
            thisSprite.color = Color.white;
            Vector2 animationSize = FitAnimationToArea(spell.ExplanationFrames);
            ShowSignText(spell.SignName, animationSize);
            SpriteSheetAnimator animator = GetAnimator();
            animator.FramesPerSecond = spell.ExplanationFps;
            animator.SetSheet(spell.ExplanationFrames);
            return;
        }

        // No sheet: show the spell's static sprite like before.
        StopAnimation();
        HideSignText();
        SpriteRenderer otherSprite = gameObject.GetComponent<SpriteRenderer>();
        thisSprite.sprite = otherSprite.sprite;
        thisSprite.color = otherSprite.color;
    }

    private SpriteSheetAnimator GetAnimator()
    {
        SpriteSheetAnimator animator = GetComponent<SpriteSheetAnimator>();
        if (animator == null) animator = gameObject.AddComponent<SpriteSheetAnimator>();
        return animator;
    }

    private void StopAnimation()
    {
        SpriteSheetAnimator animator = GetComponent<SpriteSheetAnimator>();
        if (animator != null) animator.Clear();

        // Static sprites use the original size of the explanation area.
        transform.localScale = originalScale;
    }

    /// <summary>
    /// Scales this object so the animation fits inside the explanation area
    /// (the area the static sprite covers) without being stretched.
    /// Returns the size of the animation in world units (zero if it could not be fitted).
    /// </summary>
    private Vector2 FitAnimationToArea(Sprite[] frames)
    {
        Vector2 frameSize = TypicalFrameSize(frames);
        Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;

        if (frameSize.x <= 0f || frameSize.y <= 0f || parentScale.x == 0f || parentScale.y == 0f)
        {
            transform.localScale = originalScale;
            return Vector2.zero;
        }

        // Explanation area in world units.
        float areaWidth = Mathf.Abs(areaSpriteSize.x * originalScale.x * parentScale.x) * animationFill;
        float areaHeight = Mathf.Abs(areaSpriteSize.y * originalScale.y * parentScale.y) * animationFill;

        // Same factor on both axes, so the animation keeps its proportions.
        float worldScale = Mathf.Min(areaWidth / frameSize.x, areaHeight / frameSize.y);

        transform.localScale = new Vector3(
            Mathf.Sign(originalScale.x) * worldScale / parentScale.x,
            Mathf.Sign(originalScale.y) * worldScale / parentScale.y,
            originalScale.z);

        return frameSize * worldScale;
    }

    // ------------------------------------------------------------------ Sign text

    /// <summary>Shows the Spanish name of the sign to the left of the animation.</summary>
    private void ShowSignText(string signName, Vector2 animationSize)
    {
        if (string.IsNullOrWhiteSpace(signName))
        {
            HideSignText();
            return;
        }

        if (!EnsureSignText()) return;

        signText.text = "<size=160%><b>" + signName + "</b></size>";
        signText.gameObject.SetActive(true);

        // A text assigned in the Inspector stays where it was placed.
        if (!signTextCreatedHere) return;

        // Same height as the animation (or the whole area if the size is unknown), right edge next to it.
        float height = animationSize.y > 0f ? animationSize.y : AreaWorldSize().y;
        signText.rectTransform.sizeDelta = new Vector2(signTextWidth, height);
        signText.transform.position = transform.position +
            new Vector3(-(animationSize.x * 0.5f + signTextGap + signTextWidth * 0.5f), 0f, 0f);
    }

    private void HideSignText()
    {
        if (signText != null) signText.gameObject.SetActive(false);
    }

    /// <summary>Creates the text next to the explanation if none was assigned. False if it can't.</summary>
    private bool EnsureSignText()
    {
        if (signText != null) return true;

        // Child of the book (like this object), but with world scale 1 so the text isn't stretched.
        Transform parent = transform.parent;
        Vector3 parentScale = parent != null ? parent.lossyScale : Vector3.one;
        if (parentScale.x == 0f || parentScale.y == 0f) return false;

        GameObject go = new GameObject("SignText", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.transform.localScale = new Vector3(1f / parentScale.x, 1f / parentScale.y, 1f);
        go.transform.rotation = transform.rotation;

        TextMeshPro text = go.AddComponent<TextMeshPro>();
        text.alignment = TextAlignmentOptions.Center;
        text.color = signTextColor;
        text.enableAutoSizing = true;
        text.fontSizeMin = 1f;
        text.fontSizeMax = 5f;

        // Draw on top of the book, like the animation.
        SpriteRenderer thisSprite = GetComponent<SpriteRenderer>();
        MeshRenderer textRenderer = go.GetComponent<MeshRenderer>();
        if (thisSprite != null && textRenderer != null)
        {
            textRenderer.sortingLayerID = thisSprite.sortingLayerID;
            textRenderer.sortingOrder = thisSprite.sortingOrder + 1;
        }

        signText = text;
        signTextCreatedHere = true;
        return true;
    }

    /// <summary>Size of the explanation area in world units.</summary>
    private Vector2 AreaWorldSize()
    {
        Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        return new Vector2(
            Mathf.Abs(areaSpriteSize.x * originalScale.x * parentScale.x),
            Mathf.Abs(areaSpriteSize.y * originalScale.y * parentScale.y));
    }

    /// <summary>
    /// Median frame size of the sheet. One size for the whole sheet, so the gesture
    /// does not change size between frames; the median ignores badly sliced frames.
    /// </summary>
    private static Vector2 TypicalFrameSize(Sprite[] frames)
    {
        int count = 0;
        float[] widths = new float[frames.Length];
        float[] heights = new float[frames.Length];

        foreach (Sprite frame in frames)
        {
            if (frame == null) continue;
            widths[count] = frame.bounds.size.x;
            heights[count] = frame.bounds.size.y;
            count++;
        }

        if (count == 0) return Vector2.zero;

        Array.Sort(widths, 0, count);
        Array.Sort(heights, 0, count);
        return new Vector2(widths[count / 2], heights[count / 2]);
    }

    public void ExplainCodex(GameObject gameObject){
        Init();

        if (previousGameObject != null){
            CodexSpells script = previousGameObject.GetComponent<CodexSpells>();
            script.explaining = false;
        }

        previousGameObject = gameObject;

        SpriteRenderer thisSprite = this.GetComponent<SpriteRenderer>();
        CodexSpells spell = gameObject.GetComponent<CodexSpells>();

        // Spell has a sprite sheet: play it animated.
        if (spell != null && spell.ExplanationFrames != null && spell.ExplanationFrames.Length > 0)
        {
            thisSprite.color = Color.white;
            Vector2 animationSize = FitAnimationToArea(spell.ExplanationFrames);
            ShowSignText(spell.SignName, animationSize);
            SpriteSheetAnimator animator = GetAnimator();
            animator.FramesPerSecond = spell.ExplanationFps;
            animator.SetSheet(spell.ExplanationFrames);
            return;
        }

        // No sheet: show the spell's static sprite like before.
        StopAnimation();
        HideSignText();
        SpriteRenderer otherSprite = gameObject.GetComponent<SpriteRenderer>();
        thisSprite.sprite = otherSprite.sprite;
        thisSprite.color = otherSprite.color;
    }
}
