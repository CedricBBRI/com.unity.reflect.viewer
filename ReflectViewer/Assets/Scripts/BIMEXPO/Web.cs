using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Reflect; //Unity.Reflect has to be added to the asmdef in the current folder
using System.Collections.ObjectModel;

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
    }

    private void Update()
    {
        if (GameObject.Find("Root").transform.childCount > 0 && !buildingTableCreated)
        {
            StartCoroutine(createBuildingTable());
            buildingTableCreated = true;
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
                if (go.name.Contains("Wall") || meta.GetParameter("Category").Contains("Wall") || go.name.Contains("Floor") || meta.GetParameter("Category").Contains("Floor"))
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

    public Texture2D LoadTextureFromDisk(string FilePath)
    {
        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails
        Texture2D Tex2D;
        byte[] FileData;
        string[] filesInDir;

        //Files in the directory
        filesInDir = Directory.GetFiles(FilePath);

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

    public void ProduceAmendment()
    {
        WWWForm form = new WWWForm();
        form.AddField("clientId", clientId);
        form.AddField("projectId", projectId);

        string phpScript = "http://bimexpo/CreateAmendment.php";
        
        using (UnityWebRequest www = UnityWebRequest.Post(phpScript, form))
        {
            www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                // TO DO : Populate html here
                Application.OpenURL("http://bimexpo/amendment.php");
                Debug.Log("Amendment produced!");
            }
        }
    }

}
