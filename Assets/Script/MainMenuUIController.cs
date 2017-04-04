using UnityEngine;
using System.Collections;

public class MainMenuUIController : MonoBehaviour {

    public GameObject[] dontDestroyObjArray = new GameObject[5];
    public GameObject mainMenuPanelObj = null;
    public GameObject gamePlayPanelObj = null;
    public GameObject vrModePlayPanelObj = null;

    public int loadFrame = 0;

    public Camera mainCam = null;

    public Camera uiCam = null;

    public bool isLoadingLevel = false;
    public bool isVRLoadingLevel = false;

    public GamePlayUICOntroller gamePlayUICOntroller = null;
    public VRModeUiController vrModeUiController = null;

    public SoundManager soundManager = null;

    void Awake()
    {
        // 씬이 바꿔도 삭제 되지 않게끔 등록시킵니다.
        for (int i = 0; i < dontDestroyObjArray.Length; i++)
        {
            GameObject.DontDestroyOnLoad(dontDestroyObjArray[i]);
        }
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if(isLoadingLevel == true)
        {
            // 로딩한 후 2프레임 후에 로딩된 씬에 오브젝트에 접근할 수 있다.
            loadFrame++;
            if( loadFrame == 2 )
            {
                isLoadingLevel = false;
                GameObject ballObj = GameObject.Find("/ball1"); // 현재 경로에 있는 ball 찾자 
                gamePlayUICOntroller.ballObj = ballObj;

                GameObject mainCameraObj = GameObject.Find("/Main Camera");

                //Debug.Log("mainCameraObj : " + mainCameraObj);
                this.mainCam = mainCameraObj.GetComponent<Camera>();
                gamePlayUICOntroller.mainCam = mainCameraObj.GetComponent<Camera>();

                GameObject windArrowObj = GameObject.Find("/WindArrowGroup");
                gamePlayUICOntroller.windArrowObj = windArrowObj;
                
                gamePlayUICOntroller._addporce = GameObject.Find("/Player").GetComponent<addporce>();
                gamePlayUICOntroller.mainCam = mainCam;


                gamePlayUICOntroller.swingLineCreater = GameObject.Find("/SwingLineCreater").GetComponent<SwingLineCreater>();
                GameObject golfClub = GameObject.Find("/GolfClub");
                gamePlayUICOntroller.hitWall = golfClub.transform.Find("HitWall").GetComponent<HitWall>();

                // BGM 줄여주기
                soundManager.GameSceneLoad();
            }
        }
        if( isVRLoadingLevel == true )
        {
            // 로딩한 후 2프레임 후에 로딩된 씬에 오브젝트에 접근할 수 있다.
            loadFrame++;
            if (loadFrame == 2)
            {
                isVRLoadingLevel = false;
                //GameObject ballObj = GameObject.Find("/ball1"); // 현재 경로에 있는 ball 찾자 
                //gamePlayUICOntroller.ballObj = ballObj;
                vrModeUiController.rodingComplete = true;

                vrModeUiController.uiCamara = GameObject.Find("/OVRCameraRig/TrackingSpace/LeftEyeAnchor/UICamera") as GameObject;
                vrModeUiController.parentObject = GameObject.Find("/OVRCameraRig/TrackingSpace/LeftEyeAnchor") as GameObject;

                GameObject oculusCameraObj = GameObject.Find("/OVRCameraRig/TrackingSpace/CenterEyeAnchor");
                vrModeUiController.mainCam = oculusCameraObj.GetComponent<Camera>();

                GameObject windArrowObj = GameObject.Find("/WindArrowGroup");
                vrModeUiController.windArrowObj = windArrowObj;

                //Debug.Log("mainCameraObj : " + mainCameraObj);
                //this.mainCam = oculusCameraObj.GetComponent<Camera>();
                //gamePlayUICOntroller.mainCam = oculusCameraObj.GetComponent<Camera>();

                //GameObject windArrowObj = GameObject.Find("/WindArrowGroup");
                //gamePlayUICOntroller.windArrowObj = windArrowObj;

                //gamePlayUICOntroller._addporce = GameObject.Find("/Player").GetComponent<addporce>();
                //gamePlayUICOntroller.mainCam = mainCam;


                //gamePlayUICOntroller.swingLineCreater = GameObject.Find("/SwingLineCreater").GetComponent<SwingLineCreater>();
                //GameObject golfClub = GameObject.Find("/GolfClub");
                //gamePlayUICOntroller.hitWall = golfClub.transform.Find("HitWall").GetComponent<HitWall>();

                // BGM 줄여주기
                //soundManager.GameSceneLoad();
            }
        }
	}

    // 시작 버튼 클릭시 호출됩니다.
    public void OnClick_TutorialStartButton()
    {
        mainMenuPanelObj.SetActive(false);
        gamePlayPanelObj.SetActive(true);
        vrModePlayPanelObj.SetActive(false);
        Application.LoadLevel("ballshoot " + GameInfo.Ins.curHollIndex);

        gamePlayUICOntroller.enabled = true;
        vrModeUiController.enabled = false;

        isLoadingLevel = true; // 일반 모드 활성화
        loadFrame = 0;
    }

    public void OnClick_VRModeStartButton()
    {
        mainMenuPanelObj.SetActive(false);
        gamePlayPanelObj.SetActive(false);
        vrModePlayPanelObj.SetActive(true);
        Application.LoadLevel("Beach2");

        uiCam.GetComponent<AudioListener>().enabled = false; // 오큘러스 안에 리스너 있어서 중복 X 

        vrModeUiController.enabled = true;
        gamePlayUICOntroller.enabled = false;

        isVRLoadingLevel = true; // VR 로딩 활성화
        loadFrame = 0;
    }
}
