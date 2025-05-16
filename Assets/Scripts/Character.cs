using UnityEngine;

public class Character : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Rigidbody2D rb;
    public BoxCollider2D bc;
    public Transform body;
    public Transform iris1;
    public Transform iris2;
    public Transform pupil1;
    public Transform pupil2;
    public Transform visual;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        body = transform.Find("Body");
        iris1 = body.Find("Iris1");
        iris2 = body.Find("Iris2");
        pupil1 = iris1.Find("Pupil1");
        pupil2 = iris2.Find("Pupil2");
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
        Vector3 bodyToMouse = (mouseWorldPosition - body.position).normalized;
        // Move the pupils towards the mouse within the iris, without rotating them
        float maxPupilOffset = 0.2f; // Adjust as needed for how far the pupil can move
        Vector3 iris1Center = body.position + new Vector3(-0.8f, 0.0f, 0.0f);
        Vector3 iris2Center = body.position + new Vector3(0.8f, 0.0f, 0.0f);

        float maxIrisOffset = 0.2f; // Adjust as needed for how far the iris can move
        // Move the iris towards the mouse within the body, without rotating them
        Vector3 iris1Offset = Vector3.ClampMagnitude(mouseWorldPosition - iris1Center, maxIrisOffset);
        Vector3 iris2Offset = Vector3.ClampMagnitude(mouseWorldPosition - iris2Center, maxIrisOffset);

        // Position iris directly with clamped offset
        iris1.position = new Vector3(iris1Center.x + iris1Offset.x, iris1Center.y + iris1Offset.y, iris1Center.z);
        iris2.position = new Vector3(iris2Center.x + iris2Offset.x, iris2Center.y + iris2Offset.y, iris2Center.z);

        Vector3 pupil1Offset = Vector3.ClampMagnitude(mouseWorldPosition - iris1.position, maxPupilOffset);
        Vector3 pupil2Offset = Vector3.ClampMagnitude(mouseWorldPosition - iris2.position, maxPupilOffset);

        pupil1.position = iris1.position + pupil1Offset;
        pupil2.position = iris2.position + pupil2Offset;
    }
}
