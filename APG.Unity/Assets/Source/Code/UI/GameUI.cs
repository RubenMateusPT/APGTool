using System;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField]
    private GameObject background;

    [SerializeField]
    private GameObject gameOver;
    private GameObject retry;
    [SerializeField]
    private TextMeshProUGUI gameOverText;

    private void Start()
    {
        DisableGameObjects();
    }

    public void ShowGameOver(bool playerWon)
    {
        gameOverText.text = playerWon ? "YOU WIN!" : "YOU LOSE!";
        EnableGameObject();
    }

    private void EnableGameObject()
    {
        background.SetActive(true);
        gameOver.SetActive(true);
    }

    private void DisableGameObjects()
    {
        background.SetActive(false);
        gameOver.SetActive(false);
    }
}
