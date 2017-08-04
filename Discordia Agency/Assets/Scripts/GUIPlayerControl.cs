using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Controls
{
    KnockOut, Disguise, Drag, PickUp, Throw, Length
}

public class GUIPlayerControl : MonoBehaviour {

    //private bool[] controlStatus; 

	// Use this for initialization
	void Start () {
        //controlStatus = new bool[(int)Controls.Length];
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void setControlStatus(Controls controlToChange, bool newStatus)
    {
        //this.controlStatus[(int)controlToChange] = newStatus;
        this.transform.GetChild((int)controlToChange).gameObject.SetActive(newStatus);
    }
}
