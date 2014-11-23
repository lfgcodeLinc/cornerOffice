<?php
	include('ServerConnect.php');
	$connection = Connect();//Attempt to Connect to MYSQL Server & DataBase
	
	//Get variables from unity
	$table = $_POST['table'];
	$scope = $_POST['scope'];
	$limit = $_POST['limit'];
	//Security Check
	$hash = $_POST['hash'];
	if(CheckKey($hash,$table) == 'fail'){ //Check if hash is valid
		echo 'Security Failure'; exit;
	}
		//Make sure all input is sql injection safe
	$table = CleanInput($table);
	$scope =  CleanInput($scope);
	$limit =  CleanInput($limit);
	//Create a Query For The Right Table
	if($scope == 'AllTime'){ //Get All scores
		$sql = "SELECT * FROM $table LIMIT 0,$limit";
	}else{ //Get scores posted today
		$date = strtotime(date("Y-m-d")); //Today's date
		$sql = "SELECT * FROM $table WHERE date = '$date' LIMIT 0,$limit";
	}
	$result = mysql_query($sql) or Die('Query failed: ' . mysql_error());
	//1.Build a string of all scores to send back
	$info = "";
	while($found = mysql_fetch_array($result)){
		$info = $info .'@'. $found['name'] .':'. $found['score'];
	}
	echo $info;
?>