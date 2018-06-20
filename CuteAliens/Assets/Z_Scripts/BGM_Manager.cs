using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGM_Manager : MonoBehaviour {
    private string strs;
    // Use this for initialization
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        strs =SceneManager.GetActiveScene().name;//用_来分割当前关卡的名字

        //根据关卡的名字来播放BGM
        if (strs== "Main")
        {            
           AudioManager._instance.PlayBG("orbt");                
        }
      
    }
}
