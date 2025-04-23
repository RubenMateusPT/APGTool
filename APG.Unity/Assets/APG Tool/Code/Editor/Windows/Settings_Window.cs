using APG.Unity.ScriptableObjects;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace APG.Unity.Editor.Windows
{
    public class Settings_Window : EditorWindow
    {
        [MenuItem("Audience Participation Game Framework/Settings")]
        public static void Open()
        {
            Settings_Window wnd = GetWindow<Settings_Window>();
            wnd.titleContent = new GUIContent("APG Framework Settings");
        }


        public void CreateGUI()
        {
            var settings = GetSettings();

            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            if (settings == null)
                CreateEmptyWindow(root);
            else
                CreateSettingsWindow(settings, root);
        }

        public static SettingsScriptableObject GetSettings()
        {
            SettingsScriptableObject settings = null;

#if UNITY_6000_0_OR_NEWER
            bool settingsExist = AssetDatabase.AssetPathExists(SettingsScriptableObject.ASSET_PATH);
#else
            bool settingsExist = !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(SettingsScriptableObject.ASSET_PATH));
#endif

            if (!settingsExist)
            {
                if (EditorUtility.DisplayDialog("No Settings Found!", "Do you want to create a new one?", "Yes", "No"))
                {
                    settings = CreateSettings();
                }
            }
            else
            {
                settings = AssetDatabase.LoadAssetAtPath<SettingsScriptableObject>(SettingsScriptableObject.ASSET_PATH);
            }

            return settings;
        }

        public static SettingsScriptableObject CreateSettings()
        {
            var settings = ScriptableObject.CreateInstance<SettingsScriptableObject>();
            AssetDatabase.CreateAsset(settings, SettingsScriptableObject.ASSET_PATH);

            if (EditorUtility.DisplayDialog("Default Values?", "Would you like to use the default values?", "Yes", "No"))
            {
                settings.UseDefaultValues();
            }

            return settings;
        }

        private void CreateEmptyWindow(VisualElement root)
        {
            var label = new Label("No Settings File Found!");
            root.Add(label);

            var createButton = new Button(() =>
            {
                CreateSettings();
                Close();
                Settings_Window.Open();
            });
            createButton.text = "Create Settings";
            root.Add(createButton);
        }

        private void CreateSettingsWindow(SettingsScriptableObject settings, VisualElement root)
        {
            var serialized = new SerializedObject(settings);
            if (string.IsNullOrEmpty(serialized.FindProperty("gameName").stringValue))
                serialized.FindProperty("gameName").stringValue = $"{Application.productName} - v{Application.version}";
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var insp = new VisualElement();
            InspectorElement.FillDefaultInspector(insp,serialized,null);

            foreach (var propertyField in insp.Query<PropertyField>().ToList())
            {
                propertyField.Bind(serialized);
                propertyField.name = propertyField.name.Replace("PropertyField:", string.Empty);
            }

            var commands = insp.Q<PropertyField>("commands");
            commands.RegisterValueChangeCallback(e =>
            {
                if (EditorWindow.HasOpenInstances<APGManager_Window>()) //Recreate APG Scene Commands Window
                {
                    var window = GetWindow<APGManager_Window>();
                    window.Close();
                    APGManager_Window.Open();
                }
            });

            root.Add(insp);
        }
    }
}


