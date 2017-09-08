using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableObjectOnPlayer : MonoBehaviour {

    private Vector2 direction = new Vector2(1.0f, 0.0f);

    private float power = 5f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ChangeDirection(float changeInDegrees)
    {

    }

    public void SetPower(float newPower)
    {

    }

    public void Throw()
    {
        StartCoroutine(this.MoveObject());
    }

    IEnumerator MoveObject()
    {
        while ((Vector2.Distance(transform.position, (Vector2)this.transform.position + this.direction * this.power) > 0.01f)) {
            this.transform.position = Vector2.MoveTowards(this.transform.position, (Vector2) this.transform.position + this.direction * power, power * Time.deltaTime);
            yield return null;
        }
        this.transform.GetComponentInParent<Player>().ToggleHasThrowableObject();
    }
}
