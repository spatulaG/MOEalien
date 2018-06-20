using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Drop : MonoBehaviour {
    DOTweenPath doPath;
	// Use this for initialization
	void Start () {
        doPath = GetComponent<DOTweenPath>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag.Equals("Terrain"))
        {
            //落地
            doPath.DOPlay();           
        }
    }
}
