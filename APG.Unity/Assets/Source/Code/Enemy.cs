using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform eyes;
    public float moveSpeed;

    private Rigidbody2D rb;

    private float dir;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dir = -1;
    }

    private void FixedUpdate()
    {
        rb.linearVelocityX = dir * moveSpeed * Time.fixedDeltaTime;

        var hit = Physics2D.Raycast(eyes.position, Vector2.down, 2, LayerMask.GetMask("Ground"));
        if (!hit)
        {
            transform.localEulerAngles = new Vector3(0, dir == -1 ? 180 : 0, 0);
            dir = -dir;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(eyes.position, eyes.position + new Vector3(0, -2, 0));
    }
}
