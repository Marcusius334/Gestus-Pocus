using System;
using UnityEngine;

public class Explanation : MonoBehaviour
{
    [Header("Gesture animation size")]
    [Tooltip("How much of the explanation area the gesture animation fills (1 = touches the edges of the area). " +
             "The animation keeps its proportions, so it is not stretched.")]
    [SerializeField, Range(0.1f, 1f)] private float animationFill = 0.9f;

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
            FitAnimationToArea(spell.ExplanationFrames);
            SpriteSheetAnimator animator = GetAnimator();
            animator.FramesPerSecond = spell.ExplanationFps;
            animator.SetSheet(spell.ExplanationFrames);
            return;
        }

        // No sheet: show the spell's static sprite like before.
        StopAnimation();
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
    /// </summary>
    private void FitAnimationToArea(Sprite[] frames)
    {
        Vector2 frameSize = TypicalFrameSize(frames);
        Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;

        if (frameSize.x <= 0f || frameSize.y <= 0f || parentScale.x == 0f || parentScale.y == 0f)
        {
            transform.localScale = originalScale;
            return;
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
            FitAnimationToArea(spell.ExplanationFrames);
            SpriteSheetAnimator animator = GetAnimator();
            animator.FramesPerSecond = spell.ExplanationFps;
            animator.SetSheet(spell.ExplanationFrames);
            return;
        }

        // No sheet: show the spell's static sprite like before.
        StopAnimation();
        SpriteRenderer otherSprite = gameObject.GetComponent<SpriteRenderer>();
        thisSprite.sprite = otherSprite.sprite;
        thisSprite.color = otherSprite.color;
    }
}
