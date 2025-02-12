using System;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class APGWindow : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;


    [MenuItem("Audience Participation Game Toolkit/APG Window")]
    public static void ShowExample()
    {
        APGWindow wnd = GetWindow<APGWindow>();
        wnd.titleContent = new GUIContent("APGWindow");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement wnd = m_VisualTreeAsset.Instantiate();
        root.Add(wnd);

        var currentActiveScene = SceneManager.GetActiveScene();

        APGScene apgScene = FindFirstObjectByType<APGScene>();

        if (apgScene == null)
        {
            if (EditorUtility.DisplayDialog("APG Scene Missing!", "Do you want to create one?", "Yes", "No"))
            {
                apgScene = new GameObject("APG Scene Settings", typeof(APGScene)).GetComponent<APGScene>();
            }
        }


        var apgSceneField = wnd.Q<ObjectField>();
        if (apgScene != null)
        {
            apgSceneField.value = apgScene;

            wnd.Q<Label>("APGLabel").text = ObjectNames.NicifyVariableName(apgScene.name);

            var insp = wnd.Q<VisualElement>("APGSceneInspector");
            var ser = new SerializedObject(apgScene);
            InspectorElement.FillDefaultInspector(insp,ser,null);
            insp.RemoveAt(0);

            foreach (var propertyField in insp.Query<PropertyField>().ToList())
            {
                propertyField.Bind(ser);
            }
        }
    }
}
