using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.Reflect;
using System.IO;
using System.Reflection;
using System.Collections;

public class CommentMenuScript : MonoBehaviour
{
    private Button validateButton, screenshotButton;
    private TextField txtField;
    private GameObject target;

    void OnEnable()
    {
        //Register the action on button click
        target = GameObject.Find("Root").GetComponent<ChangeMaterial>().selectedObject;
        
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        validateButton = rootVisualElement.Q<Button>("ok-button");
        screenshotButton = rootVisualElement.Q<Button>("screenshot");
        txtField = rootVisualElement.Q<TextField>("txtField");
        txtField.label = target.name;
        validateButton.RegisterCallback<ClickEvent>(ev => saveComment(target));
        screenshotButton.RegisterCallback<ClickEvent>(ev => saveScreenshotWrapper(target));

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

    void saveScreenshotWrapper(GameObject surface)
    {
        StartCoroutine(saveScreenshot(surface));
    }

    IEnumerator saveScreenshot(GameObject surface)
    {
        // Get camera coordinates and orientation
        Transform cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
        Vector3 camPos = cam.position;
        Quaternion camRot = cam.rotation;

        // Define the name of the file
        string filename;
        var meta = surface.GetComponent<Metadata>();
        if (meta != null)
        {
            filename = meta.GetParameter("Id") + ".png";
        }
        else
        {
            throw new System.Exception("No Id attached to surface!");
        }

        // Make the screenshot
        string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string gitRootDir = Directory.GetParent(currentDir).Parent.Parent.FullName;
        string screenshotsDir = gitRootDir + "\\PHP\\screenshots\\";
        
        // Wait till the last possible moment before screen rendering to hide the UI
        yield return null;
        GameObject.Find("CommentMenu").GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("root-ve").style.display = DisplayStyle.None;
        GameObject.Find("TileChoiceMenu").GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("menu").style.display = DisplayStyle.None;

        // Wait for screen rendering to complete
        yield return new WaitForEndOfFrame();

        // Take screenshot
        ScreenCapture.CaptureScreenshot(screenshotsDir + filename);

        // Show UI after we're done
        GameObject.Find("CommentMenu").GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("root-ve").style.display = DisplayStyle.Flex;
        GameObject.Find("TileChoiceMenu").GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("menu").style.display = DisplayStyle.Flex;

        // Save it to DB
        var webScript = GameObject.Find("Root").GetComponent<Web>();
        webScript.saveScreenshot(filename, camPos, camRot, target);
    }
}
