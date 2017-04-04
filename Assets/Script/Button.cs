using UnityEngine;
using System.Collections;

public class Button : MonoBehaviour {

    //EbmDataParser ebmData;
    
    // Use this for initialization
    void Start () {
        //ebmData = GameObject.Find("EbmDataParser").GetComponent("EbmDataParser") as EbmDataParser;
    }
	
    public void ButtonDown()
    {
         EbmDataParser.instance.resetflag = true;
        
    }

    void OneTimeReset()
    {
        EbmDataParser.instance.resetflag = false;
    }

	// Update is called once per frame
	void Update () {
	
	}
}
