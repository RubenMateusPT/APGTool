using System;
using System.Collections;
using System.Linq;
using APG.Common.Commands;
using APG.Common.Packets.Types;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace APG.Unity
{
    public class APGManager : MonoBehaviour
    {
        [SerializeField] private GameObject discordUser;
        [SerializeField] private TMP_Text discordText;
        [SerializeField] private Image discordSprite;

        [SerializeField] private SceneCommands[] sceneCommands;

        private NetworkManager _networkManager;

        private bool _isReady = false;

        private Coroutine _popUserCoroutine;

        private void Awake()
        {
            _networkManager = FindFirstObjectByType<NetworkManager>();
            _networkManager.RegisterSceneManager(this);

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

            if (command.DiscordUser != null)
            {
                discordText.text = $"{command.DiscordUser.Username} did {command.Command.Name}";
                discordSprite.sprite = ConvertByteImageToSprite(command.DiscordUser.ImageBytes);

                if(_popUserCoroutine != null)
                    StopCoroutine(_popUserCoroutine);
                _popUserCoroutine = StartCoroutine(PopUpUser());
            }

            execute.onReceive.Invoke();
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

        public void SendScreenShoot(string messageToSend)
        {
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

