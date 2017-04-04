using UnityEngine;
using System.Collections;

public class EffectManager : MonoBehaviour {

    public GameObject effectPrefab = null;

    public GameObject shootEffect = null;   // 공 쳤을 때
    public GameObject ballinGroundEffect = null; // 공이 땅에 닿았을 때
    public GameObject hollinEffect1 = null;  // 공이 들어 갔을 때 ( 홀 위치 )
    public GameObject hollinEffect2 = null;  // 공이 들어 갔을 때 ( 홀 주변 )
    public GameObject hollLocationEffect = null; // 홀 위치 알려주는 Effect  

    void Start () {
        // 홀 위치 : 418.3  , -0.25 , 383.71
        Instantiate(hollLocationEffect, new Vector3(418.3f, -0.25f, 383.71f), Quaternion.identity);
    }

    void Update () {

    }

    // 공 쳤을때 효과
    public void BallShootEffect( Vector3 ballshootPosition)
    {
        GameObject returnshootEffect = (GameObject)Instantiate(shootEffect, ballshootPosition, Quaternion.identity);
        GameObject.Destroy(returnshootEffect, 1.0f);
    }

    // 공이 땅에 닿았을 때
    public void BallinGround(Vector3 groundPosition)
    {
        GameObject returnballinGroundEffect = (GameObject)Instantiate(ballinGroundEffect, groundPosition, Quaternion.identity);
        GameObject.Destroy(returnballinGroundEffect, 1.0f);
    }

    // 공이 홀에 들어갔을 때
    public void BallEnterHoll()
    {
        GameObject returnEffectPrefab1 = (GameObject)Instantiate(hollinEffect1, new Vector3(418.3f,1.15f,383.71f), Quaternion.identity );
        GameObject returnEffectPrefab2 = (GameObject)Instantiate(hollinEffect2, new Vector3(418.3f, 1.15f, 383.71f), Quaternion.identity);
        GameObject.Destroy(returnEffectPrefab1, 2.0f);
        GameObject.Destroy(returnEffectPrefab2, 2.0f);
    }
}
