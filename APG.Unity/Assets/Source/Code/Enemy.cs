using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    public Transform eyes;
    public float moveSpeed;

    private Rigidbody2D rb;

    private float dir;

    private bool isAlive = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dir = -1;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Z))
            Die();
    }

    private void FixedUpdate()
    {
        if(!isAlive)
            return;

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

    public void Die()
    {
        isAlive = false;

        GetComponent<Collider2D>().enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.AddForceX(Random.Range(-250,250));
        rb.AddForceY(Random.Range(250,500));
    }
}
