using UnityEngine;
using System.Collections;

public class BackButton : MonoBehaviour {
	public GameObject deskDetails;
	public GameObject layoutRoot;
	// Use this for initialization
	void Awake () {
		deskDetails = GameObject.FindGameObjectWithTag("DeskDetails");
		layoutRoot = GameObject.FindGameObjectWithTag("layoutRoot");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnMouseDown() {
		deskDetails.SetActive(false);

		layoutRoot.SetActive(true);
	}
}
