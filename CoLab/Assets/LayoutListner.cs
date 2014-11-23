using UnityEngine;
using System.Collections;

public class LayoutListner : MonoBehaviour {

	public HighScores_cs dbController;
	// Use this for initialization

	void Open (string name) {
		
		gameObject.BroadcastMessage("activate");
		dbController.getLStuff(name,"1",null);
		
	}
	void Start () {
		Open(null);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
