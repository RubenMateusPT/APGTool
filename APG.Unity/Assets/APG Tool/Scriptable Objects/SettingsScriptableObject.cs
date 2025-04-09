using System;
using System.Linq;
using APG.Common.Commands;
using UnityEditor;
using UnityEngine;


namespace APG.Unity
{
    public class SettingsScriptableObject : ScriptableObject
    {
        public const string ASSET_PATH = "Assets/APG Tool/Settings.asset";

        [Header("Discord Settings")]
        [SerializeField] private string ip;
        public string IP => ip;
        [SerializeField] private int port;
        public int Port => port;

        [Header("Unity Settings")]
        [SerializeField] private string gameName;
        public string GameName => gameName;

        [Header("Commands Settings")]
        [SerializeField] private char commandDelimiter;
        public char CommandDelimiter => commandDelimiter;
        [SerializeField] private Command[] commands = Array.Empty<Command>();
        public Command[] Commands => commands;

        public void UseDefaultValues()
        {
            ip = "127.0.0.1";
            port = 8000;

            gameName = $"{Application.productName} - v{Application.version}";
            commandDelimiter = ';';
        }
    }

}

