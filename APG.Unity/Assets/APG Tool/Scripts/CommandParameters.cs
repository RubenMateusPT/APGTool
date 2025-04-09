using System;
using System.Collections.Generic;
using APG.Common.Commands;
using UnityEditor.Build.Content;
using UnityEngine;

public class CommandParameters
{
    private Dictionary<string, CommandParameter> _parameters = new Dictionary<string, CommandParameter>();

    public CommandParameters(CommandParameter[] parameters)
    {
        foreach (var commandParameter in parameters)
        {
            _parameters.Add(commandParameter.Name.ToUpper(), commandParameter);
        }
    }

    public bool TryToGetStringValue(out string value, string parameterName)
    {
        value = null;
        if (!_parameters.ContainsKey(parameterName.ToUpper()))
            return false;

        value = _parameters[parameterName.ToUpper()].GetString();
        return true;
    }

    public bool TryToGetIntValue(out int value, string parameterName)
    {
        value = -1;
        if (!_parameters.ContainsKey(parameterName.ToUpper()))
            return false;

        value = _parameters[parameterName.ToUpper()].GetInt();
        return true;
    }

    public bool TryToGetBoolValue(out bool value, string parameterName)
    {
        value = false;
        if (!_parameters.ContainsKey(parameterName.ToUpper()))
            return false;

        value = _parameters[parameterName.ToUpper()].GetBool();
        return true;
    }
}
