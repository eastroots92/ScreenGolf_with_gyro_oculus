using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;


public class addporce : MonoBehaviour {
    
    public enum ShotState
    {
        ConnectCheck,
        Ready, 
        Shot,
        Putting,
    }

    public GameObject golfball1;
    public GameObject golfball2;
    public GameObject pos;
    public ballshoot script_ballshoot1;
    public ballshoot script_ballshoot2;
    public LineCreater lineCreater;

    public FollowCamera followCamera;

    public int speed = 5 ;
    public int puttingSpeed = 3;

    Vector3 ballvelocity;

    public Camera shotCam = null;

    public List<Transform> targetList = new List<Transform>();
    public List<GameObject> targetColliderGroupList = new List<GameObject>();

    public LayerMask targetAreadLayerMask;
    public LayerMask hollLayerMask;

    public int curTargetIndex = 0;

    public ShotState shotState = ShotState.Shot;
    public float stateTime = 0.0f;

    public PuttingGuideLine5 puttingGuideLine5 = null;

    // 바람 방향
    public GameObject windArrowGroupObj = null;
    protected float windSpeed = 6.0f;
    public HitWall hitWall = null;

    // 스윙 라인
    public SwingLineCreater swingLineCreator = null;

    public bool checkSwingLine = false;
    public float checkSwingLineTime = 0.0f;
    
    public bool checkStop = false;
    public float checkStopTime = 0.0f;

    public PartsRotation partsRotation = null;
    public GameObject golfClubObj = null;
    public GameObject golfClub = null; // 실제 골프채

    public EffectManager effectManager = null;

    // 이펙트

    public bool checkGroundEffect = false;

    // 오디오 

    public SoundManager soundManager = null;

    public GameObject showShootPowerUI = null;
    
    void Awake()
    {
        GameInfo.Ins.swingIndex = 0;
        GameInfo.Ins.curFlyDistance = 0.0f;
        GameInfo.Ins.curTotalFlyDistance = 0.0f;

        shotState = ShotState.ConnectCheck;
        stateTime = 0.0f;
        soundManager = GameObject.Find("/AudioSourceGroup").GetComponent<SoundManager>();
    }

    // Use this for initialization
    void Start () {
        //golfClubObj.transform.position = script_ballshoot1.transform.position
        //            + new Vector3(150.9249f - 152.3155f, 2.463881f - 0.538027f, 260.5483f - 259.2376f);

        golfClubObj.transform.position = script_ballshoot1.transform.position
                + new Vector3(-0.669f, 3.28367f, 0.522f);

        Vector3 tempEulerAngles = golfClubObj.transform.eulerAngles;  // golfClubObj 각도 담기
        tempEulerAngles.y = followCamera.transform.eulerAngles.y - 90.0f; // tempEulerAngles.y = 카메라.y (카메라 이동이 끝난 시점에 !!!)
        golfClubObj.transform.eulerAngles = tempEulerAngles; // golfClubObj 다시 담기 

    }
    
	// Update is called once per frame
	void Update () {
        if (shotState == ShotState.ConnectCheck)
        {
            hitWall.gameObject.SetActive(false);
            checkSwingLine = false;
            checkSwingLineTime = 0.0f;
            checkStop = false;
            checkStopTime = 0.0f;

            if (EbmDataParser.instance.connectCheck == true)
            {
                shotState = ShotState.Ready;
                stateTime = 0.0f;
                partsRotation.Check_Receive_Data = false;  // 골프채 이동 허용
            }
        }
        else if (shotState == ShotState.Ready)
        {
            // 1.6 초후에 _ 준비 -> 슛 상태 
            if (stateTime >= 1.6f)
            {
                EbmDataParser.instance.resetflag = true; // 준비 상태 일때 센서초기화
                
                shotState = ShotState.Shot; 
                hitWall.gameObject.SetActive(true); // HitWall 활성화
            }
        }

        if (checkSwingLine == true)
        {
            checkSwingLineTime += Time.deltaTime; 
            // HitWall 을 치고 , 1.2초 후에 SwingLine 그려주기
            if (checkSwingLineTime > 1.2f)
            {
                swingLineCreator.CreateLine();
                checkSwingLine = false; 
                checkSwingLineTime = 0.0f; 
                Time.timeScale = 0.01f; // 게임 재생 1/100 으로 줄이기 ! 
                checkStop = true;
                checkStopTime = 0.0f;
                partsRotation.Check_Receive_Data = true; // 라인 그려주고 골프채 이동 멈추기
            }
        }

        if (checkStop == true)
        {
            checkStopTime += Time.unscaledDeltaTime; // 게임 재생속도 줄였으니 !!
            // 2초 동안 SwingLine 보여주고 슛 !
            if (checkStopTime > 2.0f)
            {
                checkStop = false;
                checkStopTime = 0.0f;
                Time.timeScale = 1.0f; // 재생 속도 원 상태로

                FireHit();
            }
        }
        
        /* 마우스 클릭으로 슛 날리는 거
	    if( Input.GetMouseButtonDown(0))
        {
            // < 타겟 지점으로 Shoot 방향 전환 >  
            // targetVector = 현재 타겟 위치 - 골프공 위치
            Vector3 targetVector = targetList[curTargetIndex].position - golfball1.transform.position;
            targetVector.y = 0.0f;
            pos.transform.forward = targetVector.normalized;
            
            if( shotState == ShotState.Shot)
            {
                GameInfo.Ins.swingIndex++;

                // pos.transform.eulerAngles.x 이렇게 쓸 수 없으니  , tempEulerAngles 를 이용한다.
                Vector3 tempEulerAngles = pos.transform.eulerAngles;
                tempEulerAngles.x = -40.0f;
                tempEulerAngles.y += Random.Range(-0.1f, 0.1f); // Shoot 방향 약간 랜덤하게
                pos.transform.eulerAngles = tempEulerAngles;
                
                ballvelocity = pos.transform.forward * speed;

                script_ballshoot1.Shoot(ballvelocity);

                // 바람이 한번만 바뀔 경우
                script_ballshoot1.windVel = windArrowGroupObj.transform.forward * windSpeed;

                shotCam.transform.DOMove(shotCam.transform.position + (-shotCam.transform.forward * 3.0f), 2.0f).OnComplete(OnCompleteBackMove);

            }
            else if( shotState == ShotState.Putting )
            {
                GameInfo.Ins.swingIndex++;

                golfball1.GetComponent<Rigidbody>().isKinematic = false;
                golfball1.GetComponent<Rigidbody>().useGravity = true;

                ballvelocity = pos.transform.forward * puttingSpeed;

                script_ballshoot1.Shoot(ballvelocity);

            }

            for (int i = 0; i < targetColliderGroupList.Count; ++i)
            {
                if (i == curTargetIndex)
                {
                    targetColliderGroupList[i].SetActive(true);
                }
                else
                {
                    targetColliderGroupList[i].SetActive(false);
                }
            }
            
            Debug.Log("Shot dir : " + pos.transform.forward);
            
            //golfball.GetComponent<Rigidbody>().velocity = ballvelocity;
            //golfball.transform.position += ballvelocity; 
        }
        */
        /*
        if(Input.GetMouseButtonDown(1))
        {
            ballvelocity = pos.transform.forward * speed;
            script_ballshoot2.Shoot(ballvelocity);

            for (int i = 0; i < 700; i++)
            {
                script_ballshoot2.Update2(0.05f);
            }
            lineCreater.CreateLine(script_ballshoot2.ballPoint);
        }
        */
        if ( golfball1.GetComponent<ballshoot>().isGroundBound == true )
        {
            // 공이 땅에 닿았을 때 효과.
            if (checkGroundEffect == true)
            {
                effectManager.BallinGround(golfball1.transform.position);
            }
            checkGroundEffect = false;

            if ( CheckBallStop() == true )
            {
                golfball1.GetComponent<Rigidbody>().velocity = Vector3.zero; // 공 멈춤
                golfball1.GetComponent<Rigidbody>().isKinematic = true; // Unity 물리 해제 
                golfball1.GetComponent<ballshoot>().isGroundBound = false;
                //Debug.Log("Ball Stop");
                partsRotation.Check_Receive_Data = false; // 슛을 하고 공이 멈추면 골프채 이동 활성화

                // ShootPower 값 비우기 
                showShootPowerUI.GetComponent<TextMesh>().text = string.Empty;
                
                // 골프채 Obj 공이 날라간 곳으로 이동하기 !  ( 회전은 카메라 이동이 끝난 지점에 )
                //golfClubObj.transform.position = script_ballshoot1.transform.position 
                //    + new Vector3(150.9249f- 152.3155f, 2.463881f - 0.538027f, 260.5483f - 259.2376f);

                golfClubObj.transform.position = script_ballshoot1.transform.position
                + new Vector3(-0.669f, 3.28367f, 0.522f); // -0.669f, 3.28367f, 0.522f  -1.3906f, 2.925854f, 1.3107f

                followCamera.Stop();

                GameInfo.Ins.curTotalFlyDistance += GameInfo.Ins.curFlyDistance;
                
                // 홀 안으로 들어 갔으면
                if ( CheckHoll() == true )
                {
                    GameInfo.Ins.curHollIndex++;

                    // 공이 홀에 들어가면 효과
                    effectManager.BallEnterHoll();
                    //마지막 홀이 끝났으면 결과 표시해 주고 메인 메뉴로 다시 돌아갑니다.
                    if (GameInfo.Ins.curHollIndex > GameInfo.Ins.hollConut)
                    {
                        Debug.Log(" 경기 끝났습니다. ");
                    }
                    else
                    {
                        // 마지막 홀이 아니면 다음 홀 씬을 불러옵니다.
                        Application.LoadLevel("ballshoot " + GameInfo.Ins.curHollIndex);
                        Debug.Log(" 다음 씬으로 넘어갑니다. ");
                    }
                }
                else
                {
                    // 마지막 홀이면 ( 마지막 목표 지점 ! )
                    if (CheckLastHall() == true)
                    {
                        // 마지막 홀일때는 퍼팅 모드로 바꿔준다.
                        if (CheckArriveTargetArea() == true)
                        {
                            Debug.Log("Last Putting State !!!");
                            shotState = ShotState.Putting;
                            Vector3 guideDirVector = targetList[curTargetIndex].position - golfball1.transform.position;
                            puttingGuideLine5.CreateGuideLine(golfball1.transform.position, targetList[curTargetIndex].position);
                        }
                        else // 아니면 거리를 체크해서 일반 모드로 바꿔줍니다.
                        {
                            Debug.Log("Last Shot State !!!");
                            shotState = ShotState.Shot;
                        }

                        // 타겟 뒤에 위치하게 
                        Vector3 disVector2 = targetList[curTargetIndex].position - this.golfball1.transform.position;
                        disVector2.y = 0.0f;
                        // 방향을 구합니다.
                        Vector3 tempTargetDir2 = disVector2.normalized;
                        // 볼 위치를 다음 타겟 방향 뒤로 위치시킵니다.
                        Vector3 targetCamPos = this.golfball1.transform.position + (-tempTargetDir2 * 20.0f) + Vector3.up * 3.0f;
                        shotCam.transform.DOMove(targetCamPos, 2.0f).OnComplete(OnCompleteShotMove);


                        Vector3 disVector = targetList[curTargetIndex].position - targetCamPos;
                        disVector.y = 0.0f;
                        Vector3 tempTargetDir = disVector.normalized;
                        // 오일러각으로 바꾸기
                        Vector3 targetEulerAngel = Quaternion.LookRotation(tempTargetDir).eulerAngles;
                        shotCam.transform.DORotate(targetEulerAngel, 2.0f);
                    }
                    else // 일반 홀이면
                    {
                        shotState = ShotState.Shot;

                        // 다음 타겟으로 카메라를 돌려주고 샷 모드로
                        if (CheckArriveTargetArea() == true)
                        {
                            ++curTargetIndex;
                        }
                        else // 타겟 영역에 도착하지 못했으면 현재 타겟에 샷모드로 바꿔줍니다.
                        {

                        }

                        // 타겟 뒤에 위치하게 
                        Vector3 disVector2 = targetList[curTargetIndex].position - this.golfball1.transform.position;
                        disVector2.y = 0.0f;
                        // 방향을 구합니다.
                        Vector3 tempTargetDir2 = disVector2.normalized;
                        // 볼 위치를 다음 타겟 방향 뒤로 위치시킵니다.
                        Vector3 targetCamPos = this.golfball1.transform.position + (-tempTargetDir2 * 20.0f) + Vector3.up * 3.0f;
                        shotCam.transform.DOMove(targetCamPos, 2.0f).OnComplete(OnCompleteShotMove);
                        
                        Vector3 disVector = targetList[curTargetIndex].position - targetCamPos;
                        disVector.y = 0.0f;
                        Vector3 tempTargetDir = disVector.normalized;
                        // 오일러각으로 바꾸기
                        Vector3 targetEulerAngel = Quaternion.LookRotation(tempTargetDir).eulerAngles;
                        shotCam.transform.DORotate(targetEulerAngel, 2.0f);
                        
                    }
                }
               
            }
        }

        stateTime += Time.deltaTime;
        //if (Input.GetKeyDown(KeyCode.S) == true)
        //{
        //    lineCreater.RemoveLine();
        //}
    }

