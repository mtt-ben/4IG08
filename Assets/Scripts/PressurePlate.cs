using UnityEngine;
using TMPro;

public class PressurePlate : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int requiredParticles = 10;
    private TMP_Text label;
    private BoxCollider2D bc;
    private bool activated = false;
    private int animationCount = 10;

    void Start()
    {
        bc = GetComponent<BoxCollider2D>();
        label = GetComponentInChildren<TMP_Text>();
        UpdateLabel(0);
    }

    void FixedUpdate()
    {
        Vector2 center = (Vector2)transform.position + bc.offset + Vector2.up * 0.5f;
        Vector2 size = bc.size * transform.lossyScale;

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, LayerMask.GetMask("Water"));

        int particleCount = 0;
        foreach (var hit in hits)
        {
            if (hit.GetComponent<Particle>() != null)
                particleCount++;
        }
        if (!activated) {
            UpdateLabel(particleCount);
        }
        if (requiredParticles - particleCount <= 0)
        {
            activated = true;
        }
        if (activated && animationCount >0) {
            transform.position -= new Vector3(0,1,0).normalized * 0.02f;
            animationCount--;
        } 
        else if (animationCount <= 0) {
            GetComponent<SpriteRenderer>().color = Color.grey;
        }
    }

    void UpdateLabel(int count)
    {
        int remaining = Mathf.Max(0, requiredParticles - count);
        if (label != null)
        {
            label.text = "" + remaining;
        }
    }
}
