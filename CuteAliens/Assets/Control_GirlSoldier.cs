using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Control_GirlSoldier : Controller {

    private float normalScale;
    private  float maxScale = 3f;
    private float scaleSpeed = 5.0f;

    protected override void Init()
    {
        base.Init();
        normalScale = transform.localScale.x;
    }
    protected override void Idle()
    {
        base.Idle();
        if (Mathf.Abs(normalScale - transform.localScale.x) > 0.1f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(normalScale, normalScale, normalScale), Time.deltaTime * scaleSpeed);
        }
    }
    protected override void ActClick()
    {
        base.ActClick();
        if (maxScale- transform.localScale.x > 0.1f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale,new Vector3(maxScale,maxScale,maxScale),Time.deltaTime*scaleSpeed);
        }
     
    }

}
