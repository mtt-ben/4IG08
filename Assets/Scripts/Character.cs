using UnityEngine;

public class Character : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Rigidbody2D rb;
    public BoxCollider2D bc;
    public CircleCollider2D absorbCollider;
    public Transform body;
    public Transform iris1;
    public Transform iris2;
    public Transform pupil1;
    public Transform pupil2;
    public Transform visual;
    public Transform particlesParent;
    public GameObject particleObject;
    public GameObject absorbVisual;
    private float particleCount = 300f;
    private float maxParticleCount = 800f;
    private float size = 1.0f;
    private Simulation sim;
    private bool isPropulsing = false;
    private Animator bodyAnimator;
    private Animator absorbAnimator;
    private bool isGrounded = false;
    private bool isTouchingWall = false;
    private bool isCrouching = false;
    private Vector2 shootDirection;
    private Vector3 spawnPos;
    private AudioSource audioSource;

    void Start()
    {
        Application.targetFrameRate = 60;
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        absorbCollider = GetComponent<CircleCollider2D>();
        body = transform.Find("Body");
        iris1 = body.Find("Iris1");
        iris2 = body.Find("Iris2");
        pupil1 = iris1.Find("Pupil1");
        pupil2 = iris2.Find("Pupil2");
        sim = FindFirstObjectByType<Simulation>();
        isPropulsing = false;
        absorbVisual.SetActive(false);
        bodyAnimator = body.GetComponent<Animator>();
        absorbAnimator = absorbVisual.GetComponent<Animator>();
        shootDirection = Vector2.zero;
        spawnPos = Vector3.zero;
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit2D hit = Physics2D.BoxCast(bc.bounds.center, bc.bounds.size, 0f, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
        isGrounded = hit.collider != null;
        isTouchingWall = Physics2D.Raycast(transform.position, Vector2.right * transform.localScale.x, 0.1f, LayerMask.GetMask("Ground"));
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        shootDirection = (mouseWorldPos - (Vector3)rb.position).normalized;
        spawnPos = rb.position + shootDirection * size;
        if (rb.bodyType != RigidbodyType2D.Kinematic)
        {
            handleMovements();
        }
        eyesTrackMouse();
        adaptSizeToParticleCount();
        if (transform.position.y < -20f)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }

    void FixedUpdate() {
        if (rb.bodyType != RigidbodyType2D.Kinematic)
        {
            handlePropulsion();
            handleAbsorption();
        }
    }

    void handleMovements() {
        if (isGrounded && !isPropulsing) {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isTouchingWall)
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
        if (Input.GetKey(KeyCode.S) && !isPropulsing && !isGrounded)
        {
            rb.AddForce(Vector2.down * 20f, ForceMode2D.Force);
            bodyAnimator.SetTrigger("PlayFall");
            bodyAnimator.ResetTrigger("StopFall");   
        }
        if (Input.GetKeyDown(KeyCode.S) && isGrounded)
        {
            bodyAnimator.SetTrigger("PlayCrouch");
            bodyAnimator.ResetTrigger("StopCrouch");
        }
        if (Input.GetKeyUp(KeyCode.S) || isGrounded)
        {
            bodyAnimator.SetTrigger("StopFall");
            bodyAnimator.ResetTrigger("PlayFall");
        }
        if (Input.GetKey(KeyCode.S) && isGrounded)
        {
            rb.linearVelocity = Vector2.zero;
            isCrouching = true;
            bodyAnimator.SetTrigger("PlayCrouch");
            bodyAnimator.ResetTrigger("StopCrouch");
        }
        if (Input.GetKeyUp(KeyCode.S) && isGrounded)
        {
            isCrouching = false;
            bodyAnimator.SetTrigger("StopCrouch");
            bodyAnimator.ResetTrigger("PlayCrouch");
        }
        if ((Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.A))&& !isPropulsing)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        if (Input.GetMouseButton(0)) {
            isPropulsing = true;
        }
        else if (Input.GetMouseButtonUp(0)) {
            isPropulsing = false;
        }
    }

    void handlePropulsion() {
        if (isPropulsing)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
                isPropulsing = false;
                return;
            }
            if (!audioSource.isPlaying)
            {
                audioSource.loop = true;
                audioSource.Play();
            }
            if (particleCount > 0) {
                GameObject particleGO = Instantiate(particleObject, spawnPos, Quaternion.identity, particlesParent);
                Particle p = particleGO.GetComponent<Particle>();

                if (p != null)
                {
                    // Add particle to simulation
                    // Add some random spread to the shoot direction (±2 degrees)
                    float angleOffset = Random.Range(-2f, 2f);
                    Vector2 randomizedDirection = Quaternion.Euler(0, 0, angleOffset) * shootDirection;
                    p.velocity = randomizedDirection.normalized * 24f;
                    sim.AddParticle(p);

                    // Boost player in the opposite direction
                    float boostStrength = 1.5f;
                    if (!isCrouching) {
                        rb.AddForce(-shootDirection * boostStrength, ForceMode2D.Impulse);
                    }
                    particleCount--;
                }
            }
        }
        else {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    void eyesTrackMouse()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0;

        Vector3 bodyToMouse = (mouseWorldPosition - body.position).normalized;
        float maxPupilOffset = 0.2f;
        Vector3 iris1Center = body.position + new Vector3(-0.8f, 0.0f, 0.0f) * size;
        Vector3 iris2Center = body.position + new Vector3(0.8f, 0.0f, 0.0f) * size;

        float maxIrisOffset = 0.2f;
        Vector3 iris1Offset = Vector3.ClampMagnitude(mouseWorldPosition - iris1Center, maxIrisOffset*size);
        Vector3 iris2Offset = Vector3.ClampMagnitude(mouseWorldPosition - iris2Center, maxIrisOffset*size);

        iris1.position = new Vector3(iris1Center.x + iris1Offset.x, iris1Center.y, iris1Center.z);
        iris2.position = new Vector3(iris2Center.x + iris2Offset.x, iris2Center.y, iris2Center.z);

        Vector3 pupil1Offset = Vector3.ClampMagnitude(mouseWorldPosition - iris1.position, maxPupilOffset*size);
        Vector3 pupil2Offset = Vector3.ClampMagnitude(mouseWorldPosition - iris2.position, maxPupilOffset*size);

        pupil1.position = iris1.position + pupil1Offset;
        pupil2.position = iris2.position + pupil2Offset;
    }

    void handleAbsorption() {
        if (Input.GetMouseButton(1) && !isPropulsing) {
            if (!absorbVisual.activeSelf) {
                absorbVisual.SetActive(true);
            }
            else {
                absorbAnimator.SetTrigger("Appear");
                absorbAnimator.ResetTrigger("Disappear");
            }
            absorbVisual.transform.Rotate(0, 0, 10 * Time.deltaTime);
            Vector2 center = (Vector2)absorbCollider.transform.position + absorbCollider.offset;
            float radius = absorbCollider.radius * absorbCollider.transform.lossyScale.x;
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, LayerMask.GetMask("Water","IgnoreCollision"));
            foreach (Collider2D hit in hits)
            {
                Particle particle = hit.GetComponent<Particle>();
                if (particle != null && particleCount < maxParticleCount)
                {
                    Debug.Log("Absorbing particle: " + particle.name);
                    sim.RemoveParticle(particle);
                    Destroy(hit.gameObject);
                    particleCount++;
                }
                Reservoir reservoir = hit.GetComponent<Reservoir>();
                if (reservoir != null && particleCount < maxParticleCount)
                {
                    bool absorbed = reservoir.getAbsorbed(1);
                    if (absorbed) {
                        particleCount++;
                        Debug.Log("Absorbing from reservoir");
                    }
                }
            }
        }
        else {
            absorbAnimator.SetTrigger("Disappear");
            absorbAnimator.ResetTrigger("Appear");            
        }
    }

    void adaptSizeToParticleCount() {
        float scaleFactor = Mathf.Clamp(particleCount/300f, 0.2f, 5.0f);
        size = scaleFactor;
        transform.localScale = new Vector3(0.35f * scaleFactor, 0.35f * scaleFactor, 1);
        rb.mass = scaleFactor*0.5f + 0.8f;
    }

    public float getParticleCount() {
        return particleCount;
    }
}
