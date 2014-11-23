#pragma strict
import System.Security.Cryptography;
import System.Text;

@script ExecuteInEditMode()

enum TrackingType{FreeEntry,TrackProgress}
var trackType : TrackingType = TrackingType.FreeEntry;
enum DeveloperType{debug,live}
var developmentMode : DeveloperType = DeveloperType.live;

private var secretKey : String = "test";
var sendScoreURL : String = "http://YourServer.com/.../SendHighScores.php?";
var getScoresURL : String = "http://YourServer.com/.../GetHighScores.php?";
var resetScoresURL : String = "http://YourServer.com/.../ResetHighScores.php?";
var registerUserURL : String = "http://YourServer.com/.../RegisterUser.php?";
var getUserScoreURL : String = "http://YourServer.com/.../GetUserScore.php?";
var DatabaseToolsURL : String = "http://YourServer.com/.../DatabaseTools.php?";
//Modules Done
class Module{
	var showStatus : boolean = true;
	var showScores : boolean = true;
	var showUserScore : boolean = true;
	var showSendScore : boolean = true;
	var showDifficultySwitch : boolean = true;
	var showScopeSwitch : boolean = true;
}
var displayOptions : Module;
//Class That contains 
class Mode{
	var displayName : String = "Easy";
	var playerPrefsName : String = "Easy";
	var databaseName : String = "Easy";
}
var difficultyModes : Mode[];
var startDifficultyMode : int = 0;
private var modeIndex : int = 0;
//Score Scope
private var scoreScopeText : String = "Global";//AllTime,Daily
private var currentScope : String = "AllTime";
class ScoreScopeSettings{
	var dailyName : String = "Daily";
	var allTimeName : String = "Global";
}
var scoreScope : ScoreScopeSettings;
//Get HighScores Fields(keep the same lenght)
private var serverHighScores : String[] = new String[0];
private var scrollView : Vector2;
var scoreType : String = "Points";
var maxHighScores : int = 100;
//Send HighScores Fields
private var serverHighScoreName : String = "";
var maxNameCharacters : int = 20;
//Reset  HighScores Fields
var resetNames : int = 100;
var minResetScore : int = 100;
var maxResetScore : int = 1000;
//Bad Names
var blockBadNames : boolean = true; //Block names from bas names list
var badNamesList : TextAsset;  //List Of bad names
//PlayerPrefsName
var existingNamePlayerPrefs : String = "playerName";
//Rects
var sendScoresOffsetRect : Rect = new Rect(450,150,250,30);
var getScoresOffsetRect : Rect = new Rect(450,100,100,25);
var scoreScopeOffsetRect : Rect = new Rect(300,100,100,25);
var messageOffsetBox : Rect = new Rect(450,-150,250,25);
var localScoreOffsetRect : Rect = new Rect(450,-105,250,25);
var serverScoreOffsetRect : Rect = new Rect(450,-60,250,40);
var scoresListOffsetRect : Rect = new Rect(150,150,380,350);
private var baseRect : Rect = new Rect(Screen.width * .5F,Screen.height * .5F,100F,100F);
//status
private var year : int = 2012;
private var month : int = 9;
private var day : int = 1;
private var debugScore : int = 0;
private var deleteScore : int = 0;
private var localScore : int = 0;
private var serverScore : int = 0;
private var serverRank : int = 0;
private var status : String = ""; //Status of Server Operations
private var runningHsServer : int = 0; //Are we doing server side operations
private var runningTrack : int = 0; //Are we seeking player status
//skin
var skin : GUISkin;

function DatabaseTools(mode : int ,tables : Mode[]){
	if(runningHsServer == 1){status = "Still Running"; return;}
	runningHsServer = 1; status = "Running";
	//0 = create tables
	var hsFm : WWWForm = new WWWForm();
	hsFm.AddField("mode",mode);
	for(var md : Mode in tables){
		hsFm.AddField("table",md.databaseName);	
		var hs : WWW = new WWW(DatabaseToolsURL,hsFm);
		yield hs;
		if(hs.text.Equals("Created")){status = "Database Created";}
		else{status = "Error Occured";}
		Debug.Log(hs.text);
	}
	//Stop Running
	runningHsServer = 0;
	SynchTable();
}

function DatabaseTools(mode : int,table : String){
	if(runningHsServer == 1){status = "Still Running"; return;}
	runningHsServer = 1; status = "Running";
	//Modes
	// 1 = delete by score
	// 2 = delete by date
	var hsFm : WWWForm = new WWWForm();
	hsFm.AddField("mode",mode);
	if(mode == 1){
		hsFm.AddField("table",table);
		hsFm.AddField("score",deleteScore);
	}else if(mode == 2){
		hsFm.AddField("table",table);
		hsFm.AddField("date",year+"/"+month+"/"+day);
	}
	var hs : WWW = new WWW(DatabaseToolsURL,hsFm);
	yield hs;
	if(hs.text.Equals("Created")){status = "Database Created";}
	if(hs.text.Equals("Deleted")){status = table +" Entries Cleaned";}
	
	else{status = "Error Occured";}
	Debug.Log(hs.text);
	//Stop Running
	runningHsServer = 0;
	SynchTable();
}

function ResetHighScores(table : String,mode : String){
	if(runningHsServer == 1){status = "Still Running"; return;}
	runningHsServer = 1; status = "Running";
	//
	var hsFm : WWWForm = new WWWForm();
	hsFm.AddField("table",table);
	hsFm.AddField("mode",mode);
	hsFm.AddField("count",resetNames);
	hsFm.AddField("min",minResetScore);
	hsFm.AddField("max",maxResetScore);
	var hs : WWW = new WWW(resetScoresURL,hsFm);
	yield hs;
	//Update
	status = hs.text;
	//Running
	runningHsServer = 0;
	SynchTable();
}

function GetUserScore(table : String,name : String){
	runningTrack = 1;//We are seeking user stats
	//
	serverRank = 0;
	serverScore = 0;
	//Get User Score
	var hsFm : WWWForm = new WWWForm();
	hsFm.AddField("name",name);
	hsFm.AddField("table",table);
	hsFm.AddField("hash",GetHash(name));
	var hs : WWW = new WWW(getUserScoreURL,hsFm);
	yield hs;
	Debug.Log(hs.text);
	if(hs.text != "Not Found" && !hs.text.Contains("Query failed")){
		var userData : String[] = hs.text.Split(':'[0]);
		//Process Results
		if(userData[1] != null)serverRank = int.Parse(userData[1]);
		if(userData[2] != null)serverScore = int.Parse(userData[2]);
	}
	else if(hs.text == "Not Found"){
		PlayerPrefs.SetInt("nameRegistered",0);
	}
	runningTrack = 0;
}

function GetHighScores(table : String,scope : String,limit : int){
	if(runningHsServer == 1){status = "Still Running"; return;}
	runningHsServer = 1; status = "Running";
	//Get HighScores
	serverHighScores = new String[maxHighScores];
	for(var st : int = 0;st<serverHighScores.Length;st++){
		serverHighScores[st] = "Loading....";
	}	
	var hsFm : WWWForm = new WWWForm();
	hsFm.AddField("table",table);
	hsFm.AddField("scope",scope);
	hsFm.AddField("limit",limit);
	hsFm.AddField("hash",GetHash(table));
	var hs : WWW = new WWW(getScoresURL,hsFm);
	yield hs;
	
	if(hs.text.Length > 0){
		serverHighScores = hs.text.Split('@'[0]);
		status = "Found "+table+" HighScores";
		Debug.Log("Found HighScores: " + scope+" :" +table);
	}else{
		status = "No "+scope+" Scores";
		Debug.Log("No "+scope+" Scores");
	}
	//Stop Running
	runningHsServer = 0;
	//Get User Stats If Tracking is On & We are Registered
	if(trackType == TrackingType.TrackProgress && PlayerPrefs.GetInt("nameRegistered") == 1){
		StartCoroutine(GetUserScore(difficultyModes[modeIndex].databaseName,serverHighScoreName));
	}
}

