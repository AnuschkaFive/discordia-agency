using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour {

    private GameObject guiGameStatus;

    // Use this for initialization
    void Start () {
        this.guiGameStatus = GameObject.Find("Canvas_GUIGameStatus").gameObject;
	}
	
	// Update is called once per frame
	void Update () {
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.name);
        this.guiGameStatus.GetComponent<GUIGameStatus>().SetGameStatus(GameStatus.Won, true);
    }
}
