using UnityEngine;
using System.Collections;

public class ReseervationButton : MonoBehaviour {

	private GameObject Reservation;
	private bool active = true;
	void Start() {
		Reservation = GameObject.FindGameObjectWithTag("Reservation");
		string hour = System.DateTime.Now.ToString("hh");
		if(int.Parse(hour)>int.Parse(name)-1)
			deactivate();
	}
	// Update is called once per frame
	void OnMouseDown () {
		if(active){
			Reservation.SendMessage("AddSlot",int.Parse(this.name));
			deactivate();
		}

	}
	void deactivate(){
		renderer.material.color = Color.red;
		active = false;
	}
	void activate(){
		renderer.material.color = Color.green;
		active = true;
	}
}