function SendHighScores(table : Mode,name : String,score : int,difficultyModesSet : Mode[]){
	if(runningHsServer == 1){status = "Still Running"; return;}
	runningHsServer = 1; status = "Running";
	//Check If We Have Beat Our Own Score First
	if(developmentMode == DeveloperType.live){
		if(PlayerPrefs.GetInt(table.playerPrefsName) <= PlayerPrefs.GetInt("sent"+table.playerPrefsName)){
			status = table.displayName + " Score Previously Submitted"; 
			runningHsServer = 0;
			return;		
		}
	}
	//Trim
	if(name.Length > maxNameCharacters){
		runningHsServer = 0;
		status = "Name Too Long"; return;	
	}
	//Scan & Check Name
	if(blockBadNames && CheckName(name).CompareTo("offensive") == 0){
		runningHsServer = 0;
		status = "Chosen Name Is Offensive"; return;	
	}
	var updating : int = 0; //0 = no we are making a free entry/1 = updating entry
	var newRegistration : int = 0; //We are doing a new registration
	if(trackType == TrackingType.TrackProgress){
		if(PlayerPrefs.GetInt("nameRegistered") == 0){ // We are not yet registred
			newRegistration = 1;
			status = "Registering User";
			var finalResult : String = "";
			var tables : String = "";
			for(var m : int = 0; m < difficultyModesSet.Length; m++){//Create a list of tables to send
				if(m < difficultyModesSet.Length -1){
					tables += difficultyModesSet[m].databaseName + " ";	
				}else{
					tables += difficultyModesSet[m].databaseName;		
				}
			}
			var rsFm : WWWForm = new WWWForm();
			rsFm.AddField("name",name);
			rsFm.AddField("tables",tables);
			rsFm.AddField("hash",GetHash(name));
			var rs : WWW = new WWW(registerUserURL,rsFm);
			yield rs;
			Debug.Log(rs.text+" : "+table.displayName);
			finalResult = rs.text;
			if(finalResult.Equals("Already Used")){
				runningHsServer = 0;
				status = "Name Already Used"; return;
			}else if(finalResult.Equals("Registration Complete")){//We Registered Now Update Score
				PlayerPrefs.SetInt("nameRegistered",1);	
				PlayerPrefs.SetString("registeredName",name);
			}else{
				runningHsServer = 0;
				status = finalResult; return;
			}
		}
		updating = 1; //We need to update entry now
	}
	//SEND OR UPDATE SCORE
	status = "Running"; //Run Again
	var hsFm : WWWForm = new WWWForm();
	hsFm.AddField("table",table.databaseName);
	hsFm.AddField("name",name);
	hsFm.AddField("score",score);
	hsFm.AddField("updating",updating);
	hsFm.AddField("hash",GetHash(name));
	var hs : WWW = new WWW(sendScoreURL,hsFm);
	yield hs;
	Debug.Log(hs.text+" : "+table.displayName);
	//Process Results
	if(hs.text.Contains("Accepted")){
		//Update Score For Anti Spamming
		PlayerPrefs.SetInt("sent"+table.playerPrefsName,PlayerPrefs.GetInt(table.playerPrefsName));
		if(newRegistration == 1){
			status = "Registered & " + table.displayName +" Score Submitted"; 	
		}else{
			status = "New "+ table.displayName +" Score Submitted";
		}
	}
	//Stop Running
	runningHsServer = 0;
	yield new WaitForSeconds(1); //Wait A Second Before Synch
	SynchTable();
}

function SynchTable(){//Update
	StartCoroutine(GetHighScores(difficultyModes[modeIndex].databaseName,currentScope,maxHighScores));	
}
function CheckName(usedName : String) : String{ //Make sure imput name is clean
	var names : String[] = badNamesList.text.Split('\n'[0]); 
	for(var n : String in names){
		if(usedName.Trim().ToLower().IndexOf(n.Trim().ToLower()) > -1){
			return "offensive";
		}
	}
	return "clean";
} 
function GetHash(usedString : String): String{ //Create a Hash to send to server
	var md5 : MD5 = MD5.Create();
	var bytes : byte[] = Encoding.ASCII.GetBytes(usedString+secretKey);
	var hash : byte[] = md5.ComputeHash(bytes);
	
	var sb : String = "";
	for(var i : int = 0; i < hash.Length; i++){
	    sb += hash[i].ToString("x2");
	}
	return sb;
}

function Start(){
	status = "";
	runningHsServer = 0;
	runningTrack = 0;
	StopAllCoroutines();
	//Name Operations
	if(existingNamePlayerPrefs != ""){
		serverHighScoreName = PlayerPrefs.GetString(existingNamePlayerPrefs);
	}
	//Get Base
	startDifficultyMode = Mathf.Clamp(startDifficultyMode,0,difficultyModes.Length-1); //To avoid runtime errors
	modeIndex = startDifficultyMode;
	//Set Initial Settings
	scoreScopeText = scoreScope.allTimeName;
	currentScope = "AllTime";
	//Get Scores
	SynchTable();	
}
	
