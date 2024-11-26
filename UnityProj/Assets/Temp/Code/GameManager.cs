using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI endText;
    private bool gameEnded;

    private void Awake()
    {
        Time.timeScale = 1;
        endText.gameObject.SetActive(false);
        gameEnded = false;
    }

    public void EndGame(bool won)
    {
        Time.timeScale = 0;

        if (won)
            endText.text = "YOU WIN!";
        else
            endText.text = "GAME OVER!";

        endText.gameObject.SetActive(true);

        gameEnded=true;
    }

    private void Update()
    {
        if(gameEnded && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
        }
    }
}
