using UnityEngine;
using System.Collections;

public class DeskDetails : MonoBehaviour {

	public GameObject title;
	// Use this for initialization
	void Start () {
		gameObject.SetActive(false);
	}


	// Update is called once per frame
	void Open (string name) {
		gameObject.SetActive(true);
		transform.position = Vector3.zero;
		title.SendMessage("SetTex","Desk " + name);

	}

}
