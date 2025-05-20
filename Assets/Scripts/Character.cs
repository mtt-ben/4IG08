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
    public GameObject particleObject;
    private Simulation sim;
    private bool isPropulsing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        body = transform.Find("Body");
        iris1 = body.Find("Iris1");
        iris2 = body.Find("Iris2");
        pupil1 = iris1.Find("Pupil1");
        pupil2 = iris2.Find("Pupil2");
        sim = FindObjectOfType<Simulation>();
        isPropulsing = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.bodyType == RigidbodyType2D.Kinematic)
        {
            EyesTrackMouse();
            return;
        }

        bool isGrounded = bc.IsTouchingLayers(LayerMask.GetMask("Ground"));
    
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 20);
        }
        if (Input.GetKey(KeyCode.D) && !isPropulsing)
        {
            rb.linearVelocity = new Vector2(10, rb.linearVelocity.y);
        }
        else if (Input.GetKey(KeyCode.A) && !isPropulsing)
        {
            rb.linearVelocity = new Vector2(-10, rb.linearVelocity.y);
        }
        if ((Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.A))&& !isPropulsing)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }


        EyesTrackMouse();
        // Send water particles if mouse is pressed
        if (Input.GetMouseButton(0))
        {
            isPropulsing = true;
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            Vector2 direction = (mouseWorldPos - body.position).normalized;
            Vector3 spawnPos = body.position + (Vector3)direction;

            GameObject particleGO = Instantiate(particleObject, spawnPos, Quaternion.identity);
            Particle p = particleGO.GetComponent<Particle>();

            if (p != null)
            {
                // Add particle to simulation
                p.velocity = direction * 10f;
                p.inPlayer = true;
                sim.AddParticle(p);

                // Boost player in the opposite direction
                float boostStrength = 0.5f;
                rb.AddForce(-direction * boostStrength, ForceMode2D.Impulse);
            }
        }
        else {
            isPropulsing = false;
        }
    }

    void EyesTrackMouse()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0;

        Vector3 bodyToMouse = (mouseWorldPosition - body.position).normalized;
        float maxPupilOffset = 0.2f;
        Vector3 iris1Center = body.position + new Vector3(-0.8f, 0.0f, 0.0f);
        Vector3 iris2Center = body.position + new Vector3(0.8f, 0.0f, 0.0f);

        float maxIrisOffset = 0.2f;
        Vector3 iris1Offset = Vector3.ClampMagnitude(mouseWorldPosition - iris1Center, maxIrisOffset);
        Vector3 iris2Offset = Vector3.ClampMagnitude(mouseWorldPosition - iris2Center, maxIrisOffset);

        iris1.position = new Vector3(iris1Center.x + iris1Offset.x, iris1Center.y, iris1Center.z);
        iris2.position = new Vector3(iris2Center.x + iris2Offset.x, iris2Center.y, iris2Center.z);

        Vector3 pupil1Offset = Vector3.ClampMagnitude(mouseWorldPosition - iris1.position, maxPupilOffset);
        Vector3 pupil2Offset = Vector3.ClampMagnitude(mouseWorldPosition - iris2.position, maxPupilOffset);

        pupil1.position = iris1.position + pupil1Offset;
        pupil2.position = iris2.position + pupil2Offset;
    }
}
