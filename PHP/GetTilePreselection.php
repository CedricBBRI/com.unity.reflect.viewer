<?php

try
{
	$bdd = new PDO('mysql:host=localhost;dbname=tpdemo;charset=utf8', 'root', '', array(PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION, PDO::MYSQL_ATTR_LOCAL_INFILE => true));
}
catch(Exception $e)
{
	die('Error : ' . $e->getMessage());
}

$selectTiles = "SELECT tptiles.libelle FROM tptiles INNER JOIN preselections ON tptiles.id = preselections.tile_id;";
$result = $bdd->query($selectTiles);

if ($result->errorCode() == 00000) 
{
  echo "Retrieve tiles preselection: OK\r\n";
} 
else 
{
  echo "Error while retrieving tiles preselection!\r\n";
}

if ($result->rowCount() > 0) {
    echo "RETURNS\r\n";
}

// output data of each row
while($row = $result->fetch(PDO::FETCH_ASSOC)) {
    echo ";" . $row['libelle'] . "\r\n";
} 

//Close the query access
$result->closeCursor();
?>