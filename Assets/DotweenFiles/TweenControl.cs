using UnityEngine;
using System.Collections;
using DG.Tweening;

public class TweenControl : MonoBehaviour {

    public GameObject target = null;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if( Input.GetKeyDown(KeyCode.P) == true)
        {
            //target.transform.DOMove(new Vector3(33.0f, 12.0f, 80.0f), 2.0f);
            target.transform.DORotate(new Vector3(0.0f, 90.0f, 0.0f), 3.0f);
        }
    }
}
