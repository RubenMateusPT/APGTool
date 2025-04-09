using System;
using System.Collections.Generic;
using System.Linq;
using APG.Common.Commands;
using APG.Unity;
using TMPro;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEngine.UIElements.PopupWindow;

[CustomPropertyDrawer(typeof(SceneCommands))]
public class SceneCommands_PropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        //Get available commands
        var settings = AssetDatabase.LoadAssetAtPath<SettingsScriptableObject>(SettingsScriptableObject.ASSET_PATH);
        Dictionary<Guid, string> options = new Dictionary<Guid, string>();
        foreach (var settingsCommand in settings.Commands)
        {
            options.Add(settingsCommand.ID,settingsCommand.Name);
        }

        //Get Properties
        var commandIndexProperty = property.FindPropertyRelative("commandIndex");
        var commandNameProperty = property.FindPropertyRelative("commandName");

        if(commandIndexProperty.intValue < 0)
            commandIndexProperty.intValue = 0;
        else if(commandIndexProperty.intValue >= options.Values.Count)
            commandIndexProperty.intValue = options.Values.Count - 1;
        
        commandNameProperty.stringValue = options.Values.ElementAt(commandIndexProperty.intValue);

        var commandOnReceiveEvent = property.FindPropertyRelative("onReceive");
        var commandOnReceiveStringEvent = property.FindPropertyRelative("onReceiveString");
        var commandOnReceiveIntEvent = property.FindPropertyRelative("onReceiveInt");
        var commandOnReceiveBoolEvent = property.FindPropertyRelative("onReceiveBool");
        var commandOnReveiveWithParametersEvent = property.FindPropertyRelative("onReceiveWithParameters");

        var commandOnOffline = property.FindPropertyRelative("onOffline");

        // Visual Elements of UI
        var container = new VisualElement();
        var popup = new PopupWindow();
        var guids = options.Keys.ToList();


        var dropdown = new PopupField<Guid>(
            "Name",
            options.Keys.ToList(),
            commandIndexProperty.intValue,
            guid => options[guid],
            guid => $"{options[guid]} ({guids.IndexOf(guid) + 1})"
        );

        var onReceiveField = new PropertyField(commandOnReceiveEvent);
        var onReceiveStringField = new PropertyField(commandOnReceiveStringEvent);
        var onReceiveIntField = new PropertyField(commandOnReceiveIntEvent);
        var onReceiveBoolField = new PropertyField(commandOnReceiveBoolEvent);
        var onReceiveWithParametersEvent = new PropertyField(commandOnReveiveWithParametersEvent);

        var onOffline = new PropertyField(commandOnOffline);

        // Stat of UI Build
        popup.text = commandNameProperty.stringValue;

        //Command Name Dropdown
        dropdown.RegisterValueChangedCallback((evt =>
        {
            var guid = evt.newValue;
            popup.text = options[guid];
            commandNameProperty.stringValue = options[guid];
            commandIndexProperty.intValue = guids.IndexOf(guid);

            onOffline.style.display = settings.Commands[commandIndexProperty.intValue].IsRequestFromGame
                ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex)
                : new StyleEnum<DisplayStyle>(DisplayStyle.None);
            var parameters = settings.Commands[commandIndexProperty.intValue].Parameters;
            onReceiveField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            onReceiveStringField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            onReceiveIntField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            onReceiveBoolField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            onReceiveWithParametersEvent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            if (parameters.Length == 0) //No parameters, simple command
                onReceiveField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            else if (parameters.Length == 1) //One parameter, simple command
            {
                var param = parameters[0];
                switch (param.Type)
                {
                    case ParameterType.String:
                        onReceiveStringField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                        break;

                    case ParameterType.Int:
                        onReceiveIntField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                        break;

                    case ParameterType.Bool:
                        onReceiveBoolField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                        break;
                }
            }
            else if (parameters.Length >= 2) // +Two parameters, complex command
                onReceiveWithParametersEvent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);


            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }));


        dropdown.style.marginBottom = new StyleLength(10);
        popup.Add(dropdown);


        //Command Event
        popup.Add(onReceiveField);
        popup.Add(onReceiveStringField);
        popup.Add(onReceiveIntField);
        popup.Add(onReceiveBoolField);
        popup.Add(onReceiveWithParametersEvent);

        popup.Add(onOffline);

        // End building UI for container
        container.Add(popup);


        //Show correct event
        onOffline.style.display = settings.Commands[commandIndexProperty.intValue].IsRequestFromGame
            ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex)
            : new StyleEnum<DisplayStyle>(DisplayStyle.None);
        var parameters = settings.Commands[commandIndexProperty.intValue].Parameters;
        int paramSize = parameters.Length;
        onReceiveField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        onReceiveStringField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        onReceiveIntField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        onReceiveBoolField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        onReceiveWithParametersEvent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

        if (paramSize == 0) //No parameters, simple command
            onReceiveField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        else if (paramSize == 1) //One parameter, simple command
        {
            var param = parameters[0];
            switch (param.Type)
            {
                case ParameterType.String:
                    onReceiveStringField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                    break;

                case ParameterType.Int:
                    onReceiveIntField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                    break;

                case ParameterType.Bool:
                    onReceiveBoolField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                    break;
            }
        }
        else if (paramSize >= 2) // +Two parameters, complex command
            onReceiveWithParametersEvent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);

        property.serializedObject.ApplyModifiedPropertiesWithoutUndo();

        APGManager_Window.SceneCommands.Add(new APGManager_Window.SceneCommandEvents
        {
            OnNameChange = (guid, name) =>
            {
                options[guid] = name;

                dropdown.formatSelectedValueCallback = g => options[g];
                dropdown.formatListItemCallback = g => options[g];

                if (guid == dropdown.value)
                {
                    popup.text = name;
                    commandNameProperty.stringValue = name;
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }

                return true;
            },
            OnIsRequiredChange = (guid, value)  =>
            {
                if (guid == dropdown.value)
                {
                    onOffline.style.display =
                        value
                            ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex)
                            : new StyleEnum<DisplayStyle>(DisplayStyle.None);
                }
                return true;
            },
            OnParamsSizeChange = (guid, size) =>
            {
                if (guid == dropdown.value)
                {
                    paramSize = size;

                    onReceiveField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    onReceiveStringField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    onReceiveIntField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    onReceiveBoolField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    onReceiveWithParametersEvent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

                    if (size == 0) //No parameters, simple command
                        onReceiveField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                    else if (size == 1) //One parameter, simple command
                    {
                        var param = ParameterType.String;
                        switch (param)
                        {
                            case ParameterType.String:
                                onReceiveStringField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                                break;

                            case ParameterType.Int:
                                onReceiveIntField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                                break;

                            case ParameterType.Bool:
                                onReceiveBoolField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                                break;
                        }
                    }
                    else if (size >= 2) // +Two parameters, complex command
                        onReceiveWithParametersEvent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                }
                return true;
            },
            OnParamTypeChange = (guid, paramType) =>
            {
                if (guid == dropdown.value)
                {
                    if (paramSize == 1)
                    {
                        onReceiveStringField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                        onReceiveIntField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                        onReceiveBoolField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

                        switch (paramType)
                        {
                            case ParameterType.String:
                                onReceiveStringField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                                break;

                            case ParameterType.Int:
                                onReceiveIntField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                                break;

                            case ParameterType.Bool:
                                onReceiveBoolField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                                break;
                        }
                    }
                }
                return true;
            },
            OnRemoved = () =>
            {
                try
                {
                    var temp = commandNameProperty.stringValue;
                    commandNameProperty.stringValue = string.Empty;
                    commandNameProperty.stringValue = temp;
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    return false;
                }
                catch (Exception e)
                {
                    return true;
                }
                return true;
            }
        });


        return container;
    }
}
