using UnityEngine;
using System.Collections;

public class PartsRotation : MonoBehaviour {
	
	public int ID=0;
	EbmDataParser ebmData;
	
	Quaternion quat;

    float limit_data = 0.05f;

    public bool Check_Receive_Data = false;
    
	// Use this for initialization
	void Start () {
		ebmData = GameObject.Find("EbmDataParser").GetComponent("EbmDataParser") as EbmDataParser;

    }
	
	// Update is called once per frame
	void FixedUpdate () {

        //quat = ebmData.QuatData;
        //if (ebmData.DistData.z < -limit_data)
        //{
        //    ebmData.DistData.z = -limit_data;
        //}
        //if (ebmData.DistData.z > limit_data)
        //{
        //    ebmData.DistData.z = limit_data;
        //}
        //if (ebmData.DistData.y < -limit_data)
        //{
        //    ebmData.DistData.y = -limit_data;
        //}
        //if (ebmData.DistData.y > limit_data)
        //{
        //    ebmData.DistData.y = limit_data;
        //}
        //if (ebmData.DistData.x < -limit_data)
        //{
        //    ebmData.DistData.x = -limit_data;
        //}
        //if (ebmData.DistData.x > limit_data)
        //{
        //    ebmData.DistData.x = limit_data;
        //}
        if ( Check_Receive_Data == true ) return;

        transform.localRotation = ebmData.QuatData;
        transform.localPosition = ebmData.DistData;
        
    }
}
