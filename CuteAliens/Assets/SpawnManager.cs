using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnManager : MonoBehaviour {
    public Button testButton;
    public Transform[] bornPoints;
    // Use this for initialization
    void Start () {
        testButton.onClick.AddListener(SpawnNew);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    void SpawnNew()
    {

    }
}
