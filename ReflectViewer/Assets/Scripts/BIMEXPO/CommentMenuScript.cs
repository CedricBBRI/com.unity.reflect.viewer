using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.Reflect;
using System.IO;
using System.Reflection;
using System.Collections;

public class CommentMenuScript : MonoBehaviour
{
    private Button validateButton;
    private TextField txtField;
    private GameObject target;

    void OnEnable()
    {
        //Register the action on button click
        target = GameObject.Find("Root").GetComponent<ChangeMaterial>().selectedObject;
        
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        validateButton = rootVisualElement.Q<Button>("ok-button");
        //screenshotButton = rootVisualElement.Q<Button>("screenshot");
        txtField = rootVisualElement.Q<TextField>("txtField");
        txtField.label = target.name;
        validateButton.RegisterCallback<ClickEvent>(ev => saveComment(target));
        //screenshotButton.RegisterCallback<ClickEvent>(ev => saveScreenshotWrapper(target));

        // Recuperate the comment if one already exists
        var webScript = GameObject.Find("Root").GetComponent<Web>();
        string comment = webScript.GetComment(target);
        //txtField.text = comment;  // Problem: text is not settable.. Idea for later: create custom textField..
    }

    void saveComment(GameObject target)
    {
        string comment = txtField.text;
        var webScript = GameObject.Find("Root").GetComponent<Web>();
        webScript.saveComment(comment, target);
        GameObject.Find("CommentMenu").SetActive(false);
    }

}
