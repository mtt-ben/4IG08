using UnityEngine;
using TMPro;

public class Reservoir : MonoBehaviour
{
    private BoxCollider2D detect;
    private int particleCount = 0;
    private TMP_Text label;
    private Simulation sim;
    private Transform water;
    private float initialWaterY;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find the child named "Block" and get its BoxCollider2D
        Transform detectChild = transform.Find("Detect");
        if (detectChild != null)
        {
            detect = detectChild.GetComponent<BoxCollider2D>();
            detect.isTrigger = true; // Ensure it's a trigger collider
        }
        label = GetComponentInChildren<TMP_Text>();
        UpdateLabel();
        sim = FindFirstObjectByType<Simulation>();
        water = transform.Find("Water");
        water.localScale = new Vector3(water.localScale.x, 0f, water.localScale.z);
        initialWaterY = water.position.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(detect.bounds.center, detect.bounds.size, 0f, LayerMask.GetMask("Water"));

        foreach (var hit in hits)
        {
            Particle particle = hit.GetComponent<Particle>();
            if (particle != null)
                particleCount++;
                sim.RemoveParticle(particle);
                Destroy(hit.gameObject);
        }
        UpdateLabel();
        UpdateWater();
    }

    void UpdateLabel()
    {
        if (label != null)
        {
            label.text = particleCount.ToString();
        }
    }

    void UpdateWater() {
        if (water != null)
        {
            water.position = new Vector3(water.position.x, Mathf.Min(initialWaterY + 0.75f, initialWaterY + 0.005f * particleCount), water.position.z);
            water.localScale = new Vector3(water.localScale.x, Mathf.Min(1.0f, 0.01f * particleCount), water.localScale.z);
        }
    }

    public bool getAbsorbed(int count)
    {
        if (particleCount <=0 )
        {
            return false;
        }
        particleCount -= count;
        UpdateLabel();
        UpdateWater();
        return true;
    }
}