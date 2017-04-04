using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PuttingGuideLine : MonoBehaviour {

    public GameObject lineRendererCloneObj = null;
    public Transform lineRendererGroupTrm = null;

    public Vector3 ballPos = new Vector3();
    public List<Vector3> terrainPosList = new List<Vector3>();

    public LayerMask groundLayerMask;

    // 열 갯수(가로)
    public int colCount = 5;
    // 행 갯수(세로)
    public int rowCount = 5;
    // 행, 열 간격
    public float gridSize = 1.0f;
    

    public void CreateGuideLine( Vector3 _ballPos , Vector3 _guideDirVector )
    {
        lineRendererGroupTrm.forward = _guideDirVector;

        this.ballPos = _ballPos;
        Vector3 startPos = ballPos;

        startPos.x = ballPos.x - (colCount * gridSize) / 2.0f;
        startPos.z = ballPos.z - (rowCount * gridSize);

        for (int i = 0; i < rowCount + 1; i++)
        {
            for (int j = 0; j < colCount; ++j)
            {
                Vector3 tempPos = startPos;
                tempPos.x += j * gridSize;
                tempPos.z += i * gridSize;

                float height = 0.0f;

                // 지형 위치를 구했으면
                if (GetTerrainPos(tempPos, ref height) == true)
                {
                    tempPos.y = height;
                    terrainPosList.Add(tempPos);
                }
                else
                {
                    Debug.LogError(" Can't Get Terrain Pos pickPox : " + tempPos);
                }
            }
        }


        for (int i = 0; i < rowCount + 1; ++i)
        {
            for (int j = 0; j < colCount; ++j)
            {
                Vector3 terrainPos1 = terrainPosList[i * colCount + j];
                Vector3 terrainPos2 = terrainPosList[i * colCount + (j + 1)];
                int applyGColor1 = GetGColor(terrainPos1, ballPos);
                int applyGColor2 = GetGColor(terrainPos2, ballPos);

                // 가로 라인 그리기
                Vector3 lineStartPos = startPos;
                lineStartPos.x += j * gridSize;
                lineStartPos.z += i * gridSize;

                Vector3 lineEndPos = lineStartPos;
                lineEndPos.x += gridSize;

                CreateLine(lineStartPos, lineEndPos, applyGColor1, applyGColor2);


                Vector3 terrainPos1_1 = terrainPosList[i * colCount + j];
                Vector3 terrainPos2_2 = terrainPosList[i * colCount + (j + 1)];
                int applyGColor1_1 = GetGColor(terrainPos1, ballPos);
                int applyGColor2_2 = GetGColor(terrainPos2, ballPos);

                // 세로 라인 그리기
                Vector3 lineStartPos2 = startPos;
                lineStartPos2.z += j * gridSize;
                lineStartPos2.x += i * gridSize;

                Vector3 lineEndPos2 = lineStartPos2;
                lineEndPos2.z += gridSize;

                CreateLine(lineStartPos2, lineEndPos2, applyGColor1_1, applyGColor2_2);
            }
        }
    }

    public void DestroyGuideLine()
    {
        for (int i = 0; i < lineRendererGroupTrm.childCount; i++)
        {
            GameObject.Destroy(lineRendererGroupTrm.GetChild(i).gameObject);
        }
    }

    public void CreateLine(Vector3 startPos, Vector3 endPos , int startGColor ,int endGColor)
    {
        GameObject createObj = Instantiate(lineRendererCloneObj) as GameObject; // 형변환

        createObj.transform.parent = lineRendererGroupTrm;

        LineRenderer lineRenderer = createObj.GetComponent<LineRenderer>();

        lineRenderer.SetVertexCount(2);
        
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);

        lineRenderer.SetColors(new Color32(255, (byte)startGColor, 0 , 255)  , new Color32(255, (byte)endGColor, 0 , 255));
    }

    // float으로 리턴하지 않은 것은 성공유무도 필요하니까
    // ref  포인터와 같은 개념 !  
    public bool GetTerrainPos(Vector3 pos , ref float height)
    {
        RaycastHit hit;
        Vector3 pickPos = pos;
        pickPos.y = 1000.0f;

        if (Physics.Raycast(pickPos, Vector3.down, out hit, 100000.0f, groundLayerMask) == true)
        {
            // 레이저를 쏠 위치 , 레이저 방향 , 충돌 정보 , 어느 거리 까지 , 어떤 레이어마스크와 충돌체크 하는지 
            height = hit.point.y;

            return true;
        }
        return false;
    }

    public int GetGColor( Vector3 terrainPos , Vector3 _ballPos )
    {
        float differValue = terrainPos.y - _ballPos.y;

        int applyGColor = 128;

        // 지형이 높을 때
        if (differValue > 0.0f)
        {
            // 정규화된 값 구하기
            float differValue2 = (2.0f - differValue) / 2.0f;
            // 칼라 값 구하기.
            applyGColor = (int)(128.0f * differValue2);
        }
        else
        {
            // 지형이 작았을 때
            // 정규화된 값 구하기
            float differValue2 = (2.0f + differValue) / 2.0f;
            // 칼라 값 구하기.
            applyGColor = (int)(255.0f - (128.0f * differValue2));

            //지형이 낮을수록 differValue의 값이 더욱 - 로 되고 , differValue2

        }

        return applyGColor;
    }
	
	void Update () {
	
	}
}
