using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardsView : MonoBehaviour {

    private GameObject guiGameStatus;

    // Use this for initialization
    void Start () {
        this.guiGameStatus = GameObject.Find("Canvas_GUIGameStatus").gameObject;
    }
	
	// Update is called once per frame
	void Update () {		
	}

    void OnTriggerEnter2D(Collider2D collision)
    {
        this.guiGameStatus.GetComponent<GUIGameStatus>().setGameStatus(GameStatus.Lost, true);
        Debug.Log(collision.gameObject.name + " ran into " + this.transform.parent.gameObject.name + " View!");
    }
}
