using UnityEngine;

public class Explanation : MonoBehaviour
{
    private GameObject previousGameObject;

    public void Explain(GameObject gameObject){
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
    }

    public void ExplainCodex(GameObject gameObject){
        if (previousGameObject != null){
            CodexSpells script = previousGameObject.GetComponent<CodexSpells>();
            script.explaining = false;
        }

        previousGameObject = gameObject;

        StopAnimation();
        SpriteRenderer thisSprite = this.GetComponent<SpriteRenderer>();
        SpriteRenderer otherSprite = gameObject.GetComponent<SpriteRenderer>();

        thisSprite.sprite = otherSprite.sprite;
        thisSprite.color = otherSprite.color;
    }
}
