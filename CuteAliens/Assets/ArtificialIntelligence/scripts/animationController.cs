using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationController : MonoBehaviour {
    public Animator anim;
	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("3")) {
            print("pressed 3!");
            anim.Play("WAIT03", -1, 0f);//p3:starting point
        }
        else if (Input.GetKeyDown("2"))
        {
            print("pressed 2!");
            anim.Play("WAIT02", -1, 0f);//p3:starting point
        }
        else if (Input.GetKeyDown("4"))
        {
            print("pressed 4!");
            anim.Play("WAIT04", -1, 0f);//p3:starting point
        }
    }
}
