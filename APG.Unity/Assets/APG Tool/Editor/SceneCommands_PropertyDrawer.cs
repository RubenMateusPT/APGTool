using System.Collections.Generic;
using System.Linq;
using APG.Unity;
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

        //Get Propperties
        var commandIndexProperty = property.FindPropertyRelative("commandIndex");
        var commandNameProperty = property.FindPropertyRelative("commandName");
        if (string.IsNullOrEmpty(commandNameProperty.stringValue))
        {
            commandNameProperty.stringValue = options[commandIndexProperty.intValue];
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
        var commandOnReceiveEvent = property.FindPropertyRelative("onReceive");

        // Create a new VisualElement to be the root the property UI.
        var container = new VisualElement();

        var popup = new PopupWindow();
        popup.text = commandNameProperty.stringValue;

        //Command Name
        var dropdown = new PopupField<string>("Name", options.ToList(), commandIndexProperty.intValue);
        dropdown.RegisterValueChangedCallback((evt =>
        {
            popup.text = evt.newValue;
            commandNameProperty.stringValue = evt.newValue;
            commandIndexProperty.intValue = options.IndexOf(evt.newValue);
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }));
        popup.Add(dropdown);

        //Command Event
        var onReceiveField = new PropertyField(commandOnReceiveEvent);
        popup.Add(onReceiveField);

        container.Add(popup);

        return container;
    }
}
