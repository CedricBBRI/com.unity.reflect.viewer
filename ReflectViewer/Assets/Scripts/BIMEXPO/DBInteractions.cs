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

    

    public void saveComment(string comment, GameObject surface)
    {
        //Create the table
        string createCmd = "CREATE TABLE IF NOT EXISTS c" + clientId + "_p" + projectId + "_comments ( id_surface INT UNSIGNED NOT NULL, comment VARCHAR(200), PRIMARY KEY (id_surface) ) CHARACTER SET 'utf8' ENGINE=INNODB;";
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
