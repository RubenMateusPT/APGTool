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
        List<string> options = settings.Commands.Select(c => c.Name).ToList();

        //Get Properties
        var commandIndexProperty = property.FindPropertyRelative("commandIndex");
        var commandNameProperty = property.FindPropertyRelative("commandName");
        if (string.IsNullOrEmpty(commandNameProperty.stringValue))
        {
            commandNameProperty.stringValue = options[commandIndexProperty.intValue];
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        var commandOnReceiveEvent = property.FindPropertyRelative("onReceive");
        var commandOnReceiveStringEvent = property.FindPropertyRelative("onReceiveString");
        var commandOnReceiveIntEvent = property.FindPropertyRelative("onReceiveInt");
        var commandOnReceiveBoolEvent = property.FindPropertyRelative("onReceiveBool");
        var commandOnReveiveWithParametersEvent = property.FindPropertyRelative("onReceiveWithParameters");

        var commandOnOffline = property.FindPropertyRelative("onOffline");

        // Visual Elements of UI
        var container = new VisualElement();
        var popup = new PopupWindow();
        var dropdown = new PopupField<string>("Name", options.ToList(), commandIndexProperty.intValue);
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
            popup.text = evt.newValue;
            commandNameProperty.stringValue = evt.newValue;
            commandIndexProperty.intValue = options.IndexOf(evt.newValue);

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

        return container;
    }

}
