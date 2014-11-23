<?php
	include('ServerConnect.php');
	$connection = Connect(); //Attempt to Connect to MYSQL Server & DataBase
	
	//Get variables from Unity
	$table = $_POST['table'];
	$number = $_POST['number'];
	$slots = $_POST['slots'];
	$updating = $_POST['updating'];
	$date = strtotime(date("Y-m-d")); //Get Time
	//Security Check
	$hash = $_POST['hash'];
	if(CheckKey($hash,$number) == 'fail'){ //Check if hash is valid
		echo 'Security Failure'; exit;
	}
		//Make sure all input is sql injection safe
	$number =  CleanInput($number);
	$table = CleanInput($table);
	$slots =  CleanInput($slots);
	$updating =  CleanInput($updating);
	//Run Query (inser or update)
	if($updating == 0){ //Free entry So make new row
		$sql = "INSERT INTO $table (number,slots,date)
		VALUES('$number','$slots','$date')";
	}else{ //We are updating a previously registered user
		$sql = "UPDATE $table SET slots = '$slots',
		date = '$date' WHERE number = '$number'";
	}
	$result = mysql_query($sql) or Die('Query failed: ' . mysql_error()); 

	
	echo 'Accepted';
?>