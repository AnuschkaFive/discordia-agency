using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GuardModus {
    Patrolling, Seeking, Hunting, Unconscious 
}

public class GuardsBehaviour : MonoBehaviour {
    // Which step of the patrol path the guard is currently on.
    private int currStep;

    // TODO: Wahrscheinlich nicht benötigt, Objektreferenzen reichen.
    //private int id;

    // The speed with which the guard walks.
    public float speed;

    // The direction the guard is facing during every step of the patrol route. Angle in absolute degrees, 0° being up, 90° left, ...
    public float[] directions;

    // The patrol points the guard continuously follows while Patrolling.
    public Vector2[] patrolPoints;

    // The modus the guard is currently in.
    public GuardModus modus;

    private bool canBeKnockedOut;
    

	// Use this for initialization
	void Start () {
        // TODO: Oder ID als public variable und in der Szene setzen.
        // TODO: Wahrscheinlich gar nicht benötigt; Objektreferenzen reichen
        //string guardName = this.gameObject.name.ToString();
        //this.id = int.Parse(guardName.Substring(guardName.Length - 2));

        this.currStep = 0;
        transform.eulerAngles = new Vector3(0f, 0f, this.directions[currStep]);
    }

    // Update is called once per frame
    void Update() {
        // Move the guard towards the next patrol point.
        transform.position = Vector2.MoveTowards(transform.position, this.patrolPoints[currStep], this.speed * Time.deltaTime);

        // If the patrol point is reached, set the next patrol point as target and adjust the guard's direction.
        if(Vector2.Distance(transform.position, this.patrolPoints[currStep]) < 0.001f)
        {
            transform.position = this.patrolPoints[currStep];
            currStep = (currStep + 1) % this.patrolPoints.Length;
            transform.eulerAngles = new Vector3(0f, 0f, this.directions[currStep]);
        }
    }

    // Changes both a Guard's modus and his sprite to "unconscious".
    void setGuardUnconscious()
    {
        this.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Enemy_Unconsious");
        this.modus = GuardModus.Unconscious;
    }

    void setCanBeKnockedOut(bool canBeKnockedOut)
    {
        this.canBeKnockedOut = canBeKnockedOut;
    }

    bool getCanBeKnockedOut()
    {
        return this.canBeKnockedOut;
    }
}