    public bool CheckArriveTargetArea()
    {
        RaycastHit hit;
        Vector3 pickPos = script_ballshoot1.transform.position + (Vector3.up * 120.0f);
        if (Physics.Raycast(pickPos, Vector3.down, out hit, 10000000.0f, targetAreadLayerMask) == true)
        {
            string colliderGroupName = hit.collider.transform.parent.name;

            // TargetPos_ColiderGroup_@ 의   @ 를 구하는 방법 여러가지 , 그 중에 Substirng 과 Split 을 이용한 방법
            // SubString -> 
            // colliderGroupName.Substring( colliderGroupName.Length - 2 , 1);  // 배열의 인덱스인데 마지막에!! -2 !


            // Split ->

            string[] splitStr = colliderGroupName.Split('_');   // aaa_bbb_ccc_4 가 있다면  splitStr 에는 4개가 담김

            string indexStr = splitStr[splitStr.Length - 1];   // 배열의 인덱스를 생각 ! 

            int hitColliderIndex = int.Parse(indexStr) - 1; // -1 을 해주는 이유는 

            if ( curTargetIndex == hitColliderIndex )
            {
                return true;
            }
            return false;
        }
        else
        {
            return false;
        }
    }

    // CheckLastHall()
    public bool CheckLastHall()
    {
        if( curTargetIndex >= (targetList.Count - 1) )  // index = 0 부터 , Count 는 1부터
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CheckBallStop()
    {
        if(golfball1.GetComponent<Rigidbody>().velocity.magnitude < 0.2f)
        {
            return true;
        }
        return false;
    }

    public bool CheckHoll()
    {
        RaycastHit hit;
        Vector3 pickPos = script_ballshoot1.transform.position + (Vector3.up * 120.0f);
        if (Physics.Raycast(pickPos, Vector3.down, out hit, 10000000.0f, hollLayerMask) == true)
        {
            return true;
        }
        return false;
    }

    public void OnCompleteBackMove()
    {
        followCamera.ShotPos( ballvelocity.normalized ); // normalized 시켜야한다 * speed 했으니 , 아니면 pos.transform.forward
    }


    // 카메라 회전이 끝난 후 골프채 Obj 각도에 맞춰 회전시켜주기 
    public void OnCompleteShotMove()
    {
        Vector3 tempEulerAngles = golfClubObj.transform.eulerAngles;  // golfClubObj 각도 담기
        tempEulerAngles.y = followCamera.transform.eulerAngles.y - 90.0f; // tempEulerAngles.y = 카메라.y (카메라 이동이 끝난 시점에 !!!)
        golfClubObj.transform.eulerAngles = tempEulerAngles; // golfClubObj 다시 담기 

        EbmDataParser.instance.resetflag = true; // bluetooth 초기화
        swingLineCreator.LineClear(); // 스윙 궤적 초기화
        hitWall.isShot = false; // hitWall.isShot 초기화 
    }

    public Vector3 GetBallPos()
    {
        return script_ballshoot1.transform.position;
    }

    public Vector3 GetCurTargetPos()
    {
        return targetList[curTargetIndex].position;
    }

    // 공 날라가는 부분 ! 
    public void FireHit()
    {
        // 공 날라 갈 때 효과 ! 
        effectManager.BallShootEffect(golfball1.transform.position);
        checkGroundEffect = true; // 공이 땅에 닿으면 효과 발생 true !  

        //ShootPowerUI = 공의 위치
        showShootPowerUI.transform.position = golfball1.transform.position + new Vector3(0.0f,2.0f,0.0f);
        // ShootPowerUI = 카메라 방향  
        Vector3 disVector = targetList[curTargetIndex].position - golfball1.transform.position;
        disVector.y = 0.0f;
        Vector3 tempTargetDir = disVector.normalized;
        // 오일러각으로 바꾸기
        showShootPowerUI.transform.eulerAngles = Quaternion.LookRotation(tempTargetDir).eulerAngles;
        // 데이터 입력  ( 공 멈추면 곳에서 값 초기화 )
        showShootPowerUI.GetComponent<TextMesh>().text = EbmDataParser.instance.shootPower.ToString() + "m/s";

        // < 타겟 지점으로 Shoot 방향 전환 >  
        // targetVector = 현재 타겟 위치 - 골프공 위치
        Vector3 targetVector = targetList[curTargetIndex].position - golfball1.transform.position;
        targetVector.y = 0.0f;
        pos.transform.forward = targetVector.normalized;

        if (shotState == ShotState.Shot)
        {
            // 공 날라 갈 때 스윙 소리 !
            soundManager.PlaySwingSound();

            // 스윙 횟수 증가 
            GameInfo.Ins.swingIndex++;

            // pos.transform.eulerAngles.x 이렇게 쓸 수 없으니  , tempEulerAngles 를 이용한다.
            Vector3 tempEulerAngles = pos.transform.eulerAngles;
            tempEulerAngles.x = -40.0f;
            tempEulerAngles.y += Random.Range(-0.1f, 0.1f); // Shoot 방향 약간 랜덤하게
            pos.transform.eulerAngles = tempEulerAngles;
            
            //Debug.Log("shot EbmDataParser.instance.differZAngle : " + EbmDataParser.instance.differZAngle);//.instance.differZAngle);
            ballvelocity = pos.transform.forward * EbmDataParser.instance.shootPower * 10.0f;

            script_ballshoot1.Shoot(ballvelocity);

            // 바람이 한번만 바뀔 경우
            script_ballshoot1.windVel = windArrowGroupObj.transform.forward * windSpeed;

            shotCam.transform.DOMove(shotCam.transform.position + (-shotCam.transform.forward * 3.0f), 2.0f).OnComplete(OnCompleteBackMove);

        }
        else if (shotState == ShotState.Putting)
        {
            // 공 날라 갈 때 스윙 소리 !
            soundManager.PlayPuttingSound();

            // 스윙 횟수 증가
            GameInfo.Ins.swingIndex++;

            golfball1.GetComponent<Rigidbody>().isKinematic = false;
            golfball1.GetComponent<Rigidbody>().useGravity = true;

            //Debug.Log("EbmDataParser.instance.differZAngle : " + EbmDataParser.instance.differZAngle);
            //ballvelocity = pos.transform.forward * EbmDataParser.instance.differZAngle * 15.0f;

            Debug.Log("EbmDataParser.instance.differZAngle : " + EbmDataParser.instance.shootPower);
            ballvelocity = pos.transform.forward * EbmDataParser.instance.shootPower * 30.0f;

            script_ballshoot1.Shoot(ballvelocity);

        }
    }

    // HitWall 을 치면 !  
    public void OnHit()
    {
        checkSwingLine = true;
        checkSwingLineTime = 0.0f;       
    }
}
