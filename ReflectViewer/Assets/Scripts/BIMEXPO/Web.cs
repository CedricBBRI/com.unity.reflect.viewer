using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Reflect; //Unity.Reflect has to be added to the asmdef in the current folder
using System.Collections.ObjectModel;
using Unity.Reflect.Viewer.UI;

public class Web : MonoBehaviour
{
    private bool buildingTableCreated = false;
    private bool localPreselectionDone = false;
    public bool preselectionDone { get { return localPreselectionDone; } }
    public string texturePath { get; set; }
    private List<string> localSelectedTiles = new List<string>();
    private List<string> localWallSelectedTiles = new List<string>();
    private List<string> localSlabSelectedTiles = new List<string>();
    private List<string> localTileNames = new List<string>();
    private List<string> localWallTileNames = new List<string>();
    private List<string> localSlabTileNames = new List<string>();
    public ReadOnlyCollection<string> preselectedTiles { get { return localSelectedTiles.AsReadOnly(); } } // preselectedTiles can be read but not modified outside this class
    public ReadOnlyCollection<string> wallPreselectedTiles { get { return localWallSelectedTiles.AsReadOnly(); } }
    public ReadOnlyCollection<string> slabPreselectedTiles { get { return localSlabSelectedTiles.AsReadOnly(); } }
    public ReadOnlyCollection<string> allTileNames { get { return localTileNames.AsReadOnly(); } }
    public ReadOnlyCollection<string> wallTileNames { get { return localWallTileNames.AsReadOnly(); } }
    public ReadOnlyCollection<string> slabTileNames { get { return localSlabTileNames.AsReadOnly(); } }

    [Header("DATABASE")]
    public string host;
    public string database, username, password, tilesTable;
    [Header("PROJECT DETAILS")]
    public string clientId;
    public string projectId;

