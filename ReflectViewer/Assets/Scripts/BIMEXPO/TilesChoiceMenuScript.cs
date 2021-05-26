using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Reflect;
using System.IO;
using System.Reflection;
using UnityEngine.Networking;

public class TilesChoiceMenuScript : MonoBehaviour
{
    public string chosenMaterial, chosenTexturePath;
    public GameObject target;
    private Button okButton;


    void OnEnable()
    {
        //Create the menu
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        okButton = rootVisualElement.Q<Button>("ok-button");
        okButton.styleSheets.Add(Resources.Load<StyleSheet>("USS/testVE"));
        okButton.AddToClassList("ok-but");
        okButton.clicked += SaveChosenMaterialToDB;
        okButton.clicked += ApplyChosenMaterialToSurface;
        okButton.clicked += CloseMenu;

        VisualElement imgContainer = rootVisualElement.Q<VisualElement>("img-container");
        imgContainer.styleSheets.Add(Resources.Load<StyleSheet>("USS/testVE"));
        imgContainer.AddToClassList("medaillon-container");

        Label menuTitle = rootVisualElement.Q<Label>("menu-title");
        menuTitle.styleSheets.Add(Resources.Load<StyleSheet>("USS/testVE"));
        menuTitle.AddToClassList("title-menu");

        PopulateMenu();
    }

    /// <summary>
    /// Changes the visual aspect of the tile that is selected in the menu.
    /// </summary>
    /// <param name="name">The 'libellé' of the tile</param>
    public void SelectMaterial(string name)
    {
        var webScript = GameObject.Find("Root").GetComponent<Web>();
        chosenMaterial = name;
        chosenTexturePath = webScript.GetTexturePathFromNameM(name);
        string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string tilePicturesFolder = Directory.GetParent(currentDir).Parent.Parent.FullName + "\\pictures_carrelages\\";
        chosenTexturePath = tilePicturesFolder + chosenTexturePath;

        //Change visual aspect of the chosen material, in the menu
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        VisualElement myBox = rootVisualElement.Q<VisualElement>("img-container");
        IEnumerable<VisualElement> tiles = myBox.Children();
        foreach (VisualElement tile in tiles)
        {
            if (tile.name == name)
            {
                tile.RemoveFromClassList("medaillon");
                tile.AddToClassList("selected-medaillon");
            }
            else
            {
                tile.RemoveFromClassList("selected-medaillon");
                tile.AddToClassList("medaillon");
            }
        }
    }

    void ApplyChosenMaterialToSurface()
    {
        var webScript = GameObject.Find("Root").GetComponent<Web>();

        // Get the tile dimensions
        List<int> tileDimensions = new List<int>();
        try
        {
            tileDimensions = webScript.GetTileDimensionsFromLibelle(chosenMaterial);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            throw;
        }

        // Then continue with material application onto surface
        var texture = webScript.LoadTextureFromDiskFolder(chosenTexturePath);
        Material tempMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        tempMat.mainTexture = texture;

        Texture2D texMort = (Texture2D)tempMat.mainTexture;
        tempMat.mainTexture = texMort;

        foreach (Renderer rend in target.GetComponents<Renderer>())
        {
            var mats = new Material[rend.sharedMaterials.Length];
            for (var j = 0; j < rend.sharedMaterials.Length; j++)
            {
                mats[j] = tempMat;
            }
            rend.sharedMaterials = mats;
        }
        target.GetComponent<MeshRenderer>().material = tempMat;
        return;
    }
    
