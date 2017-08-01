using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardsBehaviour : MonoBehaviour {

    private int currStep;

    private int id;

    public float speed;

    public float[] directions;

    public Vector2[] patrolPoints;
    

	// Use this for initialization
	void Start () {
        // TODO: Oder ID als public variable und in der Szene setzen.
        string guardName = this.gameObject.name.ToString();
        this.id = int.Parse(guardName.Substring(guardName.Length - 2));

        this.currStep = 0;

        transform.eulerAngles = new Vector3(0f, 0f, this.directions[currStep]);
	}

    // Update is called once per frame
    void Update() {
        transform.position = Vector2.MoveTowards(transform.position, this.patrolPoints[currStep], this.speed * Time.deltaTime);
        if(Vector2.Distance(transform.position, this.patrolPoints[currStep]) < 0.001f)
        {
            transform.position = this.patrolPoints[currStep];
            currStep = (currStep + 1) % this.patrolPoints.Length;
            transform.eulerAngles = new Vector3(0f, 0f, this.directions[currStep]);
        }
    }

}
