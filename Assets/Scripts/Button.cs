using UnityEngine;

public class Button : MonoBehaviour
{
    public bool activated = false;
    public BoxCollider2D bc;
    public Vector2 direction;
    private int animationCount = 10;

    void Start() {
        bc = GetComponent<BoxCollider2D>();
    }

    void FixedUpdate() {
        RaycastHit2D hit = Physics2D.BoxCast(bc.bounds.center, bc.bounds.size, 0f, direction.normalized, 0.2f, LayerMask.GetMask("Water"));
        bool isHit = hit.collider != null;
        if (isHit && !activated) {
            Debug.Log("Button activated!");
            activated = true;
        }
        if (activated && animationCount >0) {
            transform.position -= (Vector3)direction.normalized * 0.02f;
            animationCount--;
        } 
        else if (animationCount <= 0) {
            GetComponent<SpriteRenderer>().color = Color.grey;
        }
    }
}