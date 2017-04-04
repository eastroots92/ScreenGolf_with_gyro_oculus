using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GamePlayUICOntroller : MonoBehaviour {
    
    public Image ballImage = null;
    public GameObject ballObj = null;
    public Image ball_Line_Image = null;

    public GameObject windArrowObj = null; // 경기장 바람 방향 오브젝트
    public RectTransform uiWindArrowImage = null; // UI 바람 방향 오브젝트
    public Camera mainCam = null; // 경기장 카메라

    public addporce _addporce = null;

    public Text hollValueText = null;
    public Text scoreValueText = null;
    public Text disValueText = null;
    public Text nickNameText = null;

    public bool Check_Setting_Button = false;
    public GameObject Image_Setting = null;
    public GameObject gameobject_Minimap = null;

    public bool Check_Connect_Button = false;
    public Text connectText = null;

    public InputField portInput = null;

    public Text dataText = null;

    public SwingLineCreater swingLineCreater = null;
    public HitWall hitWall = null;

    public Text explainText = null;

    // Use this for initialization
    void Start () {
        dataText.GetComponent<Text>().text = string.Empty;
    }
	
	// Update is called once per frame
	void Update () {
        UpdateUI_Minimap();
        UpdateUI_WindArrow();
        UpdateUI_PlayerInfo();
        UpdateUI_ExplainText();
        if (EbmDataParser.instance != null)
        {
            dataText.GetComponent<Text>().text = EbmDataParser.instance.QuatData.eulerAngles.ToString() + "\n" +
                EbmDataParser.instance.AccData.ToString() + "\n" +
                EbmDataParser.instance.DistData.ToString() + "\n" +
                EbmDataParser.instance.shootPower.ToString();
                //EbmDataParser.instance.differZAngle.ToString();
        }
    }
    
    void UpdateUI_ExplainText()
    {
        if( mainCam != null )
        {
            switch (_addporce.shotState)
            {
                case addporce.ShotState.ConnectCheck:
                    explainText.text = "연결하세요";
                    break;
                case addporce.ShotState.Ready:
                    explainText.text = "준비하세요";
                    break;
                case addporce.ShotState.Shot:
                    explainText.text = "스윙하세요";
                    break;
                case addporce.ShotState.Putting:
                    explainText.text = "퍼팅하세요";
                    break;
            }
        }
    }

    void UpdateUI_PlayerInfo()
    {
        hollValueText.text = GameInfo.Ins.curHollIndex.ToString();
        scoreValueText.text = GameInfo.Ins.swingIndex.ToString();
        disValueText.text = ((int)GameInfo.Ins.curTotalFlyDistance).ToString() + "m";
        nickNameText.text = GameInfo.Ins.localUser.nickName;
    }

    void UpdateUI_WindArrow()
    {
        if ( mainCam != null )
        {
            Vector3 windArrowEulerAngles = windArrowObj.transform.eulerAngles;
            Vector3 mainCamEulerAngles = mainCam.transform.eulerAngles;
            //경기장 바람 방향에서 경기장 카메라의 오일러(Euler)의 y각을 빼줍니다.
            float differAngle = windArrowEulerAngles.y - mainCamEulerAngles.y;

            Vector3 uiWindArrowEulerAngles = uiWindArrowImage.localEulerAngles;
            uiWindArrowEulerAngles.x = 0.0f;
            uiWindArrowEulerAngles.z = -differAngle;
            uiWindArrowEulerAngles.y = 0.0f;
            uiWindArrowImage.localEulerAngles = uiWindArrowEulerAngles;
        }
    }

    void UpdateUI_Minimap()
    {
        if (ballObj != null)
        {
            // 0.0 ~ 1.0 사이 정규화된 값으로 바꿉니다.
            float xNomalize1 = ballObj.transform.position.x / 500.0f;
            float zNomalize1 = ballObj.transform.position.z / 500.0f;

            //실제 UI 위치를 구합니다.
            float uiPosX = xNomalize1 * 100.0f;
            float uiPosY = zNomalize1 * 100.0f;
            Vector3 uiPos = ballImage.rectTransform.localPosition;
            uiPos.x = uiPosX;
            uiPos.y = uiPosY;

            ballImage.rectTransform.localPosition = uiPos;
            
            Vector3 distanceVector = _addporce.GetCurTargetPos() - _addporce.GetBallPos();

            // 볼과 타겟과의 높이 차이
            float differHeight = distanceVector.y;
            // 볼과 타겟 위치와의 거리
            distanceVector.y = 0.0f; // 높이를 0 으로 !! 
            float distance = distanceVector.magnitude;
            // 볼이 타겟을 향하는 방향
            Vector3 targetDir = distanceVector.normalized;
            Vector3 targetDirEuler = Quaternion.LookRotation(targetDir).eulerAngles;

            // ui 방향으로 바꿔줍니다.
            Vector3 targetUIDirEuler = new Vector3();
            targetUIDirEuler.x = 0.0f;
            targetUIDirEuler.z = -targetDirEuler.y;
            targetUIDirEuler.y = 0.0f;

            // 3D 거리를 ui 거리로 바꿔줍니다
            float disNormalize = distance / 500.0f;
            float uiDis = disNormalize * 100.0f;

            Vector3 linePos = ball_Line_Image.rectTransform.localPosition;
            linePos.x = uiPosX;
            linePos.y = uiPosY;
            ball_Line_Image.rectTransform.localPosition = linePos;
            ball_Line_Image.rectTransform.localEulerAngles = targetUIDirEuler;
            Vector3 lineLocalScale = ball_Line_Image.rectTransform.localScale;
            lineLocalScale.y = uiDis;
            ball_Line_Image.rectTransform.localScale = lineLocalScale;


        } 
    }

    public void OnClick_SettingButton()
    {
        if (Check_Setting_Button == false)
        {
            Image_Setting.SetActive(true); // 세팅창 On
            gameobject_Minimap.SetActive(false); // 미니맵 Off
            Check_Setting_Button = true;
        }
        else
        {
            Image_Setting.SetActive(false); // 셋팅창 Off
            gameobject_Minimap.SetActive(true); // 미니맵 On
            Check_Setting_Button = false;
        }
    }

    public void OnClick_Setting_Connect()
    {
        if (Check_Connect_Button == true)
        {
            // Disconnect 클릭
            connectText.text = "Connect";
            //EbmDataParser 의 CloseSerial(); 실행
            EbmDataParser.instance.CloseSerial();

            Check_Connect_Button = false;
        }
        else
        {
            // Connect 클릭
            connectText.text = "Disonnect";
            //EbmDataParser 의 OpenSerial(portNumber); 실행
            string tempComNum = portInput.text.ToString();
            int portNum = int.Parse(tempComNum);
            EbmDataParser.instance.OpenSerial(portNum);

            Check_Connect_Button = true;
        }
    }

    public void OnClick_Reset_Button()
    {
        EbmDataParser.instance.resetflag = true;
        swingLineCreater.LineClear();
        hitWall.isShot = false;
    }

    public void OnButtonDownExit()
    {
        Application.LoadLevel("MainMenuScene");
    }
}
