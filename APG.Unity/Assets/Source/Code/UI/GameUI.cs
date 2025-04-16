using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField]
    private GameObject background;

    [SerializeField]
    private GameObject gameOver;
    [SerializeField]
    private GameObject retry;
    [SerializeField]
    private TextMeshProUGUI gameOverText;

    private void Start()
    {
        DisableGameObjects();
    }

    public void ShowGameOver(bool playerWon)
    {
        StopAllCoroutines();
        gameOverText.text = playerWon ? "YOU WIN!" : "YOU LOSE!";
        EnableGameObject();
    }

    private void EnableGameObject()
    {
        background.SetActive(true);
        gameOver.SetActive(true);
        retry.gameObject.SetActive(true);
    }

    private void DisableGameObjects()
    {
        background.SetActive(false);
        gameOver.SetActive(false);
        retry.gameObject.SetActive(false);
    }

    public void DisplayMessage(string msg)
    {
        gameOverText.text = msg;
        retry.gameObject.SetActive(false);
        gameOver.gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(AnimateMessage());
    }

    private IEnumerator AnimateMessage()
    {
        yield return new WaitForSeconds(2);
        DisableGameObjects();
    }
}
