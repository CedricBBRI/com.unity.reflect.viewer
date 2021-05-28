<?php

$surface_type = $_POST["surface_type"];
$in_out = $_POST["in_out"];

try
{
	$bdd = new PDO('mysql:host=localhost;dbname=tpdemo;charset=utf8', 'root', '', array(PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION, PDO::MYSQL_ATTR_LOCAL_INFILE => true));
}
catch(Exception $e)
{
	die('Error : ' . $e->getMessage());
}

// Create the table
$tableCreation = "CREATE TABLE IF NOT EXISTS default_materials ( material_name VARCHAR(50) NOT NULL, surface_type VARCHAR(50) NOT NULL, in_out VARCHAR(5) NOT NULL, PRIMARY KEY (surface_type, in_out)) CHARACTER SET 'utf8' ENGINE=INNODB;";
$result = $bdd->query($tableCreation);

if ($result->errorCode() == 00000) 
{
  echo "Default materials table creation: OK\r\n";
} 
else 
{
  echo "Error while creating the default materials table!\r\n";
}
$result->closeCursor();

//Insert comment
$insertCmd = "REPLACE INTO default_materials VALUES ('brick_4', 'mur', 'out');";
$result = $bdd->query($insertCmd);

if ($result->errorCode() == 00000) 
{
  echo "Default materials table insertion: OK\r\n";
} 
else 
{
  echo "Error while inserting the default materials table!\r\n";
}
$result->closeCursor();

// Retrieve default materials
$getCmd = "SELECT material_name FROM default_materials WHERE surface_type='" . $surface_type . "' AND in_out='" . $in_out . "';";
$result = $bdd->query($getCmd);

if ($result->rowCount() > 0) {
	echo "Default material obtention: OK\r\n";
    echo "RETURNS\r\n";
}
else 
{
  echo "Error while obtaining default materials!\r\n";
}

// output data of each row
while($row = $result->fetch(PDO::FETCH_ASSOC)) {
    echo ";" . $row['material_name'];
} 
$result->closeCursor();
?>