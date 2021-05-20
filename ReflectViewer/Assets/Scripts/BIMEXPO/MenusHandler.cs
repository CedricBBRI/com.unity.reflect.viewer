using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class MenusHandler : MonoBehaviour
{
    public GameObject hitSurface = null;    // The surface clicked by the user.
    UnityEvent m_MyEvent = new UnityEvent();
    private bool preselectionButtonOn = false;

    private void Update()
    {
        //Wait for the building to be loaded, then show the preselection button
        if (GameObject.Find("Root").transform.childCount > 0 && !preselectionButtonOn)
        {
            StartCoroutine(ShowButtons());
            preselectionButtonOn = true;
        }

        bool preselectionDone = GameObject.Find("Root").GetComponent<Web>().preselectionDone;
        // Watch out for user click in order to assign a material to a surface

        // ---------------------------------------------------------------------------------------------------------------------------
        // 12/05/2021 - AC: This code will be merged with Cedric's. I comment thus this part that is supposed to bring the tiles menu.
        // ---------------------------------------------------------------------------------------------------------------------------

        /*
        if (Input.GetMouseButtonDown(1) && preselectionDone)
        {
            RaycastHit hit; // Infos about the hit

            //Filter walls and slabs only
            int layerMask = LayerMask.GetMask("Default");

            //Shoot the ray towards the mouse position
            Ray rayToMouse = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(rayToMouse, out hit, Mathf.Infinity, layerMask))
            {
                hitSurface = hit.collider.gameObject;
                //HighlightSurface(hitSurface);
                ActivateTilesChoiceMenu();

                //Adding event listener so that if user clicks middle button when surface is selected, comment box appears
                //m_MyEvent.AddListener(ActivateCommentMenu);
            }
            else
            {
                Debug.Log("No hit");
                m_MyEvent.RemoveAllListeners();
            }
        }
        else if (Input.GetMouseButtonDown(1) && !preselectionDone)
        {
            Debug.LogWarning("La présélection des carrelages n'a pas encore été faite!");
        }
        if (Input.GetMouseButtonDown(2) && m_MyEvent != null)
        {
            //Begin the action
            m_MyEvent.Invoke();
        }
        */
    }

    IEnumerator ShowButtons()
    {
        yield return new WaitForSeconds(5);

        GameObject preselectionUI = GameObject.Find("PreselectionMenu");
        var rootVisualElement = preselectionUI.GetComponent<UIDocument>().rootVisualElement;
        Button showHideMenu = rootVisualElement.Q<Button>("show-hide-menu");
        Button amendment = rootVisualElement.Q<Button>("produce-amendment");
        showHideMenu.style.display = DisplayStyle.Flex;
        amendment.style.display = DisplayStyle.Flex;
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
    }
    /*

    void HighlightSurface(GameObject surf)
    {
        //surf.GetComponent<Material>().SetColor("_Color", Color.red);
        surf.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
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
                GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>().cameraCanMove = false;

                //Show menu
                go.SetActive(true);
                commentMenu = go;
                break;
            }
        }
    }
    */
}
