using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.ObjectModel;

public class PreselectionMenuScript : MonoBehaviour
{
    private VisualElement myBox, mainMenu;
    private Button okButton, showHideMenuButton;
    private List<string> localSelectedTiles = new List<string>();
    public ReadOnlyCollection<string> selectedTiles { get { return localSelectedTiles.AsReadOnly(); } } // selectedTiles can be read but not modified outside this class
    private Toggle wallToggle, slabToggle;
    //private ReadOnlyCollection<string> tileNames, wallTileNames, slabTileNames;
    private List<string> tileNames, wallTileNames, slabTileNames;
    private string allTileNames;

    void OnEnable()
    {
        //Disable player camera rotation until the preselection is made
        //GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>() = false;

        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        myBox = rootVisualElement.Q<VisualElement>("img-container");
        okButton = rootVisualElement.Q<Button>("ok-button");
        wallToggle = rootVisualElement.Q<Toggle>("wallToggle");
        slabToggle = rootVisualElement.Q<Toggle>("slabToggle");
        showHideMenuButton = rootVisualElement.Q<Button>("show-hide-menu");
        mainMenu = rootVisualElement.Q<VisualElement>("main-menu");

        showHideMenuButton.clicked += ShowHidePreselectionMenu;

        var DBScript = GameObject.Find("Root").GetComponent<DBInteractions>();
        

        StartCoroutine(getAllTilesNames());         //Finds all tiles in DB and store them in tileNames
        StartCoroutine(getAllTilesNames("walls"));  //Finds all tiles in DB and store them in wallTileNames
        StartCoroutine(getAllTilesNames("slabs"));  //Finds all tiles in DB and store them in slabTileNames

        /*
        okButton.RegisterCallback<ClickEvent>(ev => GameObject.FindGameObjectWithTag("Player").GetComponent<DBInteractions>().ValidatePreSelection());
        wallToggle.RegisterCallback<ClickEvent>(ev => UpdateDisplay());
        slabToggle.RegisterCallback<ClickEvent>(ev => UpdateDisplay());
        */
    }

    IEnumerator getAllTilesNames(string filter = "all")
    {
        var webScript = GameObject.Find("Root").GetComponent<Web>();
        if (filter == "all")
        {
            tileNames.Clear();
            webScript.ListAllTileNamesInDB("all");
        }
        else if (filter == "walls")
        {
            wallTileNames.Clear();
            webScript.ListAllTileNamesInDB("walls");
        }
        else if (filter == "slabs")
        {
            slabTileNames.Clear();
            webScript.ListAllTileNamesInDB("slabs");
        }
        yield return null;

        foreach (string item in webScript.allTileNames)
        {
            switch (filter)
            {
                case "all":
                    tileNames.Add(item);
                    break;
                case "walls":
                    wallTileNames.Add(item);
                    break;
                case "slabs":
                    slabTileNames.Add(item);
                    break;
            }
        }
        yield return null;
    }
    void ShowHidePreselectionMenu()
    {
        if (mainMenu.style.display == DisplayStyle.Flex)
        {
            mainMenu.style.display = DisplayStyle.None;
        }
        else
        {
            mainMenu.style.display = DisplayStyle.Flex;
        }
    }
    /*
    /// <summary>
    /// Based on the menu's checkboxes, shows or hide the tiles applicable to walls or slabs.
    /// </summary>
    void UpdateDisplay()
    {
        List<VisualElement> veToRemove = new List<VisualElement>();
        foreach (VisualElement ve in myBox.Children())
        {
            veToRemove.Add(ve);
        }
        foreach (VisualElement ve in veToRemove)
        {
            myBox.Remove(ve);
        }

        var DBScript = GameObject.Find("FirstPersonController").GetComponent<DBInteractions>();
        if (wallToggle.value == true)
        {
            foreach (string tileName in wallTileNames)
            {
                string texturePath;
                texturePath = DBScript.GetTexturePathFromName(tileName);
                var myVE = new VisualElement();

                myVE.styleSheets.Add(Resources.Load<StyleSheet>("USS/testVE"));
                myVE.AddToClassList("medaillon");
                myVE.name = tileName;
                myVE.style.backgroundImage = DBScript.LoadTextureFromDisk(@"c:\users\aca\Documents\Projects\BIMEXPO\pictures_carrelages\" + texturePath);
                myBox.Add(myVE);

                myVE.RegisterCallback<ClickEvent>(ev => UpdateSelection(tileName));
            }
        }
        if (slabToggle.value == true)
        {
            foreach (string tileName in slabTileNames)
            {
                string texturePath;
                texturePath = DBScript.GetTexturePathFromName(tileName);
                var myVE = new VisualElement();

                myVE.styleSheets.Add(Resources.Load<StyleSheet>("USS/testVE"));
                myVE.AddToClassList("medaillon");
                myVE.name = tileName;
                myVE.style.backgroundImage = DBScript.LoadTextureFromDisk(@"c:\users\aca\Documents\Projects\BIMEXPO\pictures_carrelages\" + texturePath);
                myBox.Add(myVE);

                myVE.RegisterCallback<ClickEvent>(ev => UpdateSelection(tileName));
            }
        }
        //Apply style to tiles already preselected
        IEnumerable<VisualElement> tiles = myBox.Children();
        foreach (VisualElement tile in tiles)
        {
            if (localSelectedTiles.Contains(tile.name))
            {
                tile.RemoveFromClassList("medaillon");
                tile.AddToClassList("selected-medaillon");
            }
        }
    }

    /// <summary>
    /// Updates the list of tiles that the user wants to place in the preselection. That list being a public variable of this class.
    /// </summary>
    /// <param name="tileName"></param>
    void UpdateSelection(string tileName)
    {
        if (localSelectedTiles.Contains(tileName))
        {
            localSelectedTiles.Remove(tileName);
            var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
            myBox = rootVisualElement.Q<VisualElement>("img-container");
            IEnumerable<VisualElement> tiles = myBox.Children();
            foreach (VisualElement tile in tiles)
            {
                if (tile.name == tileName)
                {
                    tile.RemoveFromClassList("selected-medaillon");
                    tile.AddToClassList("medaillon");
                }
            }
        }
        else
        {
            localSelectedTiles.Add(tileName);
            var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
            myBox = rootVisualElement.Q<VisualElement>("img-container");
            IEnumerable<VisualElement> tiles = myBox.Children();
            foreach (VisualElement tile in tiles)
            {
                if (tile.name == tileName)
                {
                    tile.RemoveFromClassList("medaillon");
                    tile.AddToClassList("selected-medaillon");
                }
            }
        }
            
    }
    */
}
