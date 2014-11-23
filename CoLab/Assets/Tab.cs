using UnityEngine;
using System.Collections;

public class Tab : MonoBehaviour {

	public GameObject Tabs;
	public GameObject MyRoot;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void OnMouseDown () {
		Tabs.BroadcastMessage("CloseThis");
		MyRoot.SetActive(true);
		MyRoot.SendMessage("Open","");
	}
}
