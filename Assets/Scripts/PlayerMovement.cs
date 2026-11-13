using UnityEngine;

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

            sceneLoader.LoadScene("Win");
        }
    }
}
