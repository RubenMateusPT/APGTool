using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class APGScene : MonoBehaviour
{
    [Serializable]
    public class APGCommand
    {
        public string name;
        public int cooldown;
        public float cooldownTimer = 0;
        public UnityEvent OnTrigger = new UnityEvent();
    }


    public APGCommand[] commands;

    public bool RunCommand(string command)
    {
        APGCommand com = null;

        foreach (var apgCommand in commands)
        {
            if (apgCommand.name.ToLower() == command.ToLower())
            {
                com = apgCommand;
                break;
            }
        }

        if (com == null)
            return false;

        if (com.cooldownTimer > 0)
            return false;

        com.OnTrigger.Invoke();
        com.cooldownTimer = com.cooldown;

        return true;
    }

    private void Update()
    {
        foreach (var apgCommand in commands)
        {
            if (apgCommand.cooldownTimer > 0)
            {
                apgCommand.cooldownTimer -= Time.deltaTime;
            }
        }
    }
}