<?php

	$secretKey = 'test'; //Set your secret key, must match same secret in Highscore script 
		
	$hostName = 'server26.000webhost.com'; //Set your database hostname
	$userName = 'a5678420'; //Set your database user name
	$password = 'inampass1'; //Set your databse password
	$database = 'a5678420_colab'; //Set your actual database name
	
	function Connect(){
		//Set global variables so they can be used in this function
		global $hostName; global $userName;
		global $password; global $database;
		$status = mysql_connect($hostName,$userName,$password); //Attempt to connect to Database
		echo $status;
		if(!$status){echo 'Server Not Found'; exit;} //Hosting Server not found
		if(!@mysql_select_db($database)){echo 'Database Not Found'; exit;} //Database not found
		return $status; //Return link on established connection
	}
	function CheckKey($hash,$name){  //Check weather Md5 hash matches
		global $secretKey; 
		$checkHash = md5($name.$secretKey);
		if(strcmp($checkHash,$hash) == 0){
			return 'pass'; //hash matches
		}else{
			return 'fail'; //hash failed
		}
	}
	function CleanInput($input){ //Sanitize user input
		$cleanInput = mysql_real_escape_string($input); //Use string escape
		if(preg_match('/[^a-z_\-0-9\s]/i',$cleanInput) == 1){ // Using regular expression
			echo 'Bad Input ==> ' . $input; exit; //Failed, exit to prevemt sql injection hacks
		}
		return $cleanInput; //Return clean input
	}
?>