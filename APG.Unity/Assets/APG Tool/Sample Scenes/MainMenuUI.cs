using APG.Unity.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace APG.Unity.Sample.UI
{
    public class MainMenuUI : MonoBehaviour
    {

        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_InputField codeField;

        public void UpdateStatusMessage(string msg)
        {
            statusText.text = msg;
        }

        public void UpdateHostCode(string code)
        {
            codeField.text = code;
        }

        public void CopyToClipboard()
        {
            if (string.IsNullOrEmpty(codeField.text))
                return;

            GUIUtility.systemCopyBuffer = $"{codeField.text}";
        }

        public void StartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
