using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour{    

    private float speed = 2.0f;

    public bool isDisguised = false;

    private Rigidbody2D rb;

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start () {
        // Sets the gravity so that the x-y-Plane is the plane to walk on.
        Physics2D.gravity = new Vector3(0f, 0f, 10f);
        rb = GetComponent<Rigidbody2D>();
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
    public void toggleDisguise()
    {
        this.GetComponent<SpriteRenderer>().sprite = (this.isDisguised ? 
            Resources.Load<Sprite>("Sprites/Player") : 
            Resources.Load<Sprite>("Sprites/Player_disguised"));
        this.isDisguised = !this.isDisguised;
    }
}
