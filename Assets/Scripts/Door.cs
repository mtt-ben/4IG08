using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject button;
    public int animationCount = 100;
    private Button buttonScript;
    void Start()
    {
        if (button != null)
        {
            buttonScript = button.GetComponent<Button>();
        }
    }
    
    void FixedUpdate() 
    {
        if (buttonScript != null && buttonScript.activated && animationCount > 0)
        {
            transform.position += Vector3.up * 0.02f;
            animationCount--;
        }
    }
}
