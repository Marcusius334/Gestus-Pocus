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
        SpriteRenderer otherSprite = gameObject.GetComponent<SpriteRenderer>();

        thisSprite.sprite = otherSprite.sprite;
        thisSprite.color = otherSprite.color;
    }
}
