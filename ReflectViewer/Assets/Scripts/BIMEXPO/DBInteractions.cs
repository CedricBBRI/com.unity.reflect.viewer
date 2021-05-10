using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
//using System.Windows.Forms;
//using MySql.Data.MySqlClient;
using UnityEngine.UIElements;

public class DBInteractions : MonoBehaviour
{
    [Header("DATABASE")]
    public string host;
    public string database, username, password, tilesTable;
    [Header("PROJECT DETAILS")]
    public string clientId;
    public string projectId;

    //private MySqlConnection con;

/*
    /// <summary>
    /// Given a tile name ('libelle'), finds the path to its texture, which is located in the table 'chemin_texture' column.
    /// For the moment this path is simply the name of the folder in which the textures are stored for a given tile.
    /// </summary>
    /// <param name="name">The name of the tile (i.e. the 'libelle').</param>
    /// <returns>The path to the texture, as stored in the table.</returns>
    public string GetTexturePathFromName(string name)
    {
        string data = null;
        try
        {
            Connect_DB();
            MySqlCommand cmdSql = new MySqlCommand("SELECT * FROM `" + tilesTable + "` WHERE `libelle`='" + name + "'", con);
            MySqlDataReader myReader = cmdSql.ExecuteReader();
            while (myReader.Read())
            {
                data = myReader["chemin_texture"].ToString();
            }
            if (data == null)
                Debug.Log("Image not found!");
            myReader.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        return data;
    }

    /// <summary>
    /// Given a tile name ('libelle'), finds the corresponding tile id, which is located in the table 'id' column.
    /// </summary>
    /// <param name="name">The name of the tile (i.e. the 'libelle').</param>
    /// <returns>The id of the tile, as stored in the table.</returns>
    public string GetTileIdFromName(string name)
    {
        string data = null;
        try
        {
            Connect_DB();
            MySqlCommand cmdSql = new MySqlCommand("SELECT id FROM `" + tilesTable + "` WHERE `libelle`='" + name + "'", con);
            MySqlDataReader myReader = cmdSql.ExecuteReader();
            while (myReader.Read())
            {
                data = myReader["id"].ToString();
            }
            if (data == null)
                Debug.Log("Id not found!");
            myReader.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        return data;
    }


    public void changeMaterial(GameObject obj, string texturePath)
    {
        //Create a new Material with standard shader
        Material newMat = new Material(Shader.Find("Standard"));

        //Fetch the texture from disk
        Texture newTexture = (Texture)LoadTextureFromDisk(texturePath);

        //Assign texture to material
        newMat.mainTexture = newTexture;

        //Assign the newly created Material onto the object
        obj.GetComponent<Renderer>().material = newMat;

        //StartCoroutine(GetTextureFromPC(url,obj));
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

    /// <summary>
    /// Function <c>ListAllTileNamesInDB</c> lists all the tiles names ('libelle') present in the tiles table in the DB.
    /// </summary>
    /// <returns>A List of strings of all the 'libelle'.</returns>
    public List<string> ListAllTileNamesInDB()
    {
        string data = null;
        List<string> libelles = new List<string>();
        try
        {
            Connect_DB();
            MySqlCommand cmdSql = new MySqlCommand("SELECT `libelle` FROM `" + tilesTable + "` WHERE `libelle` IS NOT NULL", con);
            MySqlDataReader myReader = cmdSql.ExecuteReader();
            while (myReader.Read())
            {
                data = myReader["libelle"].ToString();
                if (data == null)
                    Debug.Log("Entry not found!");
                else
                {
                    libelles.Add(data);
                }
            }
            myReader.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        return libelles;
    }

    public void SaveUserChoiceToDB(string tileId, string surfaceId)
    {
        //Create the user's choices DB - Using REPLACE to authorize the overwriting (if client changes its mind)
        string insertCmd = "REPLACE INTO c" + clientId + "_p" + projectId + "_choices VALUES ( " + surfaceId + ", " + tileId + ")";
        try
        {
            MySqlCommand cmdSql = new MySqlCommand(insertCmd, con);
            cmdSql.ExecuteNonQuery();
            con.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    public void produceAvenant()
    {
        //Recuperate the list of selected tiles - from DB
        try
        {
            //Create Avenant table
            Connect_DB();
            string avenantTable = "c" + clientId + "_p" + projectId + "_avenant";
            string deleteTable = "DROP TABLE IF EXISTS " + avenantTable + ";";
            string createTable = "CREATE TABLE IF NOT EXISTS " + avenantTable + " ( surface_id SMALLINT UNSIGNED NOT NULL, level TINYINT, room VARCHAR(20), libelle VARCHAR(100), comment VARCHAR(200), PRIMARY KEY (surface_id) ) CHARACTER SET 'utf8' ENGINE=INNODB;";
            MySqlCommand createAvenantTable = new MySqlCommand(deleteTable + createTable, con);
            MySqlDataReader myReader = createAvenantTable.ExecuteReader();
            myReader.Close();

            //Make the join
            Connect_DB();
            string surfacesTable = "c" + clientId + "_p" + projectId + "_surfaces";
            string choicesTable = "c" + clientId + "_p" + projectId + "_choices";
            string commentsTable = "c" + clientId + "_p" + projectId + "_comments";
            
            string cmd = "INSERT INTO " + avenantTable + " SELECT " + surfacesTable + ".id_surface, " + surfacesTable + ".level, " + surfacesTable + ".room_name, " + tilesTable + ".libelle, " + commentsTable + ".comment";
            cmd = cmd + " FROM " + surfacesTable;
            cmd = cmd + " INNER JOIN " + choicesTable + " ON " + surfacesTable + ".id_surface = " + choicesTable + ".id_surface";
            cmd = cmd + " INNER JOIN " + tilesTable + " ON " + choicesTable + ".id_tile = " + tilesTable + ".id";
            cmd = cmd + " INNER JOIN " + commentsTable + " ON " + commentsTable + ".id_surface = " + surfacesTable + ".id_surface;";

            MySqlCommand joinTables = new MySqlCommand(cmd, con);
            myReader = joinTables.ExecuteReader();

            while (myReader.Read())
            {
                Debug.Log(myReader["level"]);
            }
            myReader.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        
    }

    public void ValidatePreSelection()
    {
        //Save preselection into DB - establish connection
        try
        {
            Connect_DB();
            //For the moment, I automatically drop the table to avoid it continuously growing from test to test...
            MySqlCommand cmdSqlDropTable = new MySqlCommand("DROP TABLE IF EXISTS preselections;", con);
            MySqlDataReader myReader0 = cmdSqlDropTable.ExecuteReader();
            myReader0.Close();

            //Then recreate it
            MySqlCommand cmdSqlCreateTable = new MySqlCommand("CREATE TABLE IF NOT EXISTS preselections ( client_id MEDIUMINT UNSIGNED NOT NULL, project_id MEDIUMINT UNSIGNED NOT NULL, tile_id MEDIUMINT UNSIGNED NOT NULL, PRIMARY KEY (client_id, project_id, tile_id)) CHARACTER SET 'utf8' ENGINE=INNODB;", con);
            MySqlDataReader myReader = cmdSqlCreateTable.ExecuteReader();
            myReader.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        //Save preselection into DB - part 1: save the DB
        MySqlCommand cmdSql = new MySqlCommand("INSERT INTO preselections VALUES ", con);
        MySqlCommand getTileIdCmd = new MySqlCommand("", con);
        int count = 0;
        foreach (string tile in GameObject.Find("PreselectionMenu").GetComponent<PreselectionMenuScript>().selectedTiles)
        {
            string tileId = "-1";
            if (count > 0)
            {
                cmdSql.CommandText += ",";
            }

            getTileIdCmd.CommandText = "SELECT id FROM " + tilesTable + " WHERE libelle='" + tile + "';";
            try
            {
                Connect_DB();
                MySqlDataReader myReader = getTileIdCmd.ExecuteReader();
                while (myReader.Read())
                    tileId = myReader["id"].ToString();
                myReader.Close();
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }
            cmdSql.CommandText = cmdSql.CommandText + "( " + clientId + ", " + projectId + ", " + tileId + ")";
            count += 1;
        }
        cmdSql.CommandText += ";";
        try
        {
            Connect_DB();
            int affectedRows = cmdSql.ExecuteNonQuery();
            Debug.Log(affectedRows + " affected rows");
            con.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }

        //Hide preselection menu
        GameObject.Find("PreselectionMenu").SetActive(false);

        //Reactivate player camera rotation
        GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>().cameraCanMove = true;
    }

    public void saveComment(string comment, GameObject surface)
    {
        //Create the table
        string createCmd = "CREATE TABLE IF NOT EXISTS c" + clientId + "_p" + projectId + "_comments ( id_surface SMALLINT UNSIGNED NOT NULL, comment VARCHAR(200), PRIMARY KEY (id_surface) ) CHARACTER SET 'utf8' ENGINE=INNODB;";
        try
        {
            MySqlCommand cmdSql = new MySqlCommand(createCmd, con);
            MySqlDataReader myReader = cmdSql.ExecuteReader();
            myReader.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        //Insert comment
        string insertCmd = "REPLACE INTO c" + clientId + "_p" + projectId + "_comments (id_surface, comment) VALUES ( '" + surface.GetComponent<dummyMetadataScript>().ID + "', '" + comment + "');";
        try
        {
            MySqlCommand cmdSql = new MySqlCommand(insertCmd, con);
            MySqlDataReader myReader = cmdSql.ExecuteReader();
            myReader.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    */
}
