using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour {

    private GameObject gameStatus;

    // Use this for initialization
    void Start () {
        this.gameStatus = GameObject.Find("GameStatus").gameObject;
	}
	
	// Update is called once per frame
	void Update () {
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.name);
        this.gameStatus.GetComponent<GUIGameStatus>().SetGameStatus(GameStatus.Won, true);
    }
}
