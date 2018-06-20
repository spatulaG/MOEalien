using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    [HideInInspector]
    public AudioSource m_Bg;
    [HideInInspector]
    public AudioSource m_effect;
    public string ResourcesDirBGM = "Sound/BGM";
    public string ResourcesDirEffect = "Sound/Effect";

    public float initEffectVolume = 0.5f;
    public float initBgmVolume = 0.5f;
    private static AudioManager instance;
    public static AudioManager _instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType(typeof(AudioManager)) as AudioManager;
                if (!instance)
                {
                    var obj = new GameObject("AudioManager");
                    obj.AddComponent<BGM_Manager>();
                    instance = obj.AddComponent<AudioManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }
    void Awake()
    {
        instance = this;
        m_Bg = gameObject.AddComponent<AudioSource>();
        m_Bg.playOnAwake = false;
        m_Bg.loop = true;

        m_effect = gameObject.AddComponent<AudioSource>();
        InitVolume();
    }
    private void Update()
    {

    }
    void InitVolume()
    {
        //只执行一次
        if (PlayerPrefs.HasKey("isMute"))
        {
            //读档
            m_effect.mute = m_Bg.mute = PlayerPrefs.GetInt("isMute") == 0 ? false : true;
            m_Bg.volume = PlayerPrefs.GetFloat("bgmVolume");
            m_effect.volume = PlayerPrefs.GetFloat("effectVolume");
        }
        else
        {   //初始值  
            m_Bg.mute = false;
            m_effect.mute = false;
            m_Bg.volume = initBgmVolume;
            m_effect.volume = initEffectVolume;
        }
    }
    //播放背景音乐
    public void PlayBG(string audioName)
    {
        string oldName;
        if (m_Bg.clip == null)
        {
            oldName = "";
        }
        else
        {
            oldName = m_Bg.clip.name;
        }

        if (oldName != audioName)
        {
            //加载资源 clip
            string path = ResourcesDirBGM + "/" + audioName;
            AudioClip clip = Resources.Load<AudioClip>(path);
            //播放
            if (clip != null)
            {
                m_Bg.clip = clip;
                m_Bg.Play();
            }
        }
    }

    //音效
    public void PlayEffect(string audioName)
    {

        string path = ResourcesDirEffect + "/" + audioName;
        AudioClip clip = Resources.Load<AudioClip>(path);
        m_effect.PlayOneShot(clip);
    }
}
