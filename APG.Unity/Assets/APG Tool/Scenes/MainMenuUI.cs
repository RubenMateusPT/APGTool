using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{

    [SerializeField] private TMP_Text statusText;

    private NetworkManager _networkManager;

    public TMP_InputField codeField;

    private void Awake()
    {
        _networkManager = FindFirstObjectByType<NetworkManager>();
        _networkManager.OnStatusChange += UpdateStatusMessage;
    }

    private void UpdateStatusMessage(string msg)
    {
        statusText.text = msg;
    }

    public void CopyToClipboard()
    {
        if(string.IsNullOrEmpty(codeField.text))
            return;

        GUIUtility.systemCopyBuffer = $"{codeField.text}";
    }

    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
