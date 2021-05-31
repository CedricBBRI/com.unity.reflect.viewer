using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Unity.Reflect.Viewer.UI;
using UnityEngine.Reflect;
//using UnityEngine.UI;

public class MenusHandler : MonoBehaviour
{
    public GameObject hitSurface = null;    // The surface clicked by the user.
    public UnityEvent m_MyEvent = new UnityEvent();
    private bool buildingLoaded = false;

    private void Start()
    {
       UIStateManager.stateChanged += UIStateManager_stateChanged; // Listening to UI state change in order to know when the building is loaded.
    }
    private void UIStateManager_stateChanged(UIStateData obj)
    {
        if (obj.progressData.totalCount > 0 && obj.progressData.currentProgress == obj.progressData.totalCount)    // Then the building is fully loaded
        {
            if (!buildingLoaded)
            {
                ShowButtons();
                //InputField strInput = new InputField();
                //FindAllObjects.FindAll(InputField);
                // TO DO : change FindAll argument to string, and pass "Wall" to it. Then fire it up from here, and remove the Toggle.
                buildingLoaded = true;
            }
        }
    }
    private void Update()
    {
        /*
        //Wait for the building to be loaded, then show the preselection button
        if (GameObject.Find("Root").transform.childCount > 0 && !preselectionButtonOn)
        {
            StartCoroutine(ShowButtons());
            preselectionButtonOn = true;
        }
        */

        bool preselectionDone = GameObject.Find("Root").GetComponent<Web>().preselectionDone;

        // Look out for comments input, only if the tile choice menu is already up
        var tcm = GameObject.Find("TileChoiceMenu");
        if (tcm != null && Input.GetMouseButtonDown(2) && m_MyEvent != null)
        {
            m_MyEvent.Invoke();
        }
        else if (tcm = null)
        {
            m_MyEvent.RemoveAllListeners();
        }
    }

    void ShowButtons()
    {
        GameObject preselectionUI = GameObject.Find("PreselectionMenu");
        var rootVisualElement = preselectionUI.GetComponent<UIDocument>().rootVisualElement;
        Button showHideMenu = rootVisualElement.Q<Button>("show-hide-menu");
        Button amendment = rootVisualElement.Q<Button>("produce-amendment");
        Button restore = rootVisualElement.Q<Button>("restore-previous");
        showHideMenu.style.display = DisplayStyle.Flex;
        amendment.style.display = DisplayStyle.Flex;
        restore.style.display = DisplayStyle.Flex;
    }

    /// <summary>
    /// Sets the Tile choice menu active, so that it appears on screen. This also freezes the player camera so that as long as this menu is up, moving the mouse doesn't change the perspective.
    /// </summary>
    public void ActivateTilesChoiceMenu()
    {
        GameObject[] allGO = Resources.FindObjectsOfTypeAll<GameObject>();
        GameObject tileChoiceMenu = null;
        foreach (GameObject go in allGO)
        {
            if (go.name == "TileChoiceMenu")
            {
                //Show menu
                go.SetActive(true);
                tileChoiceMenu = go;
                break;
            }
        }
        m_MyEvent.AddListener(ActivateCommentMenu);
    }

    void ActivateCommentMenu()
    {
        GameObject[] allGO = Resources.FindObjectsOfTypeAll<GameObject>();
        GameObject commentMenu = null;
        foreach (GameObject go in allGO)
        {
            if (go.name == "CommentMenu")
            {
                //Disable player camera rotation until the preselection is made
                //GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>().cameraCanMove = false;

                //Show menu
                go.SetActive(true);
                commentMenu = go;
                break;
            }
        }
    }
}
