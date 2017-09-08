using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour{    

    private float speed = 2.0f;

    private GameObject guiPlayerControl;

    public bool isDisguised = false;

    private bool hasThrowableObject;

    private Rigidbody2D rb;

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start () {
        // Sets the gravity so that the x-y-Plane is the plane to walk on.
        Physics2D.gravity = new Vector3(0f, 0f, 10f);
        rb = GetComponent<Rigidbody2D>();

        this.guiPlayerControl = GameObject.Find("Canvas_GUIPlayerControl").gameObject;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update () {

	}

    /// <summary>
    /// Move the Player.
    /// </summary>
    private void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        
        Vector2 movement = new Vector2(moveHorizontal, moveVertical);

        // Set velocity directly, so that the Player doesn't have drag/momentum (instead of using AddForce()).
        rb.velocity = movement * speed;
    }

    /// <summary>
    /// Toggle the Player's sprite and variable "isDisguised" between disguised and undisguised.
    /// </summary>
    public void ToggleDisguise()
    {
        this.GetComponent<SpriteRenderer>().sprite = (this.isDisguised ? 
            Resources.Load<Sprite>("Sprites/Player") : 
            Resources.Load<Sprite>("Sprites/Player_disguised"));
        this.isDisguised = !this.isDisguised;
    }

    /// <summary>
    /// Toggle whether the Player has a throwable object or not.
    /// </summary>
    public void ToggleHasThrowableObject()
    {        
        this.hasThrowableObject = !this.hasThrowableObject;
        this.guiPlayerControl.gameObject.GetComponent<GUIPlayerControl>().SetControlStatus(Controls.Throw, this.hasThrowableObject);
        this.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = this.hasThrowableObject;
    }

    /// <summary>
    /// Get whether the Player has a throwable object or not.
    /// </summary>
    /// <returns></returns>
    public bool GetHasThrowableObject()
    {
        return this.hasThrowableObject;
    }
}
