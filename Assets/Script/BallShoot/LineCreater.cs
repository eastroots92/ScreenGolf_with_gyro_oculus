using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineCreater : MonoBehaviour {

    public LineRenderer lineRenderer;

    public void CreateLine( List<Vector3> ballPoint )
    {
        lineRenderer.SetVertexCount(ballPoint.Count);
        for (int i = 0; i < ballPoint.Count; i++)
        {
            lineRenderer.SetPosition(i,ballPoint[i]);
        }
    }

    public void RemoveLine()
    {
        lineRenderer.SetVertexCount( 0 );
    }
        

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
