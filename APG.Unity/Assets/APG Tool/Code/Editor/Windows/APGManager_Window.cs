using System;
using System.Collections.Generic;
using APG.Common.Commands;
using APG.Unity.Managers;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace APG.Unity.Editor.Windows
{
    public class APGManager_Window : EditorWindow
    {
        public class SceneCommandEvents
        {
            public Func<Guid, string, bool> OnNameChange;
            public Func<Guid, bool, bool> OnIsRequiredChange;
            public Func<Guid, int, bool> OnParamsSizeChange;
            public Func<Guid, ParameterType, bool> OnParamTypeChange;
            public Func<bool> OnRemoved;
        }

        public static List<SceneCommandEvents> SceneCommands = new List<SceneCommandEvents>();

        public static void OnCommandNameChange(Guid id, string name) =>
            SceneCommands.ForEach(sc => sc.OnNameChange.Invoke(id, name));

        public static void OnCommandIsRequiredChange(Guid id, bool value) =>
            SceneCommands.ForEach(sc => sc.OnIsRequiredChange.Invoke(id, value));

        public static void OnCommandParamsChange(Guid id, int numberOfParams) =>
            SceneCommands.ForEach(sc => sc.OnParamsSizeChange.Invoke(id, numberOfParams));

        public static void OnCommandParamTypeChange(Guid id, ParameterType paramType) =>
            SceneCommands.ForEach(sc => sc.OnParamTypeChange.Invoke(id, paramType));

        private VisualElement _root;
        private APGManager _apgManager;


        [MenuItem("Audience Participation Game Framework/Scene Commands")]
        public static void Open()
        {
            SceneCommands.Clear();
            APGManager_Window wnd = GetWindow<APGManager_Window>();
            wnd.titleContent = new GUIContent($"\"{SceneManager.GetActiveScene().name}\" - Scene Commands");
        }

        public void CreateGUI()
        {
            _apgManager = GetAPGManager();

            _root = rootVisualElement;

            var apgManagerObject = new ObjectField("APG Scene Manager");
            apgManagerObject.allowSceneObjects = true;
            apgManagerObject.objectType = typeof(APGManager);
            apgManagerObject.RegisterValueChangedCallback((evt =>
            {
                _apgManager = evt.newValue as APGManager;
                if (evt.newValue == null)
                    ShowEmptyWindow();
                else
                    ShowAPGSceneManagerWindow();
            }));
            apgManagerObject.value = _apgManager;
            _root.Add(apgManagerObject);

            if (_apgManager == null)
                ShowEmptyWindow();
            else
                ShowAPGSceneManagerWindow();
        }

        private APGManager GetAPGManager()
        {
            APGManager[] apgManager =
                FindObjectsByType<APGManager>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            if (apgManager.Length == 0)
            {
                if (EditorUtility.DisplayDialog("APG Scene Manager Missing!",
                        "The APG Scene Manager is missing in the current scene, would you like to create one?", "Yes",
                        "No"))
                {
                    return new GameObject("APG Scene Manager", typeof(APGManager)).GetComponent<APGManager>();
                }
            }
            else if (apgManager.Length == 1)
            {
                return apgManager[0];
            }
            else
            {
                EditorUtility.DisplayDialog("Too many APG Scene Managers!",
                    "There can only be one manager active per scene!", "OK");
            }

            return null;
        }

        private void ShowEmptyWindow()
        {
            var objField = _root[0];
            _root.Clear();
            _root.Add(objField);

            var label = new Label("Please select or create an APG Scene Manager");
            _root.Add(label);

            var btn = new Button(() =>
            {
                new GameObject("APG Scene Manager", typeof(APGManager)).GetComponent<APGManager>();
                Close();
                APGManager_Window.Open();
            });
            btn.text = "Create APG Scene Manager";
            _root.Add(btn);
        }

        private void ShowAPGSceneManagerWindow()
        {
            var objField = _root[0];
            _root.Clear();
            _root.Add(objField);

            var serialized = new SerializedObject(_apgManager);
            var insp = new VisualElement();
            InspectorElement.FillDefaultInspector(insp, serialized, null);

            foreach (var propertyField in insp.Query<PropertyField>().ToList())
            {
                propertyField.Bind(serialized);
                propertyField.name = propertyField.name.Replace("PropertyField:", string.Empty);
            }

            var settings = Settings_Window.GetSettings();
            if (settings == null)
            {
                var createSettings = new Button(() =>
                {
                    var settings = Settings_Window.CreateSettings();
                    if (settings != null)
                    {
                        Close();
                        Open();
                    }
                });
                createSettings.text = "No settings file found!\nPlease create one first!";
                _root.Add(createSettings);
                return;
            }

            var settingsField = serialized.FindProperty("settings");
            if (settingsField.objectReferenceValue == null)
            {
                settingsField.objectReferenceValue = settings;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            var sceneCommands = insp.Q<PropertyField>("sceneCommands");

            if (settings != null && settings.Commands.Length <= 0)
            {
                insp.Remove(sceneCommands);
                var createCommands = new Button(() => { Settings_Window.Open(); });
                createCommands.text = "No commands available!\nPlease create some first";
                insp.Add(createCommands);
            }
            else
            {
                sceneCommands.RegisterValueChangeCallback(e =>
                {
                    SceneCommands.RemoveAll(sc => sc.OnRemoved.Invoke());
                });
            }

            _root.Add(insp);
        }

    }
}
