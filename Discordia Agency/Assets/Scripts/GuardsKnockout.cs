using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardsKnockout : MonoBehaviour {

    private GUIGameStatus gameStatus;

    // Use this for initialization
    void Start () {
        this.gameStatus = GameObject.Find("GameStatus").GetComponent<GUIGameStatus>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player" && this.gameStatus.activatedFeatures[(int)Features.KnockOut])
        {
            this.transform.parent.gameObject.GetComponent<GuardsBehaviour>().SetCanBeKnockedOut(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player" && this.gameStatus.activatedFeatures[(int)Features.KnockOut])
        {
            this.transform.parent.gameObject.GetComponent<GuardsBehaviour>().SetCanBeKnockedOut(false);
        }
    }
}
