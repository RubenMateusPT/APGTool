using System.Linq;
using APG.Unity;
using PlasticPipe.Certificates;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class APGManager_Window : EditorWindow
{
    private VisualElement _root;
    private APGManager _apgManager;


    [MenuItem("Audience Participation Game Framework/Scene Commands")]
    public static void Open()
    {
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
            if(evt.newValue == null)
                ShowEmptyWindow();
            else
                ShowAPGSceneManagerWindow();
        }));
        apgManagerObject.value = _apgManager;
        _root.Add(apgManagerObject);

        if(_apgManager == null)
            ShowEmptyWindow();
        else
            ShowAPGSceneManagerWindow();
    }

    private APGManager GetAPGManager()
    {
        APGManager[] apgManager = FindObjectsByType<APGManager>(FindObjectsInactive.Exclude,FindObjectsSortMode.None);

        if (apgManager.Length == 0)
        {
            if (EditorUtility.DisplayDialog("APG Scene Manager Missing!",
                    "The APG Scene Manager is missing in the current scene, would you like to create one?", "Yes", "No"))
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
        }

        _root.Add(insp);
    }
}
