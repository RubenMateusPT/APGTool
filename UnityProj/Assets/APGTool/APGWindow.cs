using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class APGWindow : EditorWindow
{
    DatabaseContext db = null;
    List<TemplateContainer> commands = new List<TemplateContainer>();

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [SerializeField]
    private VisualTreeAsset commandTemplate = default;

    [MenuItem("Audience Participation Toolkit/APG")]
    public static void ShowExample()
    {
        APGWindow wnd = GetWindow<APGWindow>();
        wnd.titleContent = new GUIContent("Audience Participation Toolkit");
        wnd.saveChangesMessage = "Save your stuff";
        wnd.Show();
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        labelFromUXML.Q<Button>().clicked += () =>
        {
            hasUnsavedChanges = true;

            var newCommand = commandTemplate.Instantiate();

            var newCommandTittle = newCommand.Q<Foldout>();
            newCommand.Q<TextField>().RegisterValueChangedCallback((val) =>
            {
                newCommandTittle.text = val.newValue;
            });

            commands.Add(newCommand);
            labelFromUXML.Add(newCommand);
        };

        db = DatabaseContext.Instance;

        using (var con = db.Connection)
        {
            foreach (var command in con.Table<Command>())
            {
                var c = commandTemplate.Instantiate();
                c.Q<Foldout>().text = command.Name;
                c.Q<TextField>().value = command.Name;
                c.Q<IntegerField>().value = command.Cooldown;
                root.Add(c);
            }
        }
    }

    public override void SaveChanges()
    {
        //Save to DB
        using (var conn = db.Connection)
        {
            /*
            conn.InsertAll(
                commands.Select(
                    (c) =>
                    {
                        return new Command()
                        {
                            Name = c.Q<TextField>().value,
                            Cooldown = c.Q<IntegerField>().value,
                            NetworkTriggerObjectID = (c.Q<ObjectField>().value as EventTrigger).GetInstanceID()
                        };
                    }
                )
            );
            */

            var hi = commands.First().Q<ObjectField>().value as EventTrigger;
            Debug.Log(hi);
        }

        base.SaveChanges();
    }

}