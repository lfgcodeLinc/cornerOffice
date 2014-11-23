using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

[ExecuteInEditMode]
public class HighScores_cs : MonoBehaviour{
	public enum TrackingType{FreeEntry,TrackProgress}
	public TrackingType trackingType = TrackingType.FreeEntry;
	public enum DeveloperType{debug,live}
	public DeveloperType developmentMode = DeveloperType.live;
	
	private string secretKey = "test";
	public string sendScoreURL = "http://YourServer.com/.../SendHighScores.php?";
	public string getScoresURL = "http://YourServer.com/.../GetHighScores.php?";
	public string resetScoresURL = "http://YourServer.com/.../ResetHighScores.php?";
	public string registerUserURL = "http://YourServer.com/.../RegisterUser.php?";
	public string getUserScoreURL = "http://YourServer.com/.../GetUserScore.php?";
	public string DatabaseToolsURL = "http://YourServer.com/.../DatabaseTools.php?";
	//Modules Done
	[System.Serializable]public class Module{
		public bool showStatus = true;
		public bool showScores = true;
		public bool showUserScore = true;
		public bool showSendScore = true;
		public bool showDifficultySwitch = true;
		public bool showScopeSwitch = true;
	}
	public Module displayOptions;
	//Class That contains 
	[System.Serializable]public class Mode{
		public string displayName = "Easy";
		public string playerPrefsName = "Easy";
		public string databaseName = "Easy";
	}
	public Mode[] difficultyModes;
	public int startDifficultyMode = 0;
	int modeIndex = 0;
	//Score Scope
	string scoreScopeText = "Global";//AllTime,Daily
	string currentScope = "AllTime";
	[System.Serializable]public class ScoreScopeSettings{
		public string dailyName = "Daily";
		public string allTimeName = "Global";
	}
	public ScoreScopeSettings scoreScope;
	//Get HighScores Fields(keep the same lenght)
	string[] serverHighScores = new string[0];
	Vector2 scrollView;
	public string scoreType = "Points";
	public int maxHighScores = 100;
	//Send HighScores Fields
	string serverHighScoreName = "";
	public int maxNameCharacters = 20;
	//Reset  HighScores Fields
	public int resetNames = 100;
	public int minResetScore = 100;
	public int maxResetScore = 1000;
	//Bad Names
	public bool blockBadNames = true; //Block names from bas names list
	public TextAsset badNamesList;  //List Of bad names
	//PlayerPrefsName
	public string existingNamePlayerPrefs = "playerName";
	//Rects
	public Rect sendScoresOffsetRect = new Rect(450,150,250,30);
	public Rect getScoresOffsetRect = new Rect(450,100,100,25);
	public Rect scoreScopeOffsetRect = new Rect(300,100,100,25);
	public Rect messageOffsetBox = new Rect(450,-150,250,25);
	public Rect localScoreOffsetRect = new Rect(450,-105,250,25);
	public Rect serverScoreOffsetRect = new Rect(450,-60,250,40);
	public Rect scoresListOffsetRect = new Rect(150,150,380,350);
	Rect baseRect = new Rect(Screen.width * .5F,Screen.height * .5F,100F,100F);
	//status
	int year = 2012,month = 9,day = 1;
	int deleteScore = 0;
	int localScore = 0;
	int serverScore = 0;
	int serverRank = 0;
	string status = ""; //Status of Server Operations
	int runningHsServer = 0; //Are we doing server side operations
	int runningTrack = 0; //Are we seeking player status
	//skin
	public GUISkin skin;
	
	IEnumerator DatabaseTools(int mode,Mode[] tables){
		if(runningHsServer == 1){status = "Still Running"; yield break;}
		runningHsServer = 1; status = "Running";
		//0 = create tables
		WWWForm hsFm = new WWWForm();
		hsFm.AddField("mode",mode);
		foreach(Mode md in tables){
			hsFm.AddField("table",md.databaseName);	
			WWW hs = new WWW(DatabaseToolsURL,hsFm);
			yield return hs;
			if(hs.text.Equals("Created")){status = "Database Created";}
			else{status = "Error Occured";}
			Debug.Log(hs.text);
		}
		//Stop Running
		runningHsServer = 0;
		SynchTable();
	}
	
	IEnumerator DatabaseTools(int mode, string table){
		if(runningHsServer == 1){status = "Still Running"; yield break;}
		runningHsServer = 1; status = "Running";
		/*Modes
		 * 1 = delete by score
		 * 2 = delete by date
		 */
		WWWForm hsFm = new WWWForm();
		hsFm.AddField("mode",mode);
		if(mode == 1){
			hsFm.AddField("table",table);
			hsFm.AddField("score",deleteScore);
		}else if(mode == 2){
			hsFm.AddField("table",table);
			hsFm.AddField("date",year+"/"+month+"/"+day);
		}
		WWW hs = new WWW(DatabaseToolsURL,hsFm);
		yield return hs;
		if(hs.text.Equals("Created")){status = "Database Created";}
		if(hs.text.Equals("Deleted")){status = table +" Entries Cleaned";}
		
		else{status = "Error Occured";}
		Debug.Log(hs.text);
		//Stop Running
		runningHsServer = 0;
		SynchTable();
	}
	
	IEnumerator ResetHighScores(string table,string mode){
		if(runningHsServer == 1){status = "Still Running"; yield break;}
		runningHsServer = 1; status = "Running";
		//
		WWWForm hsFm = new WWWForm();
		hsFm.AddField("table",table);
		hsFm.AddField("mode",mode);
		hsFm.AddField("count",resetNames);
		hsFm.AddField("min",minResetScore);
		hsFm.AddField("max",maxResetScore);
		WWW hs = new WWW(resetScoresURL,hsFm);
		yield return hs;
		Debug.Log(hs.text);
		//Update
		status = hs.text;
		//Running
		runningHsServer = 0;
		SynchTable();
	}
	
	IEnumerator GetUserScore(string table, string name){
		runningTrack = 1;//We are seeking user stats
		//
		serverRank = 0;
		serverScore = 0;
		//Get User Score
		WWWForm hsFm = new WWWForm();
		hsFm.AddField("name",name);
		hsFm.AddField("table",table);
		hsFm.AddField("hash",GetHash(name));
		WWW hs = new WWW(getUserScoreURL,hsFm);
		yield return hs;
		if(hs.text.Contains("User Found")){
			string[] userData = hs.text.Split(':');
			//Process Results
			if(userData[1] != null)serverRank = int.Parse(userData[1]);
			if(userData[2] != null)serverScore = int.Parse(userData[2]);
		}
		else if(hs.text == "Not Found"){
			PlayerPrefs.SetInt("nameRegistered",0);
		}
		runningTrack = 0;

	}
	
	IEnumerator GetHighScores(string table,string scope,int limit){
		if(runningHsServer == 1){status = "Still Running"; yield break;}
		runningHsServer = 1; status = "Running";
		//Get HighScores
		serverHighScores = new string[maxHighScores];
		for(int st = 0;st<serverHighScores.Length;st++){
			serverHighScores[st] = "Loading....";
		}	
		WWWForm hsFm = new WWWForm();
		hsFm.AddField("table",table);
		hsFm.AddField("scope",scope);
		hsFm.AddField("limit",limit);
		hsFm.AddField("hash",GetHash(table));
		WWW hs = new WWW(getScoresURL,hsFm);
		yield return hs;
		
		//if(hs.text.Length > 0){
		//	serverHighScores = hs.text.Split('@');
		//	status = "Found "+table+" HighScores";
		//Debug.Log("Found HighScores: " + scope+" :" +table);
		//}else{
		//	status = "No "+scope+" Scores";
		//	Debug.Log("No "+scope+" Scores");
		//}
		//Stop Running
		runningHsServer = 0;
		//Get User Stats If Tracking is On & We are Registered
		if(trackingType == TrackingType.TrackProgress && PlayerPrefs.GetInt("nameRegistered") == 1){
			StartCoroutine(GetUserScore(difficultyModes[modeIndex].databaseName,serverHighScoreName));
		}
	}

	IEnumerator GetDeskEntry(string dNum, string tNum, GameObject button){
		runningTrack = 1;//We are seeking user stats
		//
		serverRank = 0;
		serverScore = 0;
		//Get User Score
		WWWForm hsFm = new WWWForm();
		hsFm.AddField("Desk_id",dNum);
		hsFm.AddField("table","Reservation");
		hsFm.AddField("Time_slot",tNum);
		WWW hs = new WWW(getUserScoreURL,hsFm);
		yield return hs;

		string res = hs.text.ToString();
		//int numF = int.Parse(res);

		//if(numF > 1)
		//	print ("deactivating button");
		print (hs.text);

		string strs = hs.text;
		strs = strs.Replace("Time_Slot","");
		string[] strA= strs.Split(':');

		for(int i = 0; i < strA.Length  ; i ++) {
			print (strA[i]);
			int outI = 0;

			if(int.TryParse(strA[i],out outI)){
				print (outI+"");
				GameObject hand = GameObject.Find("/Pages/Reserve/" + outI.ToString());
			hand.SendMessage("deactivate");
			}
		}
		if(hs.text.Contains("Array")){
			//print ("FSDGSFGFD");
			//button.SendMessage("deactivate");
		}
		else {
			//print ("NOOO");

		}
		runningTrack = 0;

	}

	IEnumerator GetDeskNow(string dNum, string tNum, GameObject button){
		runningTrack = 1;//We are seeking user stats
		//
		serverRank = 0;
		serverScore = 0;
		//Get User Score
		WWWForm hsFm = new WWWForm();
		hsFm.AddField("Desk_id",dNum);
		hsFm.AddField("table","Reservation");
		hsFm.AddField("Time_slot",tNum);
		WWW hs = new WWW(getUserScoreURL,hsFm);
		yield return hs;
		
		string res = hs.text.ToString();
		//int numF = int.Parse(res);
		
		//if(numF > 1)
		//	print ("deactivating button");
		print (hs.text);
		if(hs.text.Contains("Array")){
			//print ("FSDGSFGFD");
			button.SendMessage("deactivate");
		}
		else {
			//print ("NOOO");
			
		}
		runningTrack = 0;
		
	}
	IEnumerator GetDeskLEntry(string dNum, string tNum, GameObject button){
		runningTrack = 1;//We are seeking user stats
		//
		serverRank = 0;
		serverScore = 0;
		//Get User Score
		string hour = System.DateTime.Now.ToString("hh");
		int h = int.Parse(hour)+1;
		WWWForm hsFm = new WWWForm();
		hsFm.AddField("Desk_id","1");
		hsFm.AddField("table","Reservation");
		hsFm.AddField("Time_slot",h);
		WWW hs = new WWW("http://cs1410.comuv.com/DB/getLayout.php",hsFm);
		yield return hs;
		
		string res = hs.text.ToString();
		//int numF = int.Parse(res);
		
		//if(numF > 1)
		//	print ("deactivating button");
		print (hs.text);
		
		string strs = hs.text;
		strs = strs.Replace("Desk_id","");
		string[] strA= strs.Split(':');
		
		for(int i = 1; i < strA.Length -1 ; i ++) {
//			print (strA[i]);


			GameObject hand = GameObject.Find("/Pages/LayoutPage/Table/" + strA[i].Trim());
			hand.SendMessage("deactivate");
		}
		if(hs.text.Contains("Array")){
			//print ("FSDGSFGFD");
			//button.SendMessage("deactivate");
		}
		else {
			//print ("NOOO");
			
		}
		runningTrack = 0;
		
	}


	public void getLStuff(string dNum, string tNum, GameObject button){
		StartCoroutine(GetDeskLEntry(dNum, tNum,button));
		
	}

	public void getStuff(string dNum, string tNum, GameObject button){
		StartCoroutine(GetDeskEntry(dNum, tNum,button));

	}
	
	IEnumerator SendHighScores(Mode table,string name,int score,Mode[] difficultyModesSet){
		if(runningHsServer == 1){status = "Still Running"; yield break;}
		runningHsServer = 1; status = "Running";
		//Check If We Have Beat Our Own Score First
		if(developmentMode == DeveloperType.live){
			if(PlayerPrefs.GetInt(table.playerPrefsName) <= PlayerPrefs.GetInt("sent"+table.playerPrefsName)){
				status = table.displayName + " Score Previously Submitted"; 
				runningHsServer = 0;
				yield break;		
			}
		}
		//Trim
		if(name.Length > maxNameCharacters){
			runningHsServer = 0;
			status = "Name Too Long"; yield break;	
		}
		//Scan & Check Name
		if(blockBadNames && CheckName(name).CompareTo("offensive") == 0){
			runningHsServer = 0;
			status = "Chosen Name Is Offensive"; yield break;	
		}
		int updating = 0; //0 = no we are making a free entry/1 = updating entry
		int newRegistration = 0; //We are doing a new registration
		if(trackingType == TrackingType.TrackProgress){
			if(PlayerPrefs.GetInt("nameRegistered") == 0){ // We are not yet registred
				newRegistration = 1;
				status = "Registering User";
				string finalResult = "";
				string tables = "";
				for(int m = 0; m < difficultyModesSet.Length; m++){//Create a list of tables to send 
					if(m < difficultyModesSet.Length -1){
						tables += difficultyModesSet[m].databaseName + " ";	
					}else{
						tables += difficultyModesSet[m].databaseName;		
					}
				}
				WWWForm rsFm = new WWWForm();
				rsFm.AddField("name",name);
				rsFm.AddField("tables",tables);
				rsFm.AddField("hash",GetHash(name));
				WWW rs = new WWW(registerUserURL,rsFm);
				yield return rs;
				Debug.Log(rs.text+" : "+table.displayName);
				finalResult = rs.text;
				if(finalResult.Equals("Already Used")){
					runningHsServer = 0;
					status = "Name Already Used"; yield break;
				}else if(finalResult.Equals("Registration Complete")){//We Registered Now Update Score
					PlayerPrefs.SetInt("nameRegistered",1);	
					PlayerPrefs.SetString("registeredName",name);
				}else{
					runningHsServer = 0;
					status = finalResult; yield break;
				}
			}
			updating = 1; //We need to update entry now
		}
		//SEND OR UPDATE SCORE
		status = "Running"; //Run Again
		WWWForm hsFm = new WWWForm();
		hsFm.AddField("table",table.databaseName);
		hsFm.AddField("num",name);
		hsFm.AddField("score",score);
		hsFm.AddField("updating",updating);
		hsFm.AddField("hash",GetHash(name));
		WWW hs = new WWW(sendScoreURL,hsFm);
		yield return hs;
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
		yield return new WaitForSeconds(1); //Wait A Second Before Synch
		SynchTable();
	}



	
	void SynchTable(){//Update
		StartCoroutine(GetHighScores(difficultyModes[modeIndex].databaseName,currentScope,maxHighScores));	
	}
	string CheckName(string usedName){ //Make sure imput name is clean
		string[] names = badNamesList.text.Split('\n'); 
		foreach(string n in names){
			if(usedName.Trim().ToLower().IndexOf(n.Trim().ToLower()) > -1){
				return "offensive";
			}
		}
		return "clean";
	} 
	string GetHash(string usedString){ //Create a Hash to send to server
    	MD5 md5 = MD5.Create();
    	byte[] bytes = Encoding.ASCII.GetBytes(usedString+secretKey);
    	byte[] hash = md5.ComputeHash(bytes);
		
    	StringBuilder sb = new StringBuilder();
    	for(int i = 0; i < hash.Length; i++){
    	    sb.Append(hash[i].ToString("x2"));
    	}
    	return sb.ToString();
	}

	int User_id = 43245;
	int Time_slot = 3;
	string desc = "";

	string sendUrl ="http://cs1410.comuv.com/DB/send.php";
	int num = 119;
	string slots = "111111111111111111";
	IEnumerator SendDesk (){
		print ("sendDesk");
		WWWForm hsFm = new WWWForm();
		hsFm.AddField("table","Reservation");
		hsFm.AddField("User_id",User_id);
		hsFm.AddField("Desk_id",num);
		hsFm.AddField("Time_slot",Time_slot);
		hsFm.AddField("Description",desc);
		int updating = 0;
		hsFm.AddField("updating",updating);
		hsFm.AddField("hash",GetHash(User_id.ToString()));
		WWW hs = new WWW(sendUrl,hsFm);
		yield return hs;
		//		Debug.Log(hs.text+" : "+table.displayName);
		//Process Results
		print (hs.text);

		//Stop Running
		runningHsServer = 0;
		yield return new WaitForSeconds(1); //Wait A Second Before Synch
		//SynchTable();
	}
	void Open (string name) {

		num = int.Parse(name);
	}
	public void SendStuff(int inTNum, int inDNum){
		num = inDNum;
		Time_slot = inTNum;
		StartCoroutine(SendDesk ());
	

	}

}
