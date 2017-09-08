using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableObjectOnGround : MonoBehaviour
{
    private GameObject guiPlayerControl;
    private GameObject player;
    private bool canBePickedUp;

    // Use this for initialization
    void Start()
    {
        this.guiPlayerControl = GameObject.Find("Canvas_GUIPlayerControl").gameObject;
        this.player = GameObject.Find("Player").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if(this.canBePickedUp && Input.GetButton("PickUp"))
        {
            this.player.GetComponent<Player>().ToggleHasThrowableObject();
            this.Despawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player" && !this.player.GetComponent<Player>().GetHasThrowableObject())
        {
            this.guiPlayerControl.GetComponent<GUIPlayerControl>().SetControlStatus(Controls.PickUp, true);
            this.canBePickedUp = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player")
        {
            this.guiPlayerControl.GetComponent<GUIPlayerControl>().SetControlStatus(Controls.PickUp, false);
            this.canBePickedUp = false;
        }
    }

    /// <summary>
    /// Despawns the object after it has been picked up.
    /// </summary>
    private void Despawn()
    {
        Debug.Log("Object despawned!");
        this.guiPlayerControl.GetComponent<GUIPlayerControl>().SetControlStatus(Controls.PickUp, false);
        this.transform.GetComponent<SpriteRenderer>().enabled = false;
        this.transform.GetComponent<CircleCollider2D>().enabled = false;
    }

    /// <summary>
    /// Spawns the Object again so it can be picked up again.
    /// </summary>
    public void Spawn()
    {
        Debug.Log("Object spawned!");
        this.transform.GetComponent<SpriteRenderer>().enabled = true;
        this.transform.GetComponent<CircleCollider2D>().enabled = true;
    }
}
