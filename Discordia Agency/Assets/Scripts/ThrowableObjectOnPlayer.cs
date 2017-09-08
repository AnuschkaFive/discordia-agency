using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableObjectOnPlayer : MonoBehaviour {

    private float power = 5f;

    private float rotationSpeed = 100f;

    private Player player;

    private bool isFlying;

    public Vector2 startPosition;

    public Vector2 targetPosition;

	// Use this for initialization
	void Start () {
        this.player = this.gameObject.GetComponentInParent<Player>();
        startPosition = this.transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
        if (this.player.GetHasThrowableObject() && !this.isFlying)
        {
            if (Input.GetButton("Rotate Left"))
            {
                this.ChangeDirection(-this.rotationSpeed);
            }
            if (Input.GetButton("Rotate Right"))
            {
                this.ChangeDirection(this.rotationSpeed);
            }
            if (Input.GetButtonUp("Throw"))
            {
                this.Throw();                
            }
        }
    }

    /// <summary>
    /// Change the direction the object is positioned in by rotating around the center of the Player.
    /// </summary>
    /// <param name="change">Positive to rotate clockwise. Negative to rotate counterclockwise.</param>
    private void ChangeDirection(float change)
    {
        Debug.Log("Change Direction of Object");
        this.transform.RotateAround(this.transform.parent.transform.position, Vector3.back, change * Time.deltaTime);
    }

    public void SetPower(float newPower)
    {

    }

    public void Throw()
    {
        this.isFlying = true;
        this.targetPosition = this.player.transform.position + Vector3.Normalize(this.transform.position - this.player.transform.position) * this.power;
        Debug.Log("Player Pos: " + this.player.transform.position);
        Debug.Log("Objekt Pos: " + this.transform.position);
        Debug.Log("Objekt Target: " + this.targetPosition);
        StartCoroutine(this.MoveObject(this.targetPosition));       
    }

    IEnumerator MoveObject(Vector2 target)
    {
        Debug.Log("Objekt bewegst sich!");
        while ((Vector2.Distance(this.transform.position, target) > 0.01f)) {
            this.transform.position = Vector2.MoveTowards(this.transform.position, target, Time.deltaTime);
            //Debug.Log("In Schleife");
            yield return null;
        }
        Debug.Log("Nach Schleife");
        this.isFlying = false;
        this.player.ToggleHasThrowableObject();
        this.transform.localEulerAngles = new Vector2(1.0f, 0.0f);
        this.transform.localPosition = this.startPosition;
        yield return new WaitForSeconds(2);
        GameObject.Find("Object_01").gameObject.GetComponent<ThrowableObjectOnGround>().Spawn();        
    }
}
