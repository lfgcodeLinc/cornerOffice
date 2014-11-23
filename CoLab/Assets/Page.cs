using UnityEngine;
using System.Collections;

public class Page : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Close () {
		this.gameObject.SetActive(false);
	}
}
