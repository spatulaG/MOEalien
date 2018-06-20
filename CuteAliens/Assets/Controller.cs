using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller: MonoBehaviour {

    //两个状态：IDLE 和CLICKACT
    protected const int HERO_IDLE = 0;
    protected const int HERO_ACTCLICK = 1;
    protected Animator animtor;
    protected int gameState = 0;

    private  float timer = 0;
    protected float frozeTime = 5f;
    protected bool canClick = true;

    void Start()
    {
        Init();
        SetGameState(HERO_IDLE);
    }
    protected virtual void  Init()
    {
        animtor = GetComponent<Animator>();
    }
    
    //检测射线
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10000f, LayerMask.GetMask("Actor")))
            {

                if (hit.collider == this.GetComponent<Collider>())
                {
                    if ( ! canClick) return;
                    AudioManager._instance.PlayEffect("eat");
                    SetGameState(HERO_ACTCLICK);
                }
            }
        }
    }
    //更新运动
    void FixedUpdate()
    {
        switch (gameState)
        {
            case HERO_IDLE:
                Idle();
                break;
            case HERO_ACTCLICK:
                ActClick();
                break;           
        }
    }
    void SetGameState(int state)
    {       
        gameState = state;
    }

    protected virtual void Idle()
    {

    }

    protected virtual void ActClick()
    {
        timer+=Time.deltaTime;
        if (timer > frozeTime)
        {
            timer = 0;
            SetGameState(HERO_IDLE);
            canClick = true;
        }
        else
        {
            canClick = false;
        }
    }
}
