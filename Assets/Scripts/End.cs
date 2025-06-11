using UnityEngine;
using TMPro;

public class End : MonoBehaviour
{
    public string nextScene = "MainMenu";
    public float particleThreshold;
    private BoxCollider2D bc;
    private float playerParticleCount = 0f;
    private TMP_Text label;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bc = GetComponent<BoxCollider2D>();
        Character character = FindFirstObjectByType<Character>();
        playerParticleCount = character.getParticleCount();
        label = GetComponentInChildren<TMP_Text>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameObject.FindWithTag("Player") && bc.bounds.Contains(GameObject.FindWithTag("Player").transform.position) && playerParticleCount >= particleThreshold)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
        UpdateParticleCount();
        UpdateLabel();
    }

    void UpdateLabel() {
        if (label != null)
        {
            label.text = playerParticleCount.ToString() + " / " + particleThreshold.ToString();
        }
        if (playerParticleCount >= particleThreshold)
        {
            label.color = Color.green;
        }
        else
        {
            label.color = Color.red;
        }
    }

    void UpdateParticleCount() {
        playerParticleCount = FindFirstObjectByType<Character>().getParticleCount();
    }
}
