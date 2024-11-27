using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;

    public float moveSpeed;
    public float jumpForce;
    private bool canJump = true;

    private Vector3 startPos;

    public TextMeshProUGUI lifesText;
    public int lifes = 3;

    private GameManager gameManager;

    public GameObject doorrequestText;

    public void AddLife()
    {
        if (gameManager.gameEnded)
            return;

        lifes++;
        UpdateLifesText();
    }

    public void LoseLife()
    {
        lifes--;
        if(lifes > 0)
            transform.position = startPos;

        UpdateLifesText();
    }

    private void UpdateLifesText()
    {
        lifesText.text = $"x {lifes}";

        if(lifes <= 0)
            gameManager.EndGame(false);
    }


    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
    }


    private void Update()
    {

        if (Input.GetButtonDown("Jump") && canJump)
        {
            rb.AddForceY(jumpForce, ForceMode2D.Impulse);
            canJump = false;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocityX = Input.GetAxisRaw("Horizontal") * moveSpeed * Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer == 6)
        {
            canJump = true;
        }

        if(collision.gameObject.tag == "Enemy")
        {
            LoseLife();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            LoseLife();
        }

        if (collision.gameObject.tag == "DoorTrigger")
        {
            collision.gameObject.SetActive(false);
            doorrequestText.SetActive(true);
            NetworkManager.Instance.SendDoorRequest();
        }

        if (collision.gameObject.tag == "Goal")
        {
            gameManager.EndGame(true);
        }
    }
}
