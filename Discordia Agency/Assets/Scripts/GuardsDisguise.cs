using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardsDisguise : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnEnable()
    {
        this.transform.parent.gameObject.GetComponent<GuardsBehaviour>().setCanBeDisguised(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        this.transform.parent.gameObject.GetComponent<GuardsBehaviour>().setCanBeDisguised(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        this.transform.parent.gameObject.GetComponent<GuardsBehaviour>().setCanBeDisguised(false);
    }
}
