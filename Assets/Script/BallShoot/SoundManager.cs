using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public AudioSource bgm = null;
    public AudioSource swingSound = null;
    public AudioSource puttingSound = null;

    void Start()
    {
        
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.B))
        //{
        //    //this.shot1.Play();
        //    //PlaySwingSound();
        //    PlayPuttingSound();
        //}
    }

    public void GameSceneLoad()
    {
        bgm.volume = 0.2f;
    }

    public void PlaySwingSound()
    {
        swingSound.Play();
    }

    public void PlayPuttingSound()
    {
        puttingSound.Play();
    }


}
