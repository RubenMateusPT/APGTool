using System.Collections;
using System.Linq;
using APG.Common.Commands;
using APG.Common.Packets;
using APG.Common.Packets.Types;
using APG.Unity.Commands;
using APG.Unity.ScriptableObjects;
using APG.Unity.Utilities;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace APG.Unity.Managers
{
    public class APGManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        [InspectorName("Settings File")]
        private SettingsScriptableObject settings;

        [Header("UI")]
        [SerializeField] private GameObject discordUser;
        [SerializeField] private TMP_Text discordText;
        [SerializeField] private Image discordSprite;

        [Header("Commands")]
        [SerializeField] private SceneCommands[] sceneCommands;

        private NetworkManager _networkManager;

        private bool _isReady = false;

        private Coroutine _popUserCoroutine;

        

        private void Awake()
        {
            if (settings == null)
            {
                Debug.LogError("No Settings file provided!\nDisabling APG Manager");
                enabled = false;
                gameObject.SetActive(false);
            }

            _networkManager = FindFirstObjectByType<NetworkManager>();
            if(_networkManager != null)
                _networkManager.RegisterSceneManager(this);

           if(discordUser != null) 
               discordUser.SetActive(false);
        }

        private void Start()
        {
            _isReady = true;
        }


        public void ExecuteCommand(GameCommand command)
        {
            if (!_isReady)
                return;

            var execute = 
                sceneCommands.FirstOrDefault(c => c.commandName == command.Command.Name);

            if (execute == null)
                return;

            if (command.DiscordUser != null && discordUser != null) //Show User Pop Up if user is provided
            {
                discordText.text = $"{command.DiscordUser.Username} did {command.Command.Name}";
                discordSprite.sprite = ConvertByteImageToSprite(command.DiscordUser.ImageBytes);

                if (_popUserCoroutine != null)
                    StopCoroutine(_popUserCoroutine);
                _popUserCoroutine = StartCoroutine(PopUpUser());
            }

            if (command.Command.SendScreenshot)
                StartCoroutine(DelayScreenshot(command));

            //Use the correct Unity Event based on the command parameters given
            var parameters = command.Command.Parameters;
            if(parameters.Length == 0)
                execute.onReceive.Invoke();
            else if (parameters.Length == 1)
            {
                var param = parameters[0];
                switch (param.Type)
                {
                    case ParameterType.String:
                        execute.onReceiveString.Invoke(param.GetString());
                        break;

                    case ParameterType.Int:
                        execute.onReceiveInt.Invoke(param.GetInt());
                        break;

                    case ParameterType.Bool:
                        execute.onReceiveBool.Invoke(param.GetBool());
                        break;
                }
            }
            else if(parameters.Length >= 2)
                execute.onReceiveWithParameters.Invoke(new CommandParameters(parameters));
        }

        private IEnumerator DelayScreenshot(GameCommand command)
        {
            yield return new WaitForSeconds(command.Command.ScreenshotDelay);
            SendScreenShoot($"{command.DiscordUser.Username} did {command.Command.Name}");
        }

        private Sprite ConvertByteImageToSprite(byte[] bytes)
        {
            Texture2D tex = new Texture2D(1, 1);
            ImageConversion.LoadImage(tex, bytes);
            return Sprite.Create(tex, new Rect(0,0,tex.width,tex.height), new Vector2(tex.width / 2, tex.height / 2));
        }

        private IEnumerator PopUpUser()
        {
            discordUser.SetActive(true);
            yield return new WaitForSeconds(3);
            discordUser.SetActive(false);
        }

        public void SendRequest(string commandName)
        {
            SendRequest(commandName,string.Empty);
        }

        public void SendRequest(string commandName, string requiredValue)
        {
            var command = sceneCommands.FirstOrDefault(c => c.commandName == commandName);

            if (command == null)
            {
                Debug.LogError("Command not found. Is it correctly writen?");
                return;
            }

            var settingsCommand = settings.Commands.First(c => c.Name == command.commandName);

            if (!settingsCommand.IsRequestFromGame)
                return;

            if (_networkManager != null && _networkManager.IsApgEnabled) //Online
            {
                string message = settingsCommand.RequestMessage;
                if (!string.IsNullOrEmpty(requiredValue))
                    message += $"\nUse value: {requiredValue}";

                SendScreenShoot(message);
            }
            else //offline
            {
                StartCoroutine(DoOfflineRequest(command));
            }
        }

        private IEnumerator DoOfflineRequest(SceneCommands command)
        {
            yield return new WaitForEndOfFrame();
            command.onOffline.Invoke();
        }

        public void SendScreenShoot(string messageToSend)
        {
            if(_networkManager != null && _networkManager.IsApgEnabled)
                StartCoroutine(Screenshoot(messageToSend));
        }

        private IEnumerator Screenshoot(string messageToSend)
        {
            yield return new WaitForEndOfFrame();

            var screenshootTex = ScreenCapture.CaptureScreenshotAsTexture();

            TextureScale.Scale(screenshootTex, 480, 270);

            _networkManager.Send(new Screenshoot(messageToSend, screenshootTex.EncodeToPNG()));
        }
    }
}

