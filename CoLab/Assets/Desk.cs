using UnityEngine;
using System.Collections;

public class Desk : MonoBehaviour {

	public GameObject deskDetails;
	public GameObject layoutRoot;
	public GameObject reservation;
	private bool available = true;
	public GameObject lable;
	public bool[] slots;

	public string desc = "";
	// Use this for initialization
	void Awake () {
		deskDetails = GameObject.FindGameObjectWithTag("DeskDetails");
		layoutRoot = GameObject.FindGameObjectWithTag("layoutRoot");
		reservation = GameObject.FindGameObjectWithTag("Reservation");
	}

	void Start () {
		accessDatabase();
		updateDesk();
	}

	void accessDatabase () {
		string hour = System.DateTime.Now.ToString("hh");
		
		//if(int.Parse(hour)>int.Parse(name))

	}

	void updateDesk () {
		string d ="";
		if(!available) {
			deactivate ();
			if(desc.Length > 0)
				d = desc[0].ToString();
		}

		if(lable)
			lable.SendMessage("SetTex",d);
	}

	// Update is called once per frame
	void deactivate () {
		renderer.material.color = Color.red;

	}

	void activate () {
		renderer.material.color = Color.green;
		
	}

	void OnMouseDown() {
		if(available){
			reservation.SetActive(true);
			reservation.SendMessage("Open",name);

		}
		else{
			deskDetails.SetActive(true);
			deskDetails.SendMessage("Open",name);
		}
		layoutRoot.SendMessage("Close");

	}


}
