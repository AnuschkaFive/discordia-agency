using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardsDisguise : MonoBehaviour {

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
        if ((collision.gameObject.name == "Player") && (!collision.gameObject.GetComponent<Player>().isDisguised) && this.gameStatus.activatedFeatures[(int)Features.Disguise])
        {
            this.transform.parent.gameObject.GetComponent<GuardsBehaviour>().SetCanBeDisguised(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player" && this.gameStatus.activatedFeatures[(int)Features.Disguise])
        {
            this.transform.parent.gameObject.GetComponent<GuardsBehaviour>().SetCanBeDisguised(false);
        }
    }
}
