using System;
using System.Linq;
using System.Text.RegularExpressions;
using APG.Common.Commands;
using APG.Unity;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEngine.UIElements.PopupWindow;

[CustomPropertyDrawer(typeof(Command))]
public class Command_PropertyDrawer : PropertyDrawer
{
    private Regex regex = new Regex(@"\[[^\d]*(\d+)[^\d]*\]");

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        //Get Command ID
        var target = property.serializedObject.targetObject as SettingsScriptableObject;
        var propertyPath = property.propertyPath;
        var arrayValue = regex.Match(propertyPath).Groups.Last().Value;
        var id = target.Commands[int.Parse(arrayValue)].ID;

        //Get properties
        var nameProp = property.FindPropertyRelative("Name");
        
        var isRequestFromGameProp = property.FindPropertyRelative("IsRequestFromGame");
        var requestMessageProp = property.FindPropertyRelative("RequestMessage");

        var hasCooldownProp = property.FindPropertyRelative("HasCooldown");
        var cooldownTimeProp = property.FindPropertyRelative("CooldownTime");

        var sendScreenshotProp = property.FindPropertyRelative("SendScreenshot");
        var screenshotDelayProp = property.FindPropertyRelative("ScreenshotDelay");

        var parametersProp = property.FindPropertyRelative("Parameters");

        //Create UI
        var container = new VisualElement();
        var name = new PropertyField(nameProp);

        var isRequestFromGame = new PropertyField(isRequestFromGameProp);
        var requestMessage = new PropertyField(requestMessageProp);

        var hasCooldown = new PropertyField(hasCooldownProp);
        var cooldownTime = new PropertyField(cooldownTimeProp);

        var sendScreenshot = new PropertyField(sendScreenshotProp);
        var screenshotDelay = new PropertyField(screenshotDelayProp);

        var parameters = new PropertyField(parametersProp);

        //Build UI
        container.Add(name);
        container.Add(isRequestFromGame);
        container.Add(requestMessage);
        container.Add(hasCooldown);
        container.Add(cooldownTime);
        container.Add(sendScreenshot);
        container.Add(screenshotDelay);
        container.Add(parameters);

        //Register Value Change Events
        name.RegisterValueChangeCallback(e =>
        {
            var newValue = e.changedProperty.stringValue;
            APGManager_Window.OnCommandNameChange(id,newValue);
        });

        isRequestFromGame.RegisterValueChangeCallback(e =>
        {
            var newValue = e.changedProperty.boolValue;
            requestMessage.style.display = newValue
                ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex)
                : new StyleEnum<DisplayStyle>(DisplayStyle.None);

            APGManager_Window.OnCommandIsRequiredChange(id,newValue);
        });

        hasCooldown.RegisterValueChangeCallback(e =>
        {
            var newValue = e.changedProperty.boolValue;
            cooldownTime.style.display = newValue
                ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex)
                : new StyleEnum<DisplayStyle>(DisplayStyle.None);
        });

        sendScreenshot.RegisterValueChangeCallback(e =>
        {
            var newValue = e.changedProperty.boolValue;
            screenshotDelay.style.display = newValue
                ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex)
                : new StyleEnum<DisplayStyle>(DisplayStyle.None);
        });

        parameters.RegisterValueChangeCallback(e =>
        {
            var newArraySize = e.changedProperty.arraySize;
            APGManager_Window.OnCommandParamsChange(id, newArraySize);
        });

        return container;
    }
}
