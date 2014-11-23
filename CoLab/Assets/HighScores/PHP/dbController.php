function redundantPostScore(name : String, score : String, track : String) {
    //This connects to a server side php script that will add the name and score to a MySQL DB.
    
    var gID  = System.DateTime.Now.Second + (System.DateTime.Now.Minute * 100) + (System.DateTime.Now.Hour * 10000)+ (System.DateTime.Now.Day * 1000000);
	
	var level = PlayerPrefs.GetInt("AILevel")+1;
    var hash = Md5.Md5Sum(name + score + track + secretKey); 
 
    var highscore_url = redundantAddScoreUrl + 
    "name=" + WWW.EscapeURL(name) + 
    "&gameID=" + gID + 
    "&gameTime=" + score + 
    "&track=" + track + 
    "&kills=" + kills + 
    "&deaths=" + deaths + 
    "&currency_earned=" + currency + 
    "&level=" + level +
    "&hash=" + hash;
 
    // Post the URL to the site and create a download object to get the result.
    var hs_post = WWW(highscore_url);
    yield hs_post; // Wait until the download is done
    if(hs_post.error) {
        print("There was an error posting the high score: " + hs_post.error);
    }
}