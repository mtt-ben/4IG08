using UnityEngine;

public class End : MonoBehaviour
{
    public string nextScene = "MainMenu";
    private BoxCollider2D bc;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bc = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.FindWithTag("Player") && bc.bounds.Contains(GameObject.FindWithTag("Player").transform.position))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
    }
}