function OnGUI(){
	if(skin)GUI.skin = skin;
	//Set Base Rect
	baseRect = new Rect(Screen.width * .5F,Screen.height * .5F,100F,100F);
	//Update User Score
	localScore = PlayerPrefs.GetInt(difficultyModes[modeIndex].playerPrefsName);
	//Status Box
	if(displayOptions.showStatus){
		GUI.Box(new Rect(baseRect.x - messageOffsetBox.x,baseRect.y - messageOffsetBox.y,
		messageOffsetBox.width,messageOffsetBox.height),status);
	}
	if(displayOptions.showUserScore){
		//Local Score Box
		GUI.Box(new Rect(baseRect.x - localScoreOffsetRect.x,baseRect.y - localScoreOffsetRect.y,localScoreOffsetRect.width,localScoreOffsetRect.height)
			,"Local "+difficultyModes[modeIndex].displayName+" Score : "+localScore+" "+scoreType);
		if(trackType == TrackingType.TrackProgress){
			if(PlayerPrefs.GetInt("nameRegistered") == 1){
				if(runningTrack == 0){
					GUI.Box(new Rect(baseRect.x - serverScoreOffsetRect.x,baseRect.y - serverScoreOffsetRect.y,serverScoreOffsetRect.width,serverScoreOffsetRect.height)
						,"Global "+difficultyModes[modeIndex].displayName+" Score : "+serverScore+" "+scoreType +"\n"
					+ "Global Rank: "+serverRank);
				}else{
					GUI.Box(new Rect(baseRect.x - serverScoreOffsetRect.x,baseRect.y - serverScoreOffsetRect.y,
						serverScoreOffsetRect.width,serverScoreOffsetRect.height),"Seeking Stats");	
				}
			}else{
				GUI.Box(new Rect(baseRect.x - serverScoreOffsetRect.x,baseRect.y - serverScoreOffsetRect.y,
					serverScoreOffsetRect.width,serverScoreOffsetRect.height),"Send Score To Register");	
			}
		}
	}
	if(displayOptions.showDifficultySwitch){
		//Get & Show High Scores
		if(GUI.Button(new Rect(baseRect.x - getScoresOffsetRect.x,baseRect.y - getScoresOffsetRect.y,
			getScoresOffsetRect.width,getScoresOffsetRect.height),""+difficultyModes[modeIndex].displayName)){
			if(modeIndex < difficultyModes.Length - 1){
				modeIndex++;
			}else{modeIndex = 0;}
			//Update	
			StartCoroutine(GetHighScores(difficultyModes[modeIndex].databaseName,currentScope,maxHighScores));
			localScore = PlayerPrefs.GetInt(difficultyModes[modeIndex].playerPrefsName);
		}
	}
	if(displayOptions.showScopeSwitch){
		//Set Scope
		if(GUI.Button(new Rect(baseRect.x - scoreScopeOffsetRect.x,baseRect.y - scoreScopeOffsetRect.y,
			scoreScopeOffsetRect.width,scoreScopeOffsetRect.height),""+scoreScopeText)){
			if(scoreScopeText.Equals(scoreScope.allTimeName)){
				scoreScopeText = scoreScope.dailyName;
				currentScope = "Daily";
			}else{
				scoreScopeText = scoreScope.allTimeName;
				currentScope = "AllTime";
			}
			
			StartCoroutine(GetHighScores(difficultyModes[modeIndex].databaseName,currentScope,maxHighScores));
		}
	}
	if(displayOptions.showSendScore){
		//Send Scores
		GUILayout.BeginArea(new Rect(baseRect.x - sendScoresOffsetRect.x,baseRect.y - sendScoresOffsetRect.y,
			sendScoresOffsetRect.width,sendScoresOffsetRect.height));
		GUILayout.BeginHorizontal();
		if(trackType == TrackingType.TrackProgress && PlayerPrefs.GetInt("nameRegistered") == 1){
			serverHighScoreName = PlayerPrefs.GetString("registeredName");
			GUILayout.Box(serverHighScoreName,GUILayout.Width(150),GUILayout.Height(30));
		}else{
			serverHighScoreName = GUILayout.TextField(serverHighScoreName,GUILayout.Width(150));	
		}
		if(GUILayout.Button("Send")){
			StartCoroutine(SendHighScores(difficultyModes[modeIndex],serverHighScoreName,localScore,difficultyModes));
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
	//Display Scores
	if(displayOptions.showScores){
		GUILayout.BeginArea(new Rect(baseRect.x - scoresListOffsetRect.x,baseRect.y - scoresListOffsetRect.y,
			scoresListOffsetRect.width,scoresListOffsetRect.height));
		scrollView = GUILayout.BeginScrollView(scrollView);
			for(var x : int = 1;x<serverHighScores.Length;x++){
				if(x > maxHighScores){break;}
				if(serverHighScores[x] != null){
					var score : String[] = serverHighScores[x].Split(':'[0]); //Split the Score From PHP set Up
					if(score.Length > 1){
						GUILayout.BeginHorizontal();
						GUILayout.Label(x.ToString()+": ");
						GUILayout.Space(10);
						GUILayout.Label(score[0],GUILayout.Width(200),GUILayout.Height(30));
						GUILayout.FlexibleSpace();
						GUILayout.Label(score[1]+" "+scoreType,GUILayout.Width(150),GUILayout.Height(30));
						GUILayout.EndHorizontal();
					}
				}else{
					GUILayout.Label(serverHighScores[x] +" "+scoreType,GUILayout.Width(100));
				}
			}
		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}
	//Editor Tools
	if(developmentMode == DeveloperType.debug){
		EditorTools();
	}
}
	
function EditorTools(){
	GUILayout.BeginHorizontal();
	GUILayout.Label("",GUILayout.Width(Screen.width - 230));
	GUILayout.FlexibleSpace();
	GUILayout.BeginVertical();
	GUILayout.Space(100);
	GUILayout.Box("CREATE",GUILayout.Width(200));
	if(GUILayout.Button("Create Tables",GUILayout.Width(200))){
		StartCoroutine(DatabaseTools(0,difficultyModes));
	}
	GUILayout.Box("MANAGE",GUILayout.Width(200));
	if(GUILayout.Button("Reset All "+difficultyModes[modeIndex].displayName,GUILayout.Width(200))){
		StartCoroutine(ResetHighScores(difficultyModes[modeIndex].databaseName,"Reset"));	
	}
	if(GUILayout.Button("Delete All "+difficultyModes[modeIndex].displayName,GUILayout.Width(200))){
		StartCoroutine(ResetHighScores(difficultyModes[modeIndex].databaseName,"Delete"));
	}
	if(GUILayout.Button("Delete Below Score: " +difficultyModes[modeIndex].displayName ,GUILayout.Width(200))){
		StartCoroutine(DatabaseTools(1,difficultyModes[modeIndex].databaseName));	
	}
	GUILayout.BeginHorizontal();
	GUILayout.Box(""+deleteScore,GUILayout.Width(50));
	deleteScore = Mathf.CeilToInt(GUILayout.HorizontalSlider(deleteScore,1,maxResetScore,GUILayout.Width(150)));
	GUILayout.EndHorizontal();
	//
	if(GUILayout.Button("Delete Below Date: "+difficultyModes[modeIndex].displayName,GUILayout.Width(200))){
		StartCoroutine(DatabaseTools(2,difficultyModes[modeIndex].databaseName));	
	}
	GUILayout.BeginHorizontal();
	GUILayout.Label(""+year,GUILayout.Width(30));
	year = Mathf.CeilToInt(GUILayout.HorizontalSlider(year,2012,2013,GUILayout.Width(30)));
	GUILayout.Label(""+month,GUILayout.Width(20));
	month = Mathf.CeilToInt(GUILayout.HorizontalSlider(month,1,12,GUILayout.Width(40)));
	GUILayout.Label(""+day,GUILayout.Width(20));
	day = Mathf.CeilToInt(GUILayout.HorizontalSlider(day,1,31,GUILayout.Width(40)));
	GUILayout.EndHorizontal();
	GUILayout.Box("TEST DATA",GUILayout.Width(200));
	if(GUILayout.Button("Null Registration",GUILayout.Width(200))){
		PlayerPrefs.SetInt("nameRegistered",0);	
		PlayerPrefs.SetString("registeredName","");
	}
	//
	GUILayout.BeginHorizontal();
	GUILayout.Box("Score: "+PlayerPrefs.GetInt(difficultyModes[modeIndex].playerPrefsName),GUILayout.Width(100));
	PlayerPrefs.SetInt(difficultyModes[modeIndex].playerPrefsName,Mathf.CeilToInt(GUILayout.HorizontalSlider(PlayerPrefs.GetInt(difficultyModes[modeIndex].playerPrefsName),1,maxResetScore,GUILayout.Width(100))));
	GUILayout.EndHorizontal();
	//
	GUILayout.EndVertical();
	GUILayout.EndHorizontal();
}

