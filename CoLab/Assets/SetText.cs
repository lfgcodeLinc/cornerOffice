using UnityEngine;
using System.Collections;

public class SetText : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void SetTex(string t) {
		GetComponent<TextMesh>().text = t;
	}
}
