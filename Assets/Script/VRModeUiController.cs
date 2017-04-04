using UnityEngine;
using System.Collections;

public class VRModeUiController : MonoBehaviour {

    public GameObject windArrowObj = null; // 경기장 바람 방향 오브젝트
    public RectTransform uiWindArrowImage = null; // UI 바람 방향 오브젝트
    public Camera mainCam = null; // 경기장 카메라

    public Canvas canvas = null;
    public GameObject parentObject = null;
    public bool rodingComplete = false;

    public GameObject uiCamara = null;

    // Use this for initialization
    void Start () {
        canvas.renderMode = RenderMode.WorldSpace;
        
	}
	
	// Update is called once per frame
	void Update () {
        UpdateUI_WindArrow();
        if ( rodingComplete == true )
        {
            rodingComplete = false;
            canvas.transform.parent = parentObject.transform;
            canvas.transform.localPosition = new Vector3( 20.0f, -30.0f, 223.0f);

            uiCamara.SetActive(true);
        }
    }

    void UpdateUI_WindArrow()
    {
        if (mainCam != null)
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

}
