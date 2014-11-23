<?php
	include('ServerConnect.php');
	$connection = Connect();//Attempt to Connect to MYSQL Server & DataBase
	//Get variables from Unity
	$mode = $_POST['mode'];
	$table = $_POST['table'];
	if($mode == 0){ //Create current table
		$sql = "CREATE TABLE $table(id INT(10) AUTO_INCREMENT,
		PRIMARY KEY(id),
		name VARCHAR(25),
		score INT(10),
		date VARCHAR(25))"; //we keep date a varchar in the database
		$result = mysql_query($sql) or Die('Query failed: ' . mysql_error()); 
		echo $table.' Created'; exit;
	}else if ($mode == 1){
		//Delete scores below input score
		$score = $_POST['score'];
	    $sql = "DELETE FROM $table WHERE score < '$score'";
		$result = mysql_query($sql) or Die('Query failed: ' . mysql_error()); 	
		echo 'Deleted'; exit;
	}else if ($mode == 2){
		//Delete scores below posted date
		$date = strtotime($_POST['date']);
		$sql = "DELETE FROM $table WHERE date < '$date'";
		$result = mysql_query($sql) or Die('Query failed: ' . mysql_error()); 
		echo 'Deleted'; exit;
	}
?>