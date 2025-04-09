using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class SceneCommands
{
    // General
    [SerializeField] private int commandIndex;
    public string commandName;

    // Online
    public UnityEvent onReceive = new UnityEvent();
    public UnityEvent<string> onReceiveString = new UnityEvent<string>();
    public UnityEvent<int> onReceiveInt = new UnityEvent<int>();
    public UnityEvent<bool> onReceiveBool = new UnityEvent<bool>();
    public UnityEvent<CommandParameters> onReceiveWithParameters = new UnityEvent<CommandParameters>();


    // Offline


    public UnityEvent onOffline = new UnityEvent();
}
