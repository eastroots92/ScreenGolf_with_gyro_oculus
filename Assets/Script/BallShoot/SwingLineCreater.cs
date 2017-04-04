using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwingLineCreater : MonoBehaviour {

    public List<Vector3> swingPosList = new List<Vector3>();
    public LineRenderer lineRenderer = null;

    public Vector3 prevSwingPos = new Vector3();
    public Transform swingPosDummy = null;
    public float distance = 0.0f;

    // Use this for initialization
    void Start () {
        prevSwingPos = swingPosDummy.position;

    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.S) == true)
        {
            lineRenderer.SetVertexCount(swingPosList.Count);
            for (int i = 0; i < swingPosList.Count; ++i)
            {
                lineRenderer.SetPosition(i, swingPosList[i]);
            }
        }

        distance = (prevSwingPos - swingPosDummy.position).magnitude;

        if ( EbmDataParser.instance.differZAngle > 0.6f)
        {
            swingPosList.Add(swingPosDummy.position);
        }

        prevSwingPos = swingPosDummy.position;
    }

    public void CreateLine()
    {
        lineRenderer.SetVertexCount(swingPosList.Count);
        for (int i = 0; i < swingPosList.Count; ++i)
        {
            lineRenderer.SetPosition(i, swingPosList[i]);
        }
    }

    public void LineClear()
    {
        lineRenderer.SetVertexCount(0);
        swingPosList.Clear();
    }
}
