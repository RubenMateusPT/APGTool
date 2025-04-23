using System;
using System.Collections;
using APG.Unity.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private GameUI _ui;

    private bool _hasEnded;

    private void Awake()
    {
        _ui = FindFirstObjectByType<GameUI>();
    }

    private void Start()
    {
        _hasEnded = false;
        Time.timeScale = 1.0f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene(0);

        if(_hasEnded && Input.GetKeyDown(KeyCode.R)) 
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void WinGame()
    {
        if (_hasEnded)
            return;

        _ui.ShowGameOver(true);
        GameOver();
    }

    public void LoseGame()
    {
        if (_hasEnded)
            return;

        _ui.ShowGameOver(false);
        FindFirstObjectByType<APGManager>().SendScreenShoot("Player has lost :(\nPerhaps you can help?");
        GameOver();
    }

    public void ShowUserMessage(string message)
    {
        if(_hasEnded)
            return;
        _ui.DisplayMessage(message);
    }

    private void GameOver()
    {
        Time.timeScale = 0.0f;
        _hasEnded = true;
    }
}
