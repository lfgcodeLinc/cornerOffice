<?php
	include('ServerConnect.php');
	$connection = Connect();//Attempt to Connect to MYSQL Server & DataBase
	
	//Get variables from unity
	$name = $_POST['name'];
	$date = strtotime(date("Y-m-d"));
	$tables = $_POST['tables'];
	//Security Check
	$hash = $_POST['hash'];
	if(CheckKey($hash,$name) == 'fail'){ //Check if hash is valid
		echo 'Security Failure'; exit;
	}
		//Make sure all input is sql injection safe
	$name =  CleanInput($name);
	$tables = CleanInput($tables);
	//Split difficulty tables
	$tablesBroken = explode(' ', $tables);
	$arr = Array();
	$i = 0;$x = 0;
	foreach($tablesBroken as $table){ //Check if name is in any of our tables
		$sql = "SELECT * FROM $table WHERE name = '$name'";
		$result = mysql_query($sql) or Die('Query failed: ' . mysql_error()); 
		$count = mysql_num_rows($result);
		//Process Results : Tally up how many times we fing the name 
		if($count > 0){ 
			$arr[$i] = 1;
			$x++;
		}else{
			$arr[$i] = 0;
		}
		$i++;
	}
	if($x == 0){ //Name not in use in any table
		foreach($tablesBroken as $table){//Insert the user in all tables
			$sql = "INSERT INTO $table (name,score,date)
			VALUES('$name',0,'$date')";
			$result = mysql_query($sql) or Die('Query failed: ' . mysql_error()); 
		}echo 'Registration Complete'; exit;
	}else if($x >= Count($tablesBroken)){//Name in use in all tables
		echo 'Already Used'; exit; //exit
	}else{//In use but not complete in all tables
		for($c = 0; $c < Count($tablesBroken); $c++){
			if($arr[$c] == 0){ //Insert name in tables where it is not present
				$toTable = $tablesBroken[$c];
				$sql = "INSERT INTO $toTable(name,score,date)
				VALUES('$name',0,'$date')";
				$result = mysql_query($sql) or Die('Query failed: ' . mysql_error()); 
			}
		}echo 'Registration Complete'; exit;
	}
?>