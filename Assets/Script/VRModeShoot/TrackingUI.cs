using UnityEngine;
using System.Collections;

public class TrackingUI : MonoBehaviour {

    public GameObject oculusCamera = null;

	// Use this for initialization
	void Start () {
	
	}
	
    public void MyPointerDown()
    {
		GetComponent<Renderer> ().material.color = Color.blue;
        EbmDataParser.instance.OpenSerial(3);
    }

	// Update is called once per frame
	void Update () {
        this.transform.position = oculusCamera.transform.position + new Vector3( 35.5f, 19.0f, 47.0f );
	}


}
