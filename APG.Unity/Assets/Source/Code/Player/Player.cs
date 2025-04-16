using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] 
    private float movementSpeed;
    [SerializeField]
    private float jumpStrength;

    [Header("Feet Settings")]
    [SerializeField]
    private float feetPosition = 1;

    private Rigidbody2D _rigidbody;
    private CircleCollider2D _collider;

    private bool _inputEnabled;

    private bool _isGrounded;
    private bool _canDoubleJump;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CircleCollider2D>();
    }

    private void Update()
    {
        if (!_inputEnabled)
            return;

        GroundCheck();
        Jump();
    }

    private void FixedUpdate()
    {
        if(!_inputEnabled)
            return;

        Move();
        ClampMovement();
    }

    private void Move()
    {
        var xInput = Input.GetAxisRaw("Horizontal");
        _rigidbody.linearVelocityX = xInput * movementSpeed * Time.fixedDeltaTime;
    }

    private void GroundCheck()
    {
        var groundHit = Physics2D.Raycast(transform.position, Vector2.down, feetPosition, LayerMask.GetMask("Ground"));
        _isGrounded = groundHit.transform != null;

        if (!_canDoubleJump && _isGrounded)
            _canDoubleJump = true;
    }

    private void Jump()
    {
        if (!Input.GetButtonDown("Jump"))
            return;

        if (!_canDoubleJump && !_isGrounded)
            return;

        if(_canDoubleJump && !_isGrounded)
            _canDoubleJump = false;

        _rigidbody.AddForceY(jumpStrength, ForceMode2D.Impulse);
    }

    private void ClampMovement()
    {
        _rigidbody.linearVelocityX = Mathf.Clamp(_rigidbody.linearVelocityX, -movementSpeed,movementSpeed);
        _rigidbody.linearVelocityY = Mathf.Clamp(_rigidbody.linearVelocityY, -Mathf.Infinity, jumpStrength);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * feetPosition);
    }

    public void EnableControls() => _inputEnabled = true;
    public void DisableControls() => _inputEnabled = false;

    public void Spawn()
    {
        _collider.enabled = true;
    }

    public void Kill()
    {
        _collider.enabled = false;
        _rigidbody.linearVelocityY = 0.0f;
        _rigidbody.AddForce(
            new Vector2(
                (_rigidbody.linearVelocityX > 0 ? 1 : -1) * Random.Range(2, 4),
                20f),
            ForceMode2D.Impulse
        );
    }
}
