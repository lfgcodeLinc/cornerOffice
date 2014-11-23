using UnityEngine;
using System.Collections.Generic;

public class ReservationListener : MonoBehaviour {

	public HighScores_cs dbController;
	List<int> slots;
	// Use this for initialization
	void Start () {
		slots = new List<int>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void AddSlot ( int slot ){
		slots.Add(slot);

	}
	int sName = 0;
	void Open (string name) {

		gameObject.BroadcastMessage("activate");
		dbController.getStuff(name,"1",null);

	}
	void SendReservations (){

		foreach(int slot in slots)
		{
			print (slot + " ");
			dbController.SendStuff(slot,sName);
		}
		slots.Clear();
	}
}
