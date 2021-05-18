<?php
$clientId = $_GET["clientId"];
$projectId = $_GET["projectId"];
try
{
	$bdd = new PDO('mysql:host=localhost;dbname=tpdemo;charset=utf8', 'root', '', array(PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION, PDO::MYSQL_ATTR_LOCAL_INFILE => true));
}
catch(Exception $e)
{
	die('Error : ' . $e->getMessage());
}
$query = "SELECT * FROM `c" . $clientId ."_p" . $projectId . "_avenant`;";
$result = $bdd->query($query);
?>

<!DOCTYPE html>
<html>
<head>
	<title>Avenant</title>
</head>
<body>
	<h1>Avenant client <?php echo $clientId; ?>, projet <?php echo $projectId; ?></h1>
	<table style="background-color: burlywood;">
		<tr>
			<th>Etage</th>
			<th>Superficie</th>
			<th>Nom du carrelage</th>
			<th>Commentaires</th>
		</tr>
		<?php while($row = $result->fetch(PDO::FETCH_ASSOC)):;?>
		<tr>
			<td><?php echo $row['level'];?></td>
			<td><?php echo $row['area'];?></td>
			<td><?php echo $row['libelle'];?></td>
			<td><?php echo $row['comment'];?></td>
		</tr>
	<?php endwhile;?>
	</table>

</body>
</html>