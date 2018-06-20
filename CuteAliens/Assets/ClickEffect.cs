using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickEffect : MonoBehaviour {
    Vector3 point;
    GameObject effectGo;
	// Use this for initialization
	void Start () {
        effectGo = Resources.Load<GameObject>("Prefabs/EffectClick");
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButtonDown(0))
        {
            point = new Vector3(Input.mousePosition.x,Input.mousePosition.y,4f);
            point = Camera.main.ScreenToWorldPoint(point);//从屏幕空间转换到世界空间
            GameObject go = Instantiate(effectGo);
            go.transform.position = point;
            Destroy(go, 0.5f);
        }
    }
}
