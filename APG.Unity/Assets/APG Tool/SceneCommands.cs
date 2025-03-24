using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class SceneCommands
{
    [SerializeField] private int commandIndex;
    public string commandName;

    public UnityEvent onReceive = new UnityEvent();
}
