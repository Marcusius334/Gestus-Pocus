using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement speed")]
    public float speed = 5f;
    public float jumpSpeed = 5f;
    
    private Rigidbody2D rb;
    private float movement;
    private bool jumping;
    private bool moving;
    private bool isGrounded;
    private bool animating;

    [Header("Managers")]
    public SceneLoader sceneLoader;
    public AudioManager audioManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jumping = false;
        moving = false;
    }

    void Update()
    {
        if(animating) return;

        //Devuelve -1 -> A, 1 -> D, 0 -> Nada
        movement = Input.GetAxisRaw("Horizontal");

        if(moving && movement == 0){
            audioManager.StopLoop();
            moving = false;
        }else if(!moving && movement != 0 && isGrounded){
            audioManager.PlayLoop(audioManager.soundMove);
            moving = true;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            audioManager.StopLoop();
            moving = false;
            audioManager.PlayOne(audioManager.soundJump);
            jumping = true;
        }

    }

    void FixedUpdate()
    {
        if(jumping){
            rb.linearVelocity = new Vector2(movement * speed, jumpSpeed);
            jumping = false;
        }else{
            rb.linearVelocity = new Vector2(movement * speed, rb.linearVelocity.y);
        } 
    }

    public void SetGrounded(bool groundCheck){
        isGrounded = groundCheck;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Win"))
        {
            if(GameManager.levelsUnlocked == GameManager.actualLevel){
                GameManager.UnlockLevel();
            }
            if(GameManager.actualLevel==4){
                sceneLoader.LoadScene("Complete");
            }else{
                sceneLoader.LoadScene("Win");
            }
            
        }
    }

    //-----------------------------------------
    //
    // ANIMATIONS
    //
    //-----------------------------------------

    public void Animate(int numberAnimaton, Vector3 endPosition){
        audioManager.StopLoop();
        animating = true;
        jumping = false;
        moving = false;

        switch(numberAnimaton){
            case 1:
                StartCoroutine(Animation1(endPosition));
                break;
            case 2:
                StartCoroutine(Animation2(endPosition));
                break;
            case 3:
                StartCoroutine(Animation3(endPosition));
                break;
            case 4:
                StartCoroutine(Animation4(endPosition));
                break;
            case 5:
                StartCoroutine(Animation5(endPosition));
                break;
            case 6:
                StartCoroutine(Animation6(endPosition));
                break;
            case 7:
                StartCoroutine(Animation7(endPosition));
                break;
            default:
                animating = false;
                Debug.Log("defaultAnimation");
                break;
        }
    }

    private IEnumerator Animation1(Vector3 endPosition)
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color originalColor = sprite.color;
        Vector3 originalScale = transform.localScale;

        Vector3 startPosition = transform.position;

        sprite.color = Color.black;
        transform.localScale = originalScale * 0.5f;

        float duration = 1f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        sprite.color = originalColor;
        transform.localScale = originalScale;
        animating = false;
    }

    private IEnumerator Animation2(Vector3 endPosition)
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color originalColor = sprite.color;
        Vector3 originalScale = transform.localScale;

        Vector3 startPosition = transform.position;

        sprite.color = Color.green;
        transform.localScale = originalScale * 2f;

        float duration = 1f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        sprite.color = originalColor;
        transform.localScale = originalScale;
        animating = false;
    }

    private IEnumerator Animation3(Vector3 endPosition)
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color originalColor = sprite.color;
        Vector3 originalScale = transform.localScale;

        Vector3 startPosition = transform.position;

        sprite.color = Color.lightBlue;
        transform.localScale = originalScale * 0.5f;

        float duration = 2f;
        float duration2 = 0.2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration2)
        {
            float t = elapsedTime / duration2;

            transform.position = new Vector3(
                transform.position.x,
                Mathf.Lerp(endPosition.y, endPosition.y - 0.5f, t),
                transform.position.z
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            transform.position = new Vector3(
                Mathf.Lerp(startPosition.x, endPosition.x, t),
                endPosition.y - 0.5f,
                transform.position.z
            );


            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < duration2)
        {
            float t = elapsedTime / duration2;

            transform.position = new Vector3(
                transform.position.x,
                Mathf.Lerp(endPosition.y - 0.5f, endPosition.y, t),
                transform.position.z
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        sprite.color = originalColor;
        transform.localScale = originalScale;
        animating = false;
    }

    private IEnumerator Animation4(Vector3 endPosition)
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color originalColor = sprite.color;
        Vector3 originalScale = transform.localScale;

        Vector3 startPosition = transform.position;

        sprite.color = Color.white;
        transform.localScale = originalScale * 0.5f;

        float duration = 1f;
        float duration2 = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < duration2)
        {
            float t = elapsedTime / duration2;

            transform.position = new Vector3(
                transform.position.x,
                Mathf.Lerp(startPosition.y, endPosition.y, t),
                transform.position.z
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            transform.position = new Vector3(
                Mathf.Lerp(transform.position.x, endPosition.x, t),
                endPosition.y,
                transform.position.z
            );


            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        sprite.color = originalColor;
        transform.localScale = originalScale;
        animating = false;
    }

    private IEnumerator Animation5(Vector3 endPosition)
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color originalColor = sprite.color;
        Vector3 originalScale = transform.localScale;

        Vector3 startPosition = transform.position;

        sprite.color = Color.red;
        transform.localScale = originalScale * 0.5f;
        transform.localScale = new Vector3(
                originalScale.x * 1.5f,
                originalScale.y * 0.75f,
                originalScale.z
            );

        float duration1 = 0.5f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration1)
        {
            float t = elapsedTime / duration1;

            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        sprite.color = originalColor;
        transform.localScale = originalScale;
        animating = false;
    }

    private IEnumerator Animation6(Vector3 endPosition)
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color originalColor = sprite.color;
        Vector3 originalScale = transform.localScale;

        Vector3 startPosition = transform.position;

        sprite.color = Color.yellow;
        transform.localScale = originalScale * 0.5f;
        transform.localScale = new Vector3(
                originalScale.x * 2f,
                originalScale.y * 1.25f,
                originalScale.z
            );

        float duration = 1f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        sprite.color = originalColor;
        transform.localScale = originalScale;
        animating = false;
    }

    private IEnumerator Animation7(Vector3 endPosition)
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color originalColor = sprite.color;
        Vector3 originalScale = transform.localScale;

        Vector3 startPosition = transform.position;

        sprite.color = Color.magenta;
        transform.localScale = new Vector3(
                originalScale.x * 1.25f,
                originalScale.y * 0.5f,
                originalScale.z
            );

        float duration = 1f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        sprite.color = originalColor;
        transform.localScale = originalScale;
        animating = false;
    }
}
