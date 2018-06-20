using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Control_AS : Controller {

    private float angleSpeed = 10f;
    protected override void Idle()
    {
        base.Idle();
    }

    protected override void ActClick()
    {
        base.ActClick();
        transform.Rotate(transform.up,angleSpeed);
    }
}