    /// <summary>
    /// Saves the choice of a tile on a given surface. This will save the choice in the DB.
    /// </summary>
    void SaveChosenMaterialToDB()
    {
        var webScript = GameObject.Find("Root").GetComponent<Web>();
        var changeMatScript = GameObject.Find("Root").GetComponent<ChangeMaterial>();
        WWWForm form = new WWWForm();
        form.AddField("surfaceId", changeMatScript.selectedObject.GetComponent<Metadata>().GetParameter("Id"));
        form.AddField("tileName", chosenMaterial);
        form.AddField("clientId", webScript.clientId);
        form.AddField("projectId", webScript.projectId);
        form.AddField("tilePrice", webScript.GetTilePriceFromLibelle(chosenMaterial).ToString());
        form.AddField("surfaceArea", changeMatScript.selectedObject.GetComponent<Metadata>().GetParameter("Area"));

        using (UnityWebRequest www = UnityWebRequest.Post("http://bimexpo/SaveMaterialChoiceToDB.php", form))
        {
            // Request and wait for the desired page.
            www.SendWebRequest();
            while (www.result == UnityWebRequest.Result.InProgress)
            {
                //Wait
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
            }
        }




        //string path = @"c:\users\aca\Documents\Projects\BIMEXPO\pictures_carrelages\" + webScript.GetTexturePathFromName(chosenMaterial);
        //webScript.changeMaterial(target, path);
        //Get the ID of the surface from Metadata script
        //string surfaceId = target.GetComponent<dummyMetadataScript>().ID.ToString();
        //Get the tile ID from its name
        //string tileId = webScript.GetTileIdFromName(chosenMaterial);
        //Save the user choice to DB
        //webScript.SaveUserChoiceToDB(tileId, surfaceId);
    }

    void PopulateMenu()
    {
        //Recuperate the hit surface
        target = GameObject.Find("Root").GetComponent<ChangeMaterial>().selectedObject;

        //Recuperate the list of preselected tiles - from DB
        var webScript = GameObject.Find("Root").GetComponent<Web>();
        
        //Populate the menu with this selection
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        VisualElement myBox = rootVisualElement.Q<VisualElement>("img-container");
        Button okButton = rootVisualElement.Q<Button>("ok-button");

        //The menu should only suggest tiles for walls or for ground, depending on what was hit
        List<string> filteredList = new List<string>();
        if (target == null)
        {
            Debug.LogError("Tile choice menu didn't get the selected surface!");
            return;
        }
        if (target.GetComponent<Metadata>().GetParameter("Category").Contains("Wall"))
        {
            webScript.RetrievePreselectedTiles("walls"); //No need for coroutine, we have to wait for this menu anyways..
            List<string> selectedTiles = new List<string>(webScript.wallPreselectedTiles);
            filteredList = selectedTiles;
        }
        //filteredList = player.GetComponent<DBInteractions>().FilterWallsOnlyFromTileList(selectedTiles);
        else if (target.GetComponent<Metadata>().GetParameter("Category").Contains("Floor"))
        {
            webScript.RetrievePreselectedTiles("slabs"); //No need for coroutine, we have to wait for this menu anyways..
            List<string> selectedTiles = new List<string>(webScript.slabPreselectedTiles);
            filteredList = selectedTiles;
        }
        //filteredList = player.GetComponent<DBInteractions>().FilterSlabsOnlyFromTileList(selectedTiles);
        else
            Debug.Log("The hit surface is not categorized as wall or floor.");

        string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string tilePicturesFolder = Directory.GetParent(currentDir).Parent.Parent.FullName + "\\pictures_carrelages\\";
        string texturePath;
        foreach (string tileName in filteredList)
        {
            texturePath = webScript.GetTexturePathFromNameM(tileName);
            var myVE = new VisualElement();
            myVE.styleSheets.Add(Resources.Load<StyleSheet>("USS/testVE"));
            myVE.AddToClassList("medaillon");
            myVE.style.backgroundImage = webScript.LoadTextureFromDiskFolder(tilePicturesFolder + texturePath);
            myVE.name = tileName;
            myBox.Add(myVE);
            myVE.RegisterCallback<ClickEvent>(ev => SelectMaterial(tileName));
        }
    }

    /// <summary>
    /// Closes the menu.
    /// </summary>
    void CloseMenu()
    {
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        VisualElement myBox = rootVisualElement.Q<VisualElement>("img-container");
        List<VisualElement> myBoxList = new List<VisualElement>();
        foreach (VisualElement ve in myBox.Children())
        {
            myBoxList.Add(ve);
        }
        foreach (VisualElement item in myBoxList)
        {
            myBox.Remove(item);
        }
        GameObject.Find("TileChoiceMenu").SetActive(false);
        GetComponent<MenusHandler>().m_MyEvent.RemoveAllListeners();
    }
}
