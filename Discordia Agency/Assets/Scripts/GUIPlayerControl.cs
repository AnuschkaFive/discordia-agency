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

    /// <summary>
    /// Changes whether a control option is displayed in the GUI or not.
    /// </summary>
    /// <param name="controlToChange"></param>
    /// <param name="newStatus"></param>
    public void SetControlStatus(Controls controlToChange, bool newStatus)
    {
        //this.controlStatus[(int)controlToChange] = newStatus;
        this.transform.GetChild((int)controlToChange).gameObject.SetActive(newStatus);
    }   
}
