<?php
	include('ServerConnect.php');
	$connection = Connect(); //Attempt to Connect to MYSQL Server & DataBase
	
	//Get variables from Unity
	$table = $_POST['table'];
	$User_id = $_POST['User_id'];
	$Desk_id = $_POST['Desk_id'];
	$Time_slot = $_POST['Time_slot'];
	$Description = $_POST['Description'];
	

	
	
	$updating = $_POST['updating'];
	$date = date( 'Y-m-d', time() ); //Get Time
	//Security Check
	$hash = $_POST['hash'];
	//if(CheckKey($hash,$number) == 'fail'){ //Check if hash is valid
	//	echo 'Security Failure'; exit;
	//}
		//Make sure all input is sql injection safe
	// $number =  CleanInput($number);
	// $table = CleanInput($table);
	// $slots =  CleanInput($slots);
	// $updating =  CleanInput($updating);
	//Run Query (inser or update)
	if($updating == 0){ //Free entry So make new row
		$sql = "INSERT INTO $table (User_id,Desk_id,Time_slot,Description,Date)
		VALUES('$User_id','$Desk_id','$Time_slot','$Description','$date')";
	}else{ //We are updating a previously registered user
		//$sql = "UPDATE $table SET slots = '$slots',
		//date = '$date' WHERE number = '$number'";
	}
	$result = mysql_query($sql) or Die('Query failed: ' . mysql_error()); 
	
	//Now update our table, by ordering it by descending slots
	//$sql = "ALTER TABLE $table ORDER BY slots DESC,id DESC";
	$result = mysql_query($sql) or Die('Query failed: ' . mysql_error()); 
	
	echo 'Accepted';
?>