using UnityEngine;
using System.Collections;

public class TimeResManager : MonoBehaviour {


	
	// Update is called once per frame
	void Update () {
	
	}
	public HighScores_cs dbc;
	void GetDBC() {
		GameObject dbco = GameObject.FindGameObjectWithTag("dbc");
		dbc = dbco.GetComponent<HighScores_cs>();
		//		hinge.useSpring = false;
		dbc.getStuff("1","1",null);
	}
	// Use this for initialization
	void Start () {
	//GetDBC();

		
		
	}
	void open ( string n){
		print("open");
		GetDBC();
	}
}
