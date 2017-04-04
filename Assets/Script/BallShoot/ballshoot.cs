using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ballshoot : MonoBehaviour {

    public List<Vector3> ballPoint = new List<Vector3>();

    public Vector3 ballvelocity;

    public float radius = 0.5f;
    public LayerMask groundLayerMask;


    bool isUpdate = false;

    float gravity = 0f;

    public bool isGroundBound = false;

    public FollowCamera followCamera = null;

    public Vector3 windVel = new Vector3();

    void Awake()
    {
        // 공의 반지름 자동입력
        this.radius = this.transform.localScale.x / 2.0f;
    }

    // Use this for initialization
    void Start () {
	    
	}

    public void Shoot(Vector3 ballvelocity)
    {
        this.ballvelocity = ballvelocity;
        gravity = -9.81f;

        GameInfo.Ins.curFlyDistance = 0.0f;
        isUpdate = true;
    }

    public void Shoot2(Vector3 ballvelocity)
    {
        this.ballvelocity = ballvelocity;
        gravity = -9.81f;

        GameInfo.Ins.curFlyDistance = 0.0f;
        isUpdate = false;
    }

    
    // Update is called once per frame
    void Update () {
        if ( isUpdate )
        {
            Vector3 pos = transform.position;
            ballvelocity.y += (gravity * Time.deltaTime);

            // 이동량을 구합니다.
            Vector3 movement = ballvelocity * Time.deltaTime;
            // 이동량에 바람 방향을 더해 줍니다.
            movement += windVel * Time.deltaTime;

            // 현재이동거리
            Vector3 tempMovement = movement;
            tempMovement.y = 0.0f;
            GameInfo.Ins.curFlyDistance += tempMovement.magnitude;

            transform.position += movement;
            
            ballPoint.Add(transform.position);
            CheckBallGround();
        }
        followCamera.UpdateEX();
    }

    public void Update2(float deltaTime)
    {
        this.ballvelocity.y += gravity * deltaTime;
        transform.position += this.ballvelocity * deltaTime;
        ballPoint.Add(transform.position);
    }

    public void CheckBallGround()
    {   
        RaycastHit hit;
        Vector3 ballPos = this.transform.position;
        Vector3 pickPos = ballPos;
        //pickPos.y = 1000.0f;

        if (Physics.Raycast(pickPos, this.ballvelocity.normalized , out hit, 100000.0f, groundLayerMask) == true)
        {
            // Physics.Raycast(pickPos, Vector3.down, out hit, 100000.0f, groundLayerMask)
            // 레이저를 쏠 위치 , 레이저 방향 , 충돌 정보 , 어느 거리 까지 , 어떤 레이어마스크와 충돌체크 하는지 
            if ((hit.point.y - (this.transform.position.y - this.radius)) > 0.0f)
            {
                Vector3 tempBallPos = this.transform.position;
                tempBallPos.y = hit.point.y + this.radius;
                this.transform.position = tempBallPos;
                if (GetComponent<Rigidbody>() != null)
                {
                    GetComponent<Rigidbody>().velocity = this.ballvelocity;  // 
                    GetComponent<Rigidbody>().isKinematic = false;   // 물리 적용 
                    GetComponent<Rigidbody>().angularDrag = 10.0f; // 
                    GetComponent<Rigidbody>().drag = 0.6f; // 

                    isGroundBound = true;
                }
                isUpdate = false; // 이때 부터는 Unity 물리 적용 
            }
        }
        else
        {
            if (ballvelocity.y < 0.0f)
            {
                pickPos.y += 1000.0f;
                if (Physics.Raycast(pickPos, Vector3.down, out hit, 100000.0f, groundLayerMask) == true)
                {
                    // Physics.Raycast(pickPos, Vector3.down, out hit, 100000.0f, groundLayerMask)
                    // 레이저를 쏠 위치 , 레이저 방향 , 충돌 정보 , 어느 거리 까지 , 어떤 레이어마스크와 충돌체크 하는지 
                    if ((hit.point.y - (this.transform.position.y - this.radius)) > 0.0f)
                    {
                        Vector3 tempBallPos = this.transform.position;
                        tempBallPos.y = hit.point.y + this.radius;
                        this.transform.position = tempBallPos;
                        if (GetComponent<Rigidbody>() != null)
                        {
                            GetComponent<Rigidbody>().velocity = this.ballvelocity;  // 
                            GetComponent<Rigidbody>().isKinematic = false;   // 물리 적용 
                            GetComponent<Rigidbody>().angularDrag = 10.0f; // 
                            GetComponent<Rigidbody>().drag = 0.6f; // 

                            isGroundBound = true;
                        }
                        isUpdate = false; // 이때 부터는 Unity 물리 적용 
                    }
                }
            }
        }
    }
}
