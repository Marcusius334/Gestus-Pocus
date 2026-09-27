using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpSpeed = 5f;
    
    private Rigidbody2D rb;
    private float movement;
    private bool jump = false;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        //Devuelve -1 -> A, 1 -> D, 0 -> Nada
        movement = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            jump = true;
        }

    }

    void FixedUpdate()
    {
        if(jump){
            rb.linearVelocity = new Vector2(movement * speed, jumpSpeed);
            jump = false;
        }else{
            rb.linearVelocity = new Vector2(movement * speed, rb.linearVelocity.y);
        } 
    }

    public void SetGrounded(bool groundCheck){
        isGrounded = groundCheck;
    }
}
