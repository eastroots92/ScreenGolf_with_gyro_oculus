using UnityEngine;
using System.Collections;

public class HitWall : MonoBehaviour {

    public bool isShot = false;

    public addporce porce = null;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnTriggerEnter(Collider collider)
    {
        if (isShot == false)
        {
            isShot = true;
            //Debug.Log("collider : " + collider.gameObject);
            porce.OnHit();
        }
        
    }
}
