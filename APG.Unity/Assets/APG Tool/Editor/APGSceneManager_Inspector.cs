using APG.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(APGManager))]
public class APGSceneManager_Inspector : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        // Create a new VisualElement to be the root of our Inspector UI.
        VisualElement myInspector = new VisualElement();

        var button = new Button(() =>
        {
            APGManager_Window.Open();
        });
        button.text = "Open APG Manager Window";

        myInspector.Add(button);

        // Return the finished Inspector UI.
        return myInspector;
    }
}
