<?php
	function DeleteOld($table){ //Delete all entries from a table
		$sql = "TRUNCATE TABLE $table"; //truncate keeps the structure of the sql table 
		$result = mysql_query($sql) or Die('Query failed: ' . mysql_error());
	}
	function RePopulate($table,$names,$min,$max,$count){ //Repopulate With Dummy Entries
		//Repopulate table with '$count' number of entries
		for($i = 1; $i <= $count; $i++){
			$name = $names[mt_rand(0,Count($names) - 1)]; //Get a random name
			$score = floor(mt_rand($min,$max)); //Get a random score
			$date = strtotime(date("Y-m-d"));
			$sql = "INSERT INTO $table(name,score,date)
			VALUES('$name','$score','$date')";
			$result = mysql_query($sql) or Die('Query failed: ' . mysql_error()); 
		}
		//Now update our table, by ordering it by descending score
		$sql = "ALTER TABLE $table ORDER BY score DESC,id DESC";
		$result = mysql_query($sql) or Die('Query failed: ' . mysql_error()); 
	}
	
	include('ServerConnect.php');
	$connection = Connect();//Attempt to connect to MYSQL server & dataBase
	
	//Get variables from user
	$table = $_POST['table'];
	$mode = $_POST['mode'];
	$userCount = $_POST['count'];
	$minScore = $_POST['min'];
	$maxScore = $_POST['max'];
	
	//Grab all good names we will use
	if($mode == 'Reset'){
		$names[0] = 'Names Array';
		$index = 0;
		$file = fopen("GoodNames.txt","r") or exit("No Such File"); //Open goodnames.txt file
		while(!feof($file)){ //Read each line and Add
			$names[$index] = fgets($file); // add all names to an array
			$index++;
		}
	}
	DeleteOld($table); //Delete all entries from chosen table	
	if($mode == 'Reset'){RePopulate($table,$names,$minScore,$maxScore,$userCount);} //Repopulate the table if we are in reset mode
	echo 'Scores Reset';
?>