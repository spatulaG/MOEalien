using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour {
    //在给定的点之间随机移动。检测初始点，到达一个点之后，随机指定下一个点。
    public   Transform[] targetsTrans;
    private float speed =5f;
    private float rotateSpeed = 1;
    int nextIndex=0;
    Rigidbody rb;
    bool hasToDir = false;
	// Use this for initialization
	void Start () {
        SetIndex();
        rb = GetComponent<Rigidbody>();
    }
	void SetIndex()
    {
        //随机指定一个目标点;
        int tempIndex = nextIndex;
        nextIndex = Random.Range(0, targetsTrans.Length);
        if(nextIndex == tempIndex)
        {
            nextIndex = (nextIndex + 1) % targetsTrans.Length;
        }
    }
    void ToDir()
    {        
        Vector3 rotateVector =(targetsTrans[nextIndex].position - transform.position);
        Quaternion newRotation = Quaternion.LookRotation(rotateVector,transform.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, rotateSpeed * Time.deltaTime);
    }
    // Update is called once per frame
    void Update () {

        //向目标点移动
        transform.Translate((targetsTrans[nextIndex].position-transform.position).normalized*speed*Time.deltaTime);
        //调整旋转方向，面向目标点
        //由当前方向转到目标方向
        ToDir();

        if (Vector3.Distance(transform.position, targetsTrans[nextIndex].position) < 0.2f)
        {
            hasToDir = false;
            SetIndex();
        }
       



    }

}
