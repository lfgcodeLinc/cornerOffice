using UnityEngine;
using System.Collections;

public class ReserveButton : MonoBehaviour {


	public GameObject resevationL;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void OnMouseDown() {
		resevationL.SendMessage("SendReservations");
	}
}
