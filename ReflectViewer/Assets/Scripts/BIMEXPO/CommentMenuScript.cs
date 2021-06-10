using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.Reflect;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

public class CommentMenuScript : MonoBehaviour
{
    private Button validateButton;
    private Label surf;
    private GameObject target;
    private VisualElement m_Container;
    private CustomComment cc;

    void OnEnable()
    {
        //Register the action on button click
        target = GameObject.Find("Root").GetComponent<ChangeMaterial>().selectedObject;
        
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        validateButton = rootVisualElement.Q<Button>("ok-button");
        m_Container = rootVisualElement.Q<VisualElement>("Container");
        surf = rootVisualElement.Q<Label>("surface");

        surf.text = target.name;
        validateButton.RegisterCallback<ClickEvent>(ev => saveComment(target));

        // Recuperate the comment if one already exists
        var webScript = GameObject.Find("Root").GetComponent<Web>();
        string comment = webScript.GetComment(target);

        cc = new CustomComment(comment);
        m_Container.Add(cc);
    }

    void saveComment(GameObject target)
    {
        string comment = cc.textElem.value;
        var webScript = GameObject.Find("Root").GetComponent<Web>();
        webScript.saveComment(comment, target);
        GameObject.Find("CommentMenu").SetActive(false);
    }

}
