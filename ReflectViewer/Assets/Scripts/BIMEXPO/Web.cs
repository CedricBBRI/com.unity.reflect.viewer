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
using SimpleJSON;

public class Web : MonoBehaviour
{
    private bool tablesCreated = false;
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
        UIStateManager.stateChanged += UIStateManager_stateChanged; // Listening to UI state change in order to know when the building is loaded.
    }

    private void UIStateManager_stateChanged(UIStateData obj)
    {
        if (obj.progressData.totalCount > 0 && obj.progressData.currentProgress == obj.progressData.totalCount)    // Then the building is fully loaded
        {
            if (!tablesCreated)
            {
                StartCoroutine(createBuildingTable());
                //StartCoroutine(SetDefaultMaterials());
                StartCoroutine(SetDefaultMaterialsJSON()); 
                var fao = GameObject.Find("Root").GetComponent<FindAllObjects>();
                fao.FindAll("Wall");
                tablesCreated = true;
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
        /*
        if (GameObject.Find("Root").transform.Find("Cube") == null)
        {
            if (GameObject.Find("Root").transform.childCount > 0 && !buildingTableCreated)
            {
                StartCoroutine(createBuildingTable());
                StartCoroutine(SetDefaultMaterials());
                buildingTableCreated = true;
            }
        }
        else
        {
            if (GameObject.Find("Root").transform.childCount > 1 && !buildingTableCreated)
            {
                StartCoroutine(createBuildingTable());
                StartCoroutine(SetDefaultMaterials());
                buildingTableCreated = true;
            }
        }
        */
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

            JSONArray jsonTextures = new JSONArray();
            bool wait = true;
            StartCoroutine(GetJSONResultFromDBCoroutine(phpScript, (jsonResult) =>
            {
                jsonTextures = jsonResult; // Recuperate jsonResult (which is the argument of the callback method, passed in inside GetJSONResultFromDB. So it is jsonArray.)
                wait = false;               // This line is reached only upon callback completion inside GetJSONResultFromDB.
            }, form));

            // Read the JSON result
            for (int i = 0; i < jsonTextures.Count; i++)
            {
                string[] myfiles = Directory.GetFiles(picturesDir + jsonTextures[i].AsObject["chemin_texture"]);
                textures.Add(myfiles[0]);
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

    public void saveComment(string comment, GameObject surface)
    {
        // Getting surface ID
        var meta = surface.GetComponent<Metadata>();
        string surfaceID = null;
        if (meta != null)
        {
            surfaceID = meta.GetParameter("Id");
        }
        else
        {
            throw new Exception("No Id parameter found on surface!");
        }

        string phpScript = "http://bimexpo/CreateComment.php";
        WWWForm form = new WWWForm();
        form.AddField("projectId", projectId);
        form.AddField("clientId", clientId);
        form.AddField("comment", comment);
        form.AddField("surfaceID", surfaceID);

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
            }
        }
    }

    public void saveScreenshot(string filename, Vector3 position, Quaternion rotation, GameObject targetSurface)
    {
        // Getting surface ID
        var meta = targetSurface.GetComponent<Metadata>();
        string surfaceID = null;
        if (meta != null)
        {
            surfaceID = meta.GetParameter("Id");
        }
        else
        {
            throw new Exception("No Id parameter found on surface!");
        }

        string phpScript = "http://bimexpo/SaveScreenshot.php";
        WWWForm form = new WWWForm();
        form.AddField("projectId", projectId);
        form.AddField("clientId", clientId);
        form.AddField("filename", filename);
        form.AddField("surfaceID", surfaceID);
        form.AddField("positionX", position.x.ToString());
        form.AddField("positionY", position.y.ToString());
        form.AddField("positionZ", position.z.ToString());
        form.AddField("rotationX", rotation.x.ToString());
        form.AddField("rotationY", rotation.y.ToString());
        form.AddField("rotationZ", rotation.z.ToString());
        // Should I also use the Quaternion w component of the rotation? Don't care for the moment.


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
                Debug.Log(www.downloadHandler.text);
            }
        }
    }

    public string GetComment(GameObject surface)
    {
        // Getting surface ID
        var meta = surface.GetComponent<Metadata>();
        string surfaceID = null;
        if (meta != null)
        {
            surfaceID = meta.GetParameter("Id");
        }
        else
        {
            throw new Exception("No Id parameter found on surface!");
        }

        string phpScript = "http://bimexpo/GetComment.php";
        string[] phpReturnedList = { };
        WWWForm form = new WWWForm();
        form.AddField("projectId", projectId);
        form.AddField("clientId", clientId);
        form.AddField("surfaceID", surfaceID);

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
        string comment = "";

        foreach (string item in phpReturnedList)
        {
            if (startRecordingResults)
            {
                comment = item;
            }
            if (item.Contains("RETURNS"))
            {
                startRecordingResults = true;
            }
        }
        return comment;
    }

    /// <summary>
    /// Flexible function that gets the result of a PHP script as a JSON array, and that uses a callback function to then pass that result.
    /// </summary>
    /// <param name="scriptName">The PHP script to be called via WebRequest.</param>
    /// <param name="callback">The callback function that will be used to retrieve the JSON array via its argument.</param>
    /// <returns></returns>
    private IEnumerator GetJSONResultFromDBCoroutine(string scriptName, Action<JSONArray> callback, WWWForm form = null)
    {
        string[] phpReturnedList = { };
        UnityWebRequest myWWW;
        if (form != null)
        {
            myWWW = UnityWebRequest.Post(scriptName, form);
        }
        else
        {
            myWWW = UnityWebRequest.Get(scriptName);
        }

        using (UnityWebRequest www = myWWW)
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Get the response from DB and split messages from json response
                string receivedTilesString = www.downloadHandler.text;
                string jsonArrayString = "";
                phpReturnedList = receivedTilesString.Split(';');
                bool startRecordingResults = false;
                foreach (string item in phpReturnedList)
                {
                    if (startRecordingResults)
                    {
                        jsonArrayString = item;
                        break;
                    }
                    if (item.Contains("RETURNS"))
                    {
                        startRecordingResults = true;
                    }
                }

                // Parse the JSON result into a JSONArray
                JSONArray jsonArray = JSON.Parse(jsonArrayString) as JSONArray;

                // Once the results are obtained, pass them on to callback.
                callback(jsonArray);    
            }
        }
    }

    /// <summary>
    /// Automatically sets default materials on the building, based on materials identified as such in the DB.
    /// </summary>
    private IEnumerator SetDefaultMaterialsJSON()
    {
        List<List<string>> defaultsList = new List<List<string>> { };
        string phpScript = "http://bimexpo/ReadDefaults.php";
        JSONArray jsonMaterials = new JSONArray();

        bool wait = true;
        StartCoroutine(GetJSONResultFromDBCoroutine(phpScript, (jsonResult) =>
        {
            jsonMaterials = jsonResult; // Recuperate jsonResult (which is the argument of the callback method, passed in inside GetJSONResultFromDB. So it is jsonArray.)
            wait = false;               // This line is reached only upon callback completion inside GetJSONResultFromDB.
        }));

        while (wait)    // Wait for the call to DB in GetJSONResultFromDB is done, so we're sure we have now retrieved jsonResult inside jsonMaterials.
        {
            yield return null;
        }

        // Read the JSON result
        for (int i = 0; i < jsonMaterials.Count; i++)
        {
            string surfaceType = jsonMaterials[i].AsObject["surface_type"];
            string inOut = jsonMaterials[i].AsObject["in_out"];
            string matName = jsonMaterials[i].AsObject["material_name"];
            List<string> subList = new List<string>();
            defaultsList.Add(new List<string> { surfaceType, inOut, matName });
        }

        foreach (List<string> subList in defaultsList)
        {
            // Load the material
            GameObject root = GameObject.Find("Root");
            Component[] children = root.GetComponentsInChildren(typeof(Transform));
            Material matToApply = Resources.Load<Material>("defaults/materials/" + subList[2]);
            matToApply.shader = Shader.Find("UnityReflect/URPOpaque");

            // Detect brick walls and apply material
            foreach (Transform tr in children)
            {
                var meta = tr.gameObject.GetComponent<Metadata>();
                if (meta != null)
                {
                    if (meta.GetParameter("Type").Contains("Brique") && subList[1] == "out")
                    {
                        tr.gameObject.GetComponent<MeshRenderer>().material = matToApply;
                    }
                    else if (meta.GetParameter("Type").Contains("Carrelage_Mural") && subList[1] == "in")
                    {
                        tr.gameObject.GetComponent<MeshRenderer>().material = matToApply;
                    }
                }
            }
        }

        yield return null;
    }

    /// <summary>
    /// [OBSOLETE] Automatically sets default materials on the building, based on materials identified as such in the DB.
    /// </summary>
    private IEnumerator SetDefaultMaterials()
    {
        string[] input1 = { "mur", "out" };
        string[] input2 = { "mur", "in" };
        List<List<string>> defaultsList = new List<List<string>>{new List<string> ( input1 ), new List<string>(input2)};
        int nbItems = defaultsList.Count;   // Do it here to avoid infinite loop

        for (int i = 0; i < nbItems; i++)
        {
            List<string> subList = defaultsList[i];
            // Identify the material for outdoor walls, from DB
            WWWForm form = new WWWForm();
            form.AddField("surface_type", subList[0]);
            form.AddField("in_out", subList[1]);
            string phpScript = "http://bimexpo/ReadDefaults.php";
            string[] phpReturnedList = { };
            string materialName = "";

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
                    Debug.Log(receivedTilesString);
                    phpReturnedList = receivedTilesString.Split(';');
                }
            }
            bool startRecordingResults = false;
            foreach (string item in phpReturnedList)
            {
                if (startRecordingResults)
                {
                    materialName = item;
                }
                if (item.Contains("RETURNS"))
                {
                    startRecordingResults = true;
                }
            }

            subList.Add(materialName);
            defaultsList.Add(subList);
        }
        foreach (List<string> subList in defaultsList)
        {
            // Load the material
            GameObject root = GameObject.Find("Root");
            Component[] children = root.GetComponentsInChildren(typeof(Transform));
            Material matToApply = Resources.Load<Material>("defaults/materials/" + subList[2]);
            //matToApply.shader = Shader.Find("Unlit/Texture");
            matToApply.shader = Shader.Find("UnityReflect/URPOpaque");
            //matToApply.shader.

            // Detect brick walls and apply material
            foreach (Transform tr in children)
            {
                var meta = tr.gameObject.GetComponent<Metadata>();
                if (meta != null)
                {
                    if (meta.GetParameter("Type").Contains("Brique") && subList[1] == "out")
                    {
                        tr.gameObject.GetComponent<MeshRenderer>().material = matToApply;
                    }
                    else if (meta.GetParameter("Type").Contains("Carrelage_Mural") && subList[1] == "in")
                    {
                        tr.gameObject.GetComponent<MeshRenderer>().material = matToApply;
                    }
                }
            }
        }
        
        yield return null;
    }

    /// <summary>
    /// Restores the previous choices a user may have done in another session.
    /// These choices are retrieved from DB and overwrite any choices in the current session.
    /// </summary>
    public IEnumerator RestorePreviousConfig()
    {
        WWWForm form = new WWWForm();
        form.AddField("clientId", clientId);
        form.AddField("projectId", projectId);
        string phpScript = "http://bimexpo/GetChoices.php";
        JSONArray jsonResponse = new JSONArray();
        bool wait = true;
        StartCoroutine(GetJSONResultFromDBCoroutine(phpScript, (jsonResult) =>
        {
            jsonResponse = jsonResult; // Recuperate jsonResult (which is the argument of the callback method, passed in inside GetJSONResultFromDB. So it is jsonArray.)
            wait = false;              // This line is reached only upon callback completion inside GetJSONResultFromDB.
        }, form));

        while (wait)    // Wait for the call to DB in GetJSONResultFromDB is done, so we're sure we have now retrieved jsonResult inside jsonMaterials.
        {
            yield return null;
        }

        // Read the JSON result
        List<List<string>> idsAndLibelle = new List<List<string>>();
        for (int i = 0; i < jsonResponse.Count; i++)
        {
            string id_surface = jsonResponse[i].AsObject["id_surface"];
            string libelle = jsonResponse[i].AsObject["libelle"];
            idsAndLibelle.Add(new List<string> { id_surface, libelle });
        }

        // Now actually restore it
        GameObject root = GameObject.Find("Root");
        TilesChoiceMenuScript tcms = null;
        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name == "TileChoiceMenu")
            {
                tcms = go.GetComponent<TilesChoiceMenuScript>();
            }
        }

        foreach (List<string> item in idsAndLibelle)
        {
            Component[] children = root.GetComponentsInChildren(typeof(Transform));
            foreach (Transform tr in children)
            {
                var meta = tr.gameObject.GetComponent<Metadata>();
                if (meta != null)
                {
                    if (meta.GetParameter("Id") == item[0])
                    {
                        tcms.ApplyMaterialToSurface(item[1], tr.gameObject);
                        continue;
                    }
                }
            }
        }
    }
}
