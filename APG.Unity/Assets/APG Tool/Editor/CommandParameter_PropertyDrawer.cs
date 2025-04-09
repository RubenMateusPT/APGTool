using System;
using System.Linq;
using System.Text.RegularExpressions;
using APG.Common.Commands;
using APG.Unity;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(CommandParameter))]
public class CommandParameter_PropertyDrawer : PropertyDrawer
{
    private Regex regex = new Regex(@"\[[^\d]*(\d+)[^\d]*\]");

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        //Get Command ID
        var target = property.serializedObject.targetObject as SettingsScriptableObject;
        var propertyPath = property.propertyPath;
        var arrayValue = regex.Match(propertyPath).Groups.Last().Value;
        var id = target.Commands[int.Parse(arrayValue)].ID;

        //Get Properties
        var nameProp = property.FindPropertyRelative("Name");
        var typeProp = property.FindPropertyRelative("Type");
        var isRequiredProp = property.FindPropertyRelative("IsRequired");
        var defaultValueProp = property.FindPropertyRelative("DefaultValue");

        //Create UI
        var container = new VisualElement();
        var name = new PropertyField(nameProp);
        var type = new PropertyField(typeProp);
        var isRequired = new PropertyField(isRequiredProp);
        var defaultValue = new PropertyField(defaultValueProp);

        //Build UI
        container.Add(name);
        container.Add(type);
        container.Add(isRequired);
        container.Add(defaultValue);

        //Register Value Change Events
        isRequired.RegisterValueChangeCallback(e =>
        {
            var newValue = e.changedProperty.boolValue;
            defaultValue.style.display = !newValue
                ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex)
                : new StyleEnum<DisplayStyle>(DisplayStyle.None);
        });

        type.RegisterValueChangeCallback(e =>
        {
            var newValue = e.changedProperty.enumValueIndex;
            APGManager_Window.OnCommandParamTypeChange(id, (ParameterType)newValue);
        });

        return container;
    }
}
