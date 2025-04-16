using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Body Info")]
    [SerializeField]
    private GameObject eyes;

    [Header("Movement Settings")]
    [SerializeField]
    private float movementSpeed;

    private Rigidbody2D _rigidbody;
    private float _currentDirection;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void RandomizeDirection()
    {
        _currentDirection = Random.Range(0, 2) == 0 ? -1 : 1;
        ChangeDirection();
    }

    public void RandomizeMovementSpeed()
    {
        movementSpeed *= Random.Range(0.75f, 2f); // 75% to 200% speed
    }

    private void Update()
    {
        CheckForGround();
        ChangeDirection();
    }

    private void CheckForGround()
    {
        var groundHit = Physics2D.Raycast(eyes.transform.position, Vector2.down, 1.5f, LayerMask.GetMask("Ground"));
        
        if(groundHit.transform == null)
            _currentDirection = _currentDirection == 1 ? -1 : 1;
    }

    private void ChangeDirection()
    {
        if(_currentDirection == transform.localScale.x)
            return;

        transform.localScale = new Vector3(_currentDirection, transform.localScale.y);
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        _rigidbody.linearVelocityX = transform.localScale.x * movementSpeed * Time.fixedDeltaTime;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            FindFirstObjectByType<PlayerManager>().KillPlayer();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if(eyes == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(eyes.transform.position, eyes.transform.position + Vector3.down * 1.5f);
    }
}
