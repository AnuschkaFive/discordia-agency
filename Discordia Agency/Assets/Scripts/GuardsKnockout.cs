using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardsKnockout : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        this.transform.parent.gameObject.GetComponent<GuardsBehaviour>().setCanBeKnockedOut(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        this.transform.parent.gameObject.GetComponent<GuardsBehaviour>().setCanBeKnockedOut(false);
    }
}