    void Start()
    {
        // !! YOU NEED TO HAVE SET UP A VRITUALHOST NAMED 'bimexpo', pointing to the 'PHP' folder
        string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string csvDir = Directory.GetParent(currentDir).Parent.Parent.FullName;
        string csvPath = csvDir + "\\DB_Carrelages_Demo.csv";
        csvPath = csvPath.Replace("\\", "/");                   //SQL needs forwards slashes...
        StartCoroutine(CreateTableFromCSV(csvPath, "tptiles"));
        StartCoroutine(CreateUserChoicesTable());

        // Enable the preselection menu now, to be 100% sure the coroutines here above rune BEFORE that
        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name == "PreselectionMenu")
            {
                go.SetActive(true);
            }
        }

    }

    private void Update()
    {
        //UIStateManager myUIState = new UIStateManager();
        //UIStateData usd = myUIState.stateData;
        //int cp = usd.progressData.currentProgress;

        //Debug.Log("DEBUG UI STATE: " + cp);

        //m_UIStateData
        // Try to access m_UImanager
        if (GameObject.Find("Root").transform.Find("Cube") == null)
        {
            if (GameObject.Find("Root").transform.childCount > 0 && !buildingTableCreated)
            {
                StartCoroutine(createBuildingTable());
                buildingTableCreated = true;
            }
        }
        else
        {
            if (GameObject.Find("Root").transform.childCount > 1 && !buildingTableCreated)
            {
                StartCoroutine(createBuildingTable());
                buildingTableCreated = true;
            }
        }
    }

    IEnumerator CreateUserChoicesTable()
    {
        WWWForm form = new WWWForm();
        form.AddField("clientId", GameObject.Find("Root").GetComponent<DBInteractions>().clientId);
        form.AddField("projectId", GameObject.Find("Root").GetComponent<DBInteractions>().projectId);

        using (UnityWebRequest www = UnityWebRequest.Post("http://bimexpo/CreateUserChoicesTable.php", form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
            }
        }
    }
    IEnumerator CreateTableFromCSV(string csvPath, string tableName)
    {
        WWWForm form = new WWWForm();
        form.AddField("tableName", tableName);
        form.AddField("csvPath", csvPath);

        using (UnityWebRequest www = UnityWebRequest.Post("http://bimexpo/CreateTableFromCSV.php", form))
        {
            // Request and wait for the desired page.
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
            }
        }
    }

    IEnumerator createBuildingTable()
    {
        yield return new WaitForSeconds(10); // Waits 10s for the model to be loaded before creating the table

        List<string> surfaceIDs = new List<string>();
        List<string> surfaceArea = new List<string>();
        List<string> surfaceLevels = new List<string>();
        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            var meta = go.GetComponent<Metadata>();
            if (meta != null)
            {
                if ((go.name.Contains("Wall") || meta.GetParameter("Category").Contains("Wall") || go.name.Contains("Floor") || meta.GetParameter("Category").Contains("Floor"))
                    && (meta.GetParameter("Id").Length > 0))
                {
                    surfaceIDs.Add(meta.GetParameter("Id"));
                    surfaceArea.Add(meta.GetParameter("Area"));
                    surfaceLevels.Add(meta.GetParameter("Base Constraint"));
                }
            }
        }

        WWWForm form = new WWWForm();
        form.AddField("clientId", GameObject.Find("Root").GetComponent<DBInteractions>().clientId);
        form.AddField("projectId", GameObject.Find("Root").GetComponent<DBInteractions>().projectId);
        for (int i = 0; i < surfaceIDs.Count; i++)
        {
            form.AddField("ID[]", surfaceIDs[i]);
            form.AddField("Area[]", surfaceArea[i]);
            form.AddField("Level[]", surfaceLevels[i]);
        }
        
        using (UnityWebRequest www = UnityWebRequest.Post("http://bimexpo/CreateBuildingTable.php", form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
            }
        }
    }

    /// <summary>
    /// Gets the list of the names ('libelles') of all the preselected tiles in the project.
    /// </summary>
    //public IEnumerator RetrievePreselectedTiles()
    public void RetrievePreselectedTiles(string category = "all")
    {
        //yield return new WaitForSeconds(10); // If this function is called immediately after CreateTableFromCSV, it needs some time for the table to actually be created

        WWWForm form = new WWWForm();
        form.AddField("clientId", GameObject.Find("Root").GetComponent<DBInteractions>().clientId);
        form.AddField("projectId", GameObject.Find("Root").GetComponent<DBInteractions>().projectId);
        form.AddField("category", category);

        string[] phpReturnedList = { };        

        using (UnityWebRequest www = UnityWebRequest.Post("http://bimexpo/GetTilePreselection.php", form))
        {
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
                string receivedTilesString = www.downloadHandler.text;
                phpReturnedList = receivedTilesString.Split(';');
            }
        }

        bool startRecordingResults = false;

        foreach (string item in phpReturnedList)
        {
            if (startRecordingResults)
            {
                switch (category)
                {
                    case "all":
                        localSelectedTiles.Add(item);
                        break;
                    case "walls":
                        localWallSelectedTiles.Add(item);
                        break;
                    case "slabs":
                        localSlabSelectedTiles.Add(item);
                        break;
                }
                
            }
            if (item.Contains("RETURNS"))
            {
                startRecordingResults = true;
                switch (category)
                {
                    case "all":
                        localSelectedTiles.Clear();
                        break;
                    case "walls":
                        localWallSelectedTiles.Clear();
                        break;
                    case "slabs":
                        localSlabSelectedTiles.Clear();
                        break;
                }
            }
        }
    }

    public string GetTexturePathFromNameM(string name)
    {
        texturePath = null; // So that is is null as long as the DB request is not done.
        WWWForm form = new WWWForm();
        form.AddField("clientId", GameObject.Find("Root").GetComponent<DBInteractions>().clientId);
        form.AddField("projectId", GameObject.Find("Root").GetComponent<DBInteractions>().projectId);
        form.AddField("name", name);

        string[] phpReturnedList = { };

        using (UnityWebRequest www = UnityWebRequest.Post("http://bimexpo/GetTexturePathFromName.php", form))
        {
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
                string receivedTilesString = www.downloadHandler.text;
                phpReturnedList = receivedTilesString.Split(';');
            }
        }

        List<string> texturePaths = new List<string>();
        bool startRecordingResults = false;

        foreach (string item in phpReturnedList)
        {
            if (startRecordingResults)
            {
                texturePaths.Add(item);
            }
            if (item.Contains("RETURNS"))
            {
                startRecordingResults = true;
            }
        }
        return texturePaths[0];
    }

    /// <summary>
    /// Given a tile name ('libelle'), finds the path to its texture, which is located in the table 'chemin_texture' column.
    /// For the moment this path is simply the name of the folder in which the textures are stored for a given tile.
    /// </summary>
    /// <param name="name">The name of the tile (i.e. the 'libelle').</param>
    /// <returns>The path to the texture, as stored in the table.</returns>
    public IEnumerator GetTexturePathFromName(string name)
    {
        texturePath = null; // So that is is null as long as the DB request is not done.
        WWWForm form = new WWWForm();
        form.AddField("clientId", GameObject.Find("Root").GetComponent<DBInteractions>().clientId);
        form.AddField("projectId", GameObject.Find("Root").GetComponent<DBInteractions>().projectId);
        form.AddField("name", name);

        string[] phpReturnedList = { };

        using (UnityWebRequest www = UnityWebRequest.Post("http://bimexpo/GetTexturePathFromName.php", form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string receivedTilesString = www.downloadHandler.text;
                phpReturnedList = receivedTilesString.Split(';');
            }
        }

        List<string> texturePaths = new List<string>();
        bool startRecordingResults = false;

        foreach (string item in phpReturnedList)
        {
            if (startRecordingResults)
            {
                texturePaths.Add(item);
            }
            if (item.Contains("RETURNS"))
            {
                startRecordingResults = true;
            }
        }
        texturePath = texturePaths[0];
        //yield return null;

        // TO DO: RETURN the path either via a class variable (but then I can't use it right after, because it might tke some time) or via callback??
        // http://codesaying.com/action-callback-in-unity/
        // https://forum.unity.com/threads/how-to-use-coroutines-and-callback-properly-retrieving-an-array-out-of-an-ienumerator.508017/
    }

    /// <summary>
    /// Sends a WebRequest via PHP script to retrieve a list of the names of either all the tiles, or all the wall tiles, or all the slab tiles in the DB.
    /// The class variables localTileNames, localWallTileNames, or localSlabTileNames are updated accordingly.
    /// </summary>
    /// <param name="filter">The (optional) filter to choose between all, walls, or slabs.</param>
    public IEnumerator ListAllTileNamesInDB(string filter = "all")
    {
        WWWForm form = new WWWForm();
        form.AddField("tilesTableName", tilesTable);

        string[] phpReturnedList = { };
        string phpScript = "http://bimexpo/ListAllTilesNamesInDB.php";
        switch (filter)
        {
            case "all":
                localTileNames.Clear();
                break;
            case "walls":
                phpScript = "http://bimexpo/ListAllWallTilesNamesInDB.php";
                localWallTileNames.Clear();
                break;
            case "slabs":
                phpScript = "http://bimexpo/ListAllSlabTilesNamesInDB.php";
                localSlabTileNames.Clear();
                break;
        }

        using (UnityWebRequest www = UnityWebRequest.Post(phpScript, form))
        {
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string receivedTilesString = www.downloadHandler.text;
                phpReturnedList = receivedTilesString.Split(';');
            }
        }

        bool startRecordingResults = false;

        foreach (string item in phpReturnedList)
        {
            if (startRecordingResults)
            {
                switch (filter)
                {
                    case "all":
                        localTileNames.Add(item);
                        break;
                    case "walls":
                        localWallTileNames.Add(item);
                        break;
                    case "slabs":
                        localSlabTileNames.Add(item);
                        break;
                }
                
            }
            if (item.Contains("RETURNS"))
            {
                startRecordingResults = true;
            }
        }
    }

    /// <summary>
    /// Given a folder path, this function gets the 1st file in the folder and returns it as a 2D Texture.
    /// </summary>
    /// <param name="FolderPath">The full path of the folder to look into.</param>
    /// <returns>A Texture2D of the 1st file found in the folder.</returns>
    public Texture2D LoadTextureFromDiskFolder(string FolderPath)
    {
        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails
        Texture2D Tex2D;
        byte[] FileData;
        string[] filesInDir;

        //Files in the directory
        filesInDir = Directory.GetFiles(FolderPath);

        //Get the 1st image within directory
        string picture = filesInDir[0];

        if (File.Exists(picture))
        {
            Debug.Log("File exists!");
            FileData = File.ReadAllBytes(picture);
            Tex2D = new Texture2D(2, 2);                // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))              // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                           // If data = readable -> return texture
        }
        Debug.Log("File doesn't exist!");
        return null;                                    // Return null if load failed
    }

    public IEnumerator ValidatePreSelection()
    {
        WWWForm form = new WWWForm();
        form.AddField("clientId", clientId);
        form.AddField("projectId", projectId);
        foreach (string tile in GameObject.Find("PreselectionMenu").GetComponent<PreselectionMenuScript>().selectedTiles)
        {
            form.AddField("preselectedTiles[]", tile);
        }

        using (UnityWebRequest www = UnityWebRequest.Post("http://bimexpo/ValidatePreselections.php", form))
        {
            // Request and wait for the desired page.
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
            }
        }
        localPreselectionDone = true;
    }

    /// <summary>
    /// This function is merely a wrapper function for ProduceAmendment.
    /// This way, it can be associated to a click event on the corresponding button.
    /// </summary>
    public void ProduceAmendmentWrapper()
    {
        StartCoroutine(ProduceAmendment());
    }

    /// <summary>
    /// Produces the amendment in an HTML page, and opens the page.
    /// </summary>
    private IEnumerator ProduceAmendment()
    {
        WWWForm form = new WWWForm();
        form.AddField("clientId", clientId);
        form.AddField("projectId", projectId);

        string phpScript = "http://bimexpo/CreateAmendment.php";
        
        using (UnityWebRequest www = UnityWebRequest.Post(phpScript, form))
        {
            yield return www.SendWebRequest();      // I have to do this here, otherwise www.result is still "InProgress" on the next line, and therefore enters the if, although it is a Success!

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                Application.OpenURL("http://bimexpo/amendment.php?clientId=" + clientId + "&projectId=" + projectId);
            }
        }
    }

    /// <summary>
    /// This method returns a list of the local paths for the textures that are compatible with the surface provided as argument.
    /// In this simple version, the only filtering is done on the type of surface: wall or floor.
    /// Only the preselected tiles are pulled from DB.
    /// </summary>
    /// <param name="surface">The surface for which the compatible textures are requested.</param>
    /// <returns>A List<string> of all the paths to the texture files.</string></returns>
    public List<string> PullTexturesForSurface(GameObject surface)
    {
        string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string picturesDir = Directory.GetParent(currentDir).Parent.Parent.FullName + "\\pictures_carrelages\\";

        string phpScript = "http://bimexpo/GetCompatibleTexturesFromDB.php";
        string[] phpReturnedList = { };
        List<string> textures = new List<string>();
        var meta = surface.GetComponent<Metadata>();

        WWWForm form = new WWWForm();

        if (meta != null)
        {
            if (surface.name.Contains("Wall") || meta.GetParameter("Category").Contains("Wall"))
            {
                form.AddField("category", "wall");
            }
            else if (surface.name.Contains("Floor") || meta.GetParameter("Category").Contains("Floor"))
            {
                form.AddField("category", "floor");
            }
            else
            {
                Debug.Log("The type of surface is not recognized");
                return textures;
            }
            using (UnityWebRequest www = UnityWebRequest.Post(phpScript, form))
            {
                www.SendWebRequest();
                while (www.result == UnityWebRequest.Result.InProgress)
                {
                    // Just wait
                }
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    string receivedTilesString = www.downloadHandler.text;
                    phpReturnedList = receivedTilesString.Split(';');
                }
            }
            bool startRecordingResults = false;

            foreach (string item in phpReturnedList)
            {
                if (startRecordingResults)
                {
                    string[] myfiles = Directory.GetFiles(picturesDir + item);
                    textures.Add(myfiles[0]);
                }
                if (item.Contains("RETURNS"))
                {
                    startRecordingResults = true;
                }
            }
            return textures;
        }
        else
        {
            Debug.Log("The selected surface has no metadata!");
            return textures;
        }
    }

    /// <summary>
    /// Extracts the tile dimensions in a 2 items list of integers.
    /// The dimensions are extracted from the tile libelle, since it is part of it.
    /// </summary>
    /// <returns>A List containting 2 integers, which are the dimensions of the tile</returns>
    public List<double> GetTileDimensionsFromLibelle(string libelle)
    {
        List<double> dimensions = new List<double>();
        string[] libelleList = libelle.Split(' ');
        string[] dimList = { };
        foreach (string item in libelleList)
        {
            if (item.Contains("/"))
            {
                dimList = item.Split('/');
                break;
            }
        }
        if (dimList == null || dimList.Length != 2)
        {
            throw new Exception("Can't extract dimensions from tile libelle!");
        }
        foreach (string item in dimList)
        {
            double convertedDim;
            if (Double.TryParse(item, out convertedDim))
            {
                dimensions.Add(convertedDim/100.0);
            }
            else
            {
                throw new Exception("Can't extract dimensions from tile libelle!");
            }
        }
        return dimensions;
    }

    public double GetTilePriceFromLibelle(string libelle)
    {
        double price = -1;
        string stringPrice = "";

        WWWForm form = new WWWForm();
        form.AddField("libelle", libelle);
        string phpScript = "http://bimexpo/GetTilePriceFromLibelle.php";
        string[] phpReturnedList = { };

        using (UnityWebRequest www = UnityWebRequest.Post(phpScript, form))
        {
            www.SendWebRequest();
            while (www.result == UnityWebRequest.Result.InProgress)
            {
                // Just wait
            }
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string receivedTilesString = www.downloadHandler.text;
                phpReturnedList = receivedTilesString.Split(';');
            }
        }
        bool startRecordingResults = false;
        foreach (string item in phpReturnedList)
        {
            if (startRecordingResults)
            {
                stringPrice = item;
            }
            if (item.Contains("RETURNS"))
            {
                startRecordingResults = true;
            }
        }
        if (stringPrice != "" && stringPrice.Split('€').Length > 0)
        {
            var toto = stringPrice.Split('€');
            var tata = toto[0];
            if (Double.TryParse(stringPrice.Split('€')[0], out price))
            {
                return price;
            }
            else
            {
                throw new Exception("Couldn't extract the tile price from the DB!");
            }
        }
        else
        {
            throw new Exception("Couldn't extract the tile price from the DB!");
        }
    }
}
