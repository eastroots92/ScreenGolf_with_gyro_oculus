using UnityEngine;
using System.Collections;

[System.Serializable]
public class UserInfo
{
    public string nickName = "JaeHyun";


}

public class GameInfo : MonoBehaviour
{
    private static GameInfo _instance = null;

    public static GameInfo Ins
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = GameObject.Find("/GameInfo");

                // 씬에 PlayInfo클래스가 없으면 생성해줍니다.
                if (obj == null)
                    obj = new GameObject("GameInfo");

                // PlayInfo GameObject에 PlayInfoComp가 없으면 붙여줍니다.
                _instance = obj.GetComponent<GameInfo>();
                if (_instance == null)
                    _instance = obj.AddComponent<GameInfo>();

                _instance.Init();
            }

            return _instance;
        }
    }

    public int curHollIndex = 1;
    public int hollConut = 3;
    public int swingIndex = 1;

    public float curTotalFlyDistance = 0.0f;
    public float curFlyDistance = 0.0f;

    public UserInfo localUser = null;

    public Vector3 offsetEulerAngles = new Vector3();

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (GameInfo._instance == null)
        {
            GameInfo._instance = GameObject.Find("/GameInfo").GetComponent<GameInfo>();
            GameInfo._instance.Init();
        }
    }

    public void Init()
    {

    }
}