<?php
	include('ServerConnect.php');	
	$connection = Connect();//Attempt to Connect to MYSQL Server & DataBase
	
	//Get variables from unity
	$tNum = $_POST['Time_slot'];
	$name = $_POST['Desk_id'];
	$table = $_POST['table'];
	//Security Check
echo $tNum;
echo $name;
	//Create a Query For The Right Table
	//$sql = "SELECT * FROM $table";
	$sql = "SELECT * FROM $table
                WHERE Desk_id = $name AND Time_slot = $tNum ;";
echo $sql;
//echo $sql;
	$result = mysql_query($sql) or Die('Query failed: ' . mysql_error());
	
	//1.Build a string to send
	$index = 1; //To Find Position/rank of user
	$info = "Not Found";
	while($found = mysql_fetch_array($result)){ //Search for user name & rank
		if($found['Time_slot'] == $Time_slot){
			$info = 'User Found:' . $index . ':' . $found['score'];
			echo $info; exit; //Send & Exit
		}
		$index++;
	}
	echo $index;
//while($row = mysql_fetch_object($result))
    //var_dump($row);
?>