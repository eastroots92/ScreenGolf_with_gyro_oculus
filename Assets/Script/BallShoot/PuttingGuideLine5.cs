using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PuttingGuideLine5 : MonoBehaviour
{
    [System.Serializable]
    public class GridInfo
    {
        public Vector3 terrainPos = new Vector3();
        public List<LineRenderer> lineRendererList = new List<LineRenderer>(2);
    }

    public GameObject lineRendererCloneObj = null;
    public Transform lineRendererGroupTrm = null;
    public GameObject posDummyCloneObj = null;
    public List<GameObject> posDummyObjList = new List<GameObject>();

    // 열 갯수(가로)
    public int colCount = 5;
    // 행 갯수(세로)
    public int rowCount = 5;
    // 행, 열 간격
    public float gridSize = 1.0f;

    public Vector3 ballPos = new Vector3();
    public List<Vector3> terrainPosList = new List<Vector3>();
    public List<LineRenderer> lineRendererList = new List<LineRenderer>();
    public GameObject testBallObj = null;

    public LayerMask groundLayerMask;

    public ballshoot ball = null;

    void Start()
    {
        //CreateGuideLine(testBallObj.transform.position, this.lineRendererGroupTrm.forward);
    }

    public void CreateGuideLine(Vector3 _ballPos, Vector3 hollPos)
    {
        DestoryGuideLine();

        // 볼 반지름을 빼줍니다.
        _ballPos.y -= ball.radius;

        // 홀 위취와 볼위치로 방향을 구합니다.
        Vector3 targetDistanceVector = hollPos - _ballPos;
        targetDistanceVector.y = 0.0f;
        // 거리를 구합니다.
        float distance = targetDistanceVector.magnitude * 2.0f;
        // 행 갯수를 구합니다.
        this.rowCount = (int)(distance / this.gridSize);


        Vector3 targetDir = targetDistanceVector.normalized;
        // 라인을 그리는 Transform을 위치와 방향을 세팅해 줍니다.
        lineRendererGroupTrm.position = _ballPos;
        lineRendererGroupTrm.forward = targetDir;

        this.ballPos = _ballPos;
        Vector3 startPos = this.ballPos;
        startPos.x = this.ballPos.x - ((colCount * gridSize) / 2.0f);
        startPos.z = this.ballPos.z - (rowCount * gridSize);
        Vector3 startLocalPos = this.ballPos;
        // 로컬 위치 시작 지점을 구합니다. 
        // 월드 좌표가 아니기 때문에 볼 위치에서 더하지 않습니다.
        startLocalPos.x = -((colCount * gridSize) / 2.0f);
        startLocalPos.z = (rowCount * gridSize);
        startLocalPos.y = 0.0f;

        // 그리드 위치 저장하기 시작
        for (int i = 0; i < rowCount + 1; ++i)
        {
            for (int j = 0; j < colCount + 1; ++j)
            {
                Vector3 tempPos = startPos;
                tempPos.x = startPos.x + (j * gridSize);
                tempPos.z = startPos.z - (i * gridSize);

                // 로컬 위치를 구합니다.
                Vector3 tempLocalPos = startPos;
                tempLocalPos.x = startLocalPos.x + (j * gridSize);
                tempLocalPos.z = startLocalPos.z - (i * gridSize);

                GridInfo gridInfo = new GridInfo();

                GameObject createObj = GetPosDummy(i);
                // 위치 더미를 만들어야 하면
                if (createObj == null)
                {
                    // 위치 더미를 만듭니다.
                    createObj = Instantiate(posDummyCloneObj) as GameObject;
                    // 위치 더미를 저장합니다. 그래야 다음에 생성하지 않고 쓸 수 있습니다.
                    posDummyObjList.Add(createObj);
                }
                else
                {
                    createObj = posDummyObjList[i];
                }
                
                    
                Renderer tempRenderer = createObj.GetComponent<Renderer>();
                tempRenderer.enabled = false;

                createObj.transform.parent = lineRendererGroupTrm;
                createObj.transform.localPosition = tempLocalPos;
                Vector3 terrainPickPos = createObj.transform.position;

                float height = 0.0f;
                // 지형 위치를 구했으면
                if (GetTerrainPos(terrainPickPos, ref height) == true)
                {
                    terrainPickPos.y = height;
                }
                else
                {
                    //Debug.LogError("Cant Get Terrain Pos pickPos : " + tempPos);
                }

                terrainPosList.Add(terrainPickPos);

                // 라인이 바닥에 있으면 안보이므로 살짝 올려 줍니다.
                terrainPickPos.y += 0.2f;
                createObj.transform.position = terrainPickPos;
            }
        }
        // 그리드 위치 저장하기 끝

        // 라인 그리기 시작
        for (int i = 0; i < rowCount + 1; ++i)
        {
            for (int j = 0; j < colCount + 1; ++j)
            {
                // 가로 라인 그리기
                if (j < colCount)
                {
                    //Debug.Log("i*colCount + (j+1) : " + (i * (colCount+1) + j));
                    // 라인 두점의 위치를 구합니다.
                    Vector3 terrainPos1 = terrainPosList[i * (colCount+1) + j];
                    Vector3 terrainPos2 = terrainPosList[i * (colCount+1) + (j + 1)];
                    // 라인 두점의 칼라를 구합니다.
                    Color applyGColor1 = GetGColor(terrainPos1, this.ballPos);
                    Color applyGColor2 = GetGColor(terrainPos2, this.ballPos);
                    // 실제 라인을 만듭니다.
                    CreateLine(terrainPos1, terrainPos2, applyGColor1, applyGColor2);
                }

                if (i < rowCount)
                {
                    // 라인 두점의 위치를 구합니다.
                    Vector3 terrainPos1 = terrainPosList[i * (colCount+1) + j];
                    Vector3 terrainPos2 = terrainPosList[(i + 1) * (colCount+1) + j];
                    // 라인 두점의 칼라를 구합니다.
                    Color applyGColor1 = GetGColor(terrainPos1, this.ballPos);
                    Color applyGColor2 = GetGColor(terrainPos2, this.ballPos);
                    // 실제 라인을 만듭니다.
                    CreateLine(terrainPos1, terrainPos2, applyGColor1, applyGColor2);
                }
            }
        }
        // 라인 그리기 끝

    }

    public GameObject GetPosDummy(int index)
    {
        if (index >= posDummyObjList.Count)
        {
            return null;
        }

        return posDummyObjList[index];
    }

    public void DestoryGuideLine()
    {
        terrainPosList.Clear();

        for (int i = 0; i < lineRendererGroupTrm.childCount; ++i)
        {
            LineRenderer lineRenderer = 
                lineRendererGroupTrm.GetChild(i).GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                GameObject.Destroy(lineRendererGroupTrm.GetChild(i).gameObject);
            }
        }
    }


    public void CreateLine(Vector3 startPos, Vector3 endPos, Color startGColor, Color endGColor)
    {
        startPos.y += 0.2f;
        endPos.y += 0.2f;
        GameObject createObj =
            (GameObject)Instantiate(lineRendererCloneObj);
        createObj.transform.parent = lineRendererGroupTrm;

        LineRenderer lineRenderer =
            createObj.GetComponent<LineRenderer>();
        lineRenderer.SetVertexCount(2);

        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);

        lineRenderer.SetColors(startGColor, endGColor);
    }

    public bool GetTerrainPos(Vector3 pos, ref float height)
    {
        RaycastHit hit;
        Vector3 pickPos = pos;
        pickPos.y = 1000.0f;

        if (Physics.Raycast(pickPos, Vector3.down,
            out hit, 100000.0f, groundLayerMask) == true)
        {
            height = hit.point.y;
            return true;
        }

        return false;
    }

    public Color GetGColor(Vector3 terrainPos, Vector3 _ballPos)
    {
        float differValue1 = terrainPos.y - _ballPos.y;
        float differValue2 = terrainPos.y - _ballPos.y;
        float maxDiffer = 0.5f;
        // 지형 값이 더 크면
        if (differValue1 >= 0.0f)
        {
            // 정규화된 값을 구해줍니다.
            // 지형 값이 높으면 red값은 높아지고 green 값은 낮아집니다.
            float applyValue1 = differValue1 / maxDiffer; // red 값(0.0 ~ 1.0)
            float applyValue2 = 1.0f - applyValue1; // green 값(0.0 ~ 1.0)

            //Debug.Log("GetGColor terrainPos.y : " + terrainPos.y +
            //    ", _ballPos.y : " + _ballPos.y + ", differValue : " + differValue1 + ", applyValue1 : " + applyValue1 + ", applyValue2 : " + applyValue2);
            // 칼라 값을 구해 줍니다.
            return new Color(applyValue1, applyValue2, 0.0f);
        }
        else
        {
            // 지형 값이 낮으면 green 값 낮아지고 blue 값은 높아지고
            float applyValue1 = 1.0f - ((maxDiffer + differValue1) / maxDiffer); // blue 값(0.0 ~ 1.0)
            float applyValue2 = (maxDiffer + differValue1) / maxDiffer; // green 값(0.0 ~ 1.0)
            //Debug.Log("GetGColor terrainPos.y : " + terrainPos.y +
            //    ", _ballPos.y : " + _ballPos.y + ", differValue : " + differValue1 + 
            //    ", applyValue1 : " + applyValue1 + ", applyValue2 : " + applyValue2);

            return new Color(0.0f, applyValue2, applyValue1);
        }
    }

    void Update()
    {
    }

}
