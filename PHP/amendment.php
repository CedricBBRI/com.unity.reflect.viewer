<?php
$clientId = $_GET["clientId"];
$projectId = $_GET["projectId"];
$session = $_GET["session"];

$lastSession = date_create_from_format('Y-m-d H:i:s', $session);
$lastSession->getTimestamp();

try
{
	$bdd = new PDO('mysql:host=localhost;dbname=tpdemo;charset=utf8', 'root', '', array(PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION, PDO::MYSQL_ATTR_LOCAL_INFILE => true));
}
catch(Exception $e)
{
	die('Error : ' . $e->getMessage());
}
$query = "SELECT * FROM `c" . $clientId ."_p" . $projectId . "_avenant` ORDER BY session DESC;";
$result = $bdd->query($query);
?>

<!DOCTYPE html>
<html>
<head>
	<title>Avenant</title>
</head>
<link rel="stylesheet" href="myCSS.css">
<body>
	<h1>Avenant client <?php echo $clientId; ?>, projet <?php echo $projectId; ?></h1>
	<table style="background-color: lightblue; width: 100%">
		<colgroup>
       		<col span="1" style="width: 10%;">
       		<col span="1" style="width: 4%;">
       		<col span="1" style="width: 20%;">
       		<col span="1" style="width: 4%;">
       		<col span="1" style="width: 5%;">
       		<col span="1" style="width: 49%;">
       		<col span="1" style="width: 8%;">
    	</colgroup>
		<tr>
			<th>Etage</th>
			<th>Superficie</th>
			<th>Nom du carrelage</th>
			<th>Prix m²</th>
			<th>Prix surface</th>
			<th>Commentaires</th>
			<th>Session</th>
		</tr>
		<?php while($row = $result->fetch(PDO::FETCH_ASSOC)):;?>
			<?php $thisRowSession = date_create_from_format('Y-m-d H:i:s', $row['session']);
			$thisRowSession->getTimestamp();
			if ($thisRowSession == $lastSession):;?>
				<!--<table style="background-color: green;">-->
				<tr style="background-color: lightgreen;">
					<td><?php echo $row['level'];?></td>
					<td><?php echo $row['area'];?></td>
					<td><?php echo $row['libelle'];?></td>
					<td><?php echo $row['unit_price'];?></td>
					<td><?php echo $row['total_price'];?></td>
					<td><?php echo $row['comment'];?></td>
					<td><?php echo $row['session'];?></td>
				</tr>
			<?php else: ?>
				<tr style="background-color: burlywood;">
					<td><?php echo $row['level'];?></td>
					<td><?php echo $row['area'];?></td>
					<td><?php echo $row['libelle'];?></td>
					<td><?php echo $row['unit_price'];?></td>
					<td><?php echo $row['total_price'];?></td>
					<td><?php echo $row['comment'];?></td>
					<td><?php echo $row['session'];?></td>
				</tr>
			<?php endif;?>
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
			<td>
				<div class="imageContainer">
					<img src=<?php echo "http://bimexpo/screenshots/" . $row['filename'] . " alt=\"" . $row['filename'] . "\"";?>  height="150" width="300">
				</div>
			</td>
			<td><?php echo $row['comment'];?></td>
		</tr>
		<?php endwhile;?>
		<?php $result->closeCursor();?>
	</table>
</body>
</html>