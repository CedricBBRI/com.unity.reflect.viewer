<?php

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

$selectTiles = "SELECT id_surface, libelle FROM c" . $clientId . "_p" . $projectId . "_choices INNER JOIN tptiles ON c" . $clientId . "_p" . $projectId . "_choices.id_tile=tptiles.id;";
$result = $bdd->query($selectTiles);

if ($result->errorCode() == 00000) 
{
  echo "Retrieve previous choice of tiles: OK\r\n";
} 
else 
{
  echo "Error while retrieving previous choice of tiles!\r\n";
}

if ($result->rowCount() > 0) {
    echo "RETURNS\r\n";
}

// output data of each row
while($row = $result->fetch(PDO::FETCH_ASSOC)) {
    echo ";" . $row['id_surface'] . "," . $row['libelle'];
} 

//Close the query access
$result->closeCursor();
?>