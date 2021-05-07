<?php

//$clientId = $_POST["clientId"];
//$projectId = $_POST["projectId"];

try
{
	$bdd = new PDO('mysql:host=localhost;dbname=tpdemo;charset=utf8', 'root', '', array(PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION, PDO::MYSQL_ATTR_LOCAL_INFILE => true));
}
catch(Exception $e)
{
	die('Error : ' . $e->getMessage());
}

$deleteTable = "DROP TABLE IF EXISTS c" . $clientId . "_p" . $projectId . "_surfaces;";
$createCmd = "CREATE TABLE IF NOT EXISTS c" . $clientId . "_p" . $projectId . "_surfaces ( id_surface SMALLINT UNSIGNED NOT NULL, area VARCHAR(20), level VARCHAR(20), surface_group SMALLINT, PRIMARY KEY (id_surface) ) CHARACTER SET 'utf8' ENGINE=INNODB;";

//Populate the table
//$insertCmd = "INSERT INTO c" . $clientId . "_p" . $projectId . "_surfaces (id_surface, area, level, surface_group) VALUES";
//$count = 0;
// foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
//         {
//             if (go.layer == 6 || go.layer == 7)
//             {
//                 if (count > 0)
//                 {
//                     insertCmd = insertCmd + ", ";
//                 }
//                 insertCmd = insertCmd + "( '" + go.GetComponent<dummyMetadataScript>().ID + "', '" + go.GetComponent<dummyMetadataScript>().room + "', '" + go.GetComponent<dummyMetadataScript>().level + "', NULL)";
//                 count += 1;
//             }
//         }
//

foreach ($_POST["ID"] as $key => $value) {
	echo "PHP " . $value;
}
?>