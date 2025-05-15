using UnityEngine;

public class Character : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Rigidbody2D rb;
    public BoxCollider2D bc;
    public Transform visual;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        visual = transform.Find("Visual");
    }

    // Update is called once per frame
    void Update()
    {
        bool isGrounded = bc.IsTouchingLayers(LayerMask.GetMask("Ground"));
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 20);
        }
        if (Input.GetKey(KeyCode.D))
        {
            rb.linearVelocity = new Vector2(10, rb.linearVelocity.y);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            rb.linearVelocity = new Vector2(-10, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        // Get mouse position in world space
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0;
        
        // Calculate the direction from the character to the mouse position
        Vector3 direction = (mouseWorldPosition - transform.position).normalized;
        // Calculate the angle in degrees
        float angle;
        if (direction.x < 0)
        {
            angle = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
            // Flip the visual to face the mouse position
            visual.localScale = new Vector3(-1, 1, 1);
        }
        else {
            angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // Reset the visual scale to face the mouse position
            visual.localScale = new Vector3(1, 1, 1);
        }
        // Rotate the character to face the mouse position
        visual.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
}
