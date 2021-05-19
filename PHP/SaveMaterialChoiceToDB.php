<?php

$surfaceId = $_POST["surfaceId"];
$tileName = $_POST["tileName"];
$clientId = $_POST["clientId"];
$projectId = $_POST["projectId"];

try
{
	$bdd = new PDO('mysql:host=localhost;dbname=tpdemo;charset=utf8', 'root', '', array(PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION, PDO::MYSQL_ATTR_LOCAL_INFILE => true));
}
catch(Exception $e)
{
	die('Error : ' . $e->getMessage());
}

//TO DO : Get the tile id from its name, by using joins in db
// insert (or replace) the line in the table

$choiceInsertion = "REPLACE INTO c" . $clientId . "_p" . $projectId . "_choices (id_surface, id_tile) ";

$choiceInsertion = $choiceInsertion . "SELECT tptiles.id, c" . $clientId . "_p" . $projectId . "_surfaces.id_surface FROM tptiles, c" . $clientId . "_p" . $projectId . "_surfaces ";
$choiceInsertion = $choiceInsertion . "WHERE libelle='" . $tileName . "' AND c" . $clientId . "_p" . $projectId . "_surfaces.id_surface='" . $surfaceId . "';";

//$choiceInsertion = $choiceInsertion . "SELECT c" . $clientId . "_p" . $projectId . "_surfaces.id_surface, tptiles.id ";
//$choiceInsertion = $choiceInsertion . "FROM c" . $clientId . "_p" . $projectId . "_surfaces INNER JOIN tptiles ON c" . $clientId . "_p" . $projectId . "_surfaces.";
$result = $bdd->query($choiceInsertion);

if ($result->errorCode() == 00000) 
{
  echo "User's choices table update: OK\r\n";
  echo "command: " . $choiceInsertion;
} 
else 
{
  echo "Error while updating the user's choices table!\r\n";
}

//Close the query access
$result->closeCursor();
?>