using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Unity.Reflect.Viewer.UI;
using UnityEngine.Reflect;

public class MenusHandler : MonoBehaviour
{
    public GameObject hitSurface = null;    // The surface clicked by the user.
    public UnityEvent m_MyEvent = new UnityEvent();
    private bool buildingLoaded = false;
    private List<string> busyIds = new List<string>();

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

        // Look for surfaces that are not tiled by default (not included in price)
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            Metadata md = hit.collider.gameObject.GetComponent<Metadata>();
            if (md != null)
            {
                string id = md.GetParameter("Id");
                if (md.GetParameter("Type").Contains("Plafonnage") && !busyIds.Contains(id))
                {
                    StartCoroutine(ShowNonIncludedInfo(hit, id));
                }
            }
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

    /// <summary>
    /// Method to show to the user the surfaces for which tiles are not included in the price.
    /// The surface is colored red, then progressively returns to its original color.
    /// </summary>
    IEnumerator ShowNonIncludedInfo(RaycastHit hit, string id)
    {
        busyIds.Add(id);
        Material mat = hit.collider.gameObject.GetComponent<Renderer>().material;
        if (mat.HasProperty("_Tint"))
        {
            Color initColor = mat.GetColor("_Tint");    // This will fail if the shader changes
            yield return null;

            float interp = 0.0f;

            while (interp < 1.0f)
            {
                mat.SetColor("_Tint", Color.Lerp(Color.red, initColor, interp));
                interp += 0.025f;
                yield return new WaitForSeconds(0.03f);
            }

            mat.SetColor("_Tint", Color.Lerp(Color.red, initColor, interp));
            busyIds.Remove(id);
            yield return null;
        }
        else
        {
            yield return null;
        }
    }
}
