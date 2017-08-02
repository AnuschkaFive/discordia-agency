using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour{    

    public float speed;

    public float drag = 1.0f;

    public bool isDisguised = false;

    private Rigidbody2D rb;

	// Use this for initialization
	void Start () {
        Physics2D.gravity = new Vector3(0f, 0f, 10f);
        rb = GetComponent<Rigidbody2D>();
       // ResizeRelativeToField();
        rb.drag = drag;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        
        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        
        rb.AddForce(movement * speed);
    }

    private void toggleDisguise()
    {
        this.GetComponent<SpriteRenderer>().sprite = (this.isDisguised ? 
            Resources.Load<Sprite>("Sprites/Player") : 
            Resources.Load<Sprite>("Sprites/Player_disguised"));
        this.isDisguised = !this.isDisguised;
    }
}
