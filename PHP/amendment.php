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
			<th>Prix m²</th>
			<th>Prix surface</th>
			<th>Commentaires</th>
		</tr>
		<?php while($row = $result->fetch(PDO::FETCH_ASSOC)):;?>
		<tr>
			<td><?php echo $row['level'];?></td>
			<td><?php echo $row['area'];?></td>
			<td><?php echo $row['libelle'];?></td>
			<td><?php echo $row['unit_price'];?></td>
			<td><?php echo $row['total_price'];?></td>
			<td><?php echo $row['comment'];?></td>
		</tr>
	<?php endwhile;?>
	</table>
	<h1>Captures d'écran</h1>
	<?php
	$result->closeCursor();
	$newquery = "SELECT * FROM `c" . $clientId ."_p" . $projectId . "_screenshots` INNER JOIN c" . $clientId . "_p" . $projectId . "_comments ON c" . $clientId ."_p" . $projectId . "_screenshots.id_surface=c" . $clientId . "_p" . $projectId . "_comments.id_surface;";
	$result = $bdd->query($newquery);
	?>
	<table>
		<?php while ($row = $result->fetch(PDO::FETCH_ASSOC)):;?>
		<tr>
			<td><img src=<?php echo "http://bimexpo/screenshots/" . $row['filename'] . " alt=\"" . $row['filename'] . "\"";?>></td>
			<td><?php echo $row['comment'];?></td>
		</tr>
		<?php endwhile;?>
		<?php $result->closeCursor();?>
	</table>
</body>
</html>