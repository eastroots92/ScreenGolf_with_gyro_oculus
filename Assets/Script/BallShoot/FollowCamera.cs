using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {
    public GameObject target;

    public Vector3 backMovement;
    public Vector3 upMovement;

    public bool shootCheck = false;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	public void UpdateEX () {
        if ( shootCheck == true )
        {
            // 최종 타겟 위치 구하기
            Vector3 targetPos = target.transform.position + backMovement + upMovement;
            this.transform.position = targetPos;

            this.transform.LookAt(target.transform);
        }
	}

    
    public void ShotPos(Vector3 targetDir)
    {
        shootCheck = true;   // 밖에서 함수만 건드리자 ; 

        targetDir.y = 0.0f;
        // 얼마나 뒤로 갈건지
        backMovement = ( -targetDir.normalized * 10.0f);
        // 얼마나 위로 갈건지
        upMovement = ( Vector3.up * 3.0f);
    }

    public void Stop()
    {
        this.shootCheck = false;
    }
}
