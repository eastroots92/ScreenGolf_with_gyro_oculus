using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIScript : MonoBehaviour {

    public Text dataText;

    // Use this for initialization
    void Start () {
        dataText.GetComponent<Text>().text = string.Empty;
    }
	
	// Update is called once per frame
	void Update () {
        dataText.GetComponent<Text>().text = EbmDataParser.instance.OriginalData;
    }
}
