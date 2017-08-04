using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameStatus
{
    Lost, Won, Length
}

public class GUIGameStatus : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void setGameStatus(GameStatus gameStatusToChange, bool newStatus)
    {
        //this.controlStatus[(int)controlToChange] = newStatus;
        this.transform.GetChild((int)gameStatusToChange).gameObject.SetActive(newStatus);
    }
}
