using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GuardModus {
    Patrolling, Seeking, Hunting, KnockedOut 
}

public enum GuardRanges
{
    View, KnockOut, Disguise, Drag
}

public class GuardsBehaviour : MonoBehaviour {
    // Which step of the patrol path the guard is currently on.
    private int currStep;

    // The speed with which the guard walks.
    public float speed;

    //
    public float seekingSpeedFactor;

    public float huntingSpeedFactor;

    // The direction the guard is facing during every step of the patrol route. Angle in absolute degrees, 0° being up, 90° left, ...
    public float[] directions;

    // The patrol points the guard continuously follows while Patrolling.
    public Vector2[] patrolPoints;

    // The modus the guard is currently in.
    public GuardModus modus;

    // Whether the Guard can currently be knocked out, because Player is close enough from behind.
    private bool canBeKnockedOut;

    // Whether the Player can disguise as this Guard, because Guard is knocked out and Player is close enough from any side.
    private bool canBeDisguised;

    // Whether the Player can drag this Guard, because Guard is knocked out and Player is close enough from any side.
    private bool canBeDragged;

    // Whether the Guard is currently being dragged.
    private bool isBeingDragged;

    private GameObject guiPlayerControl;

    private GuardsFOV FOV;

    private GameObject gameStatus;

    private AILerp aiLerp;

    private Seeker seeker;

    private GameObject target;

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start () {
        this.currStep = 0;
        this.transform.eulerAngles = new Vector3(0f, 0f, this.directions[currStep]);
        this.guiPlayerControl = GameObject.Find("Canvas_GUIPlayerControl").gameObject;
        this.FOV = this.GetComponentInChildren<GuardsFOV>();
        this.gameStatus = GameObject.Find("GameStatus").gameObject;
        this.aiLerp = this.GetComponent<AILerp>();
        this.target = this.aiLerp.target.gameObject;
        this.seeker = this.GetComponent<Seeker>();
        // Set the Guard's target to the initial patrol point.
        this.SetPatrolling();
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update() {
        switch (this.modus)
        {
            case GuardModus.Patrolling:
                {
                    // If the patrol point is reached, set the next patrol point as target and calculate path towards it.
                    if (this.aiLerp.targetReached)
                    {
                        Debug.Log("Patrolling-Target reached!");
                        currStep = (currStep + 1) % this.patrolPoints.Length;
                        Debug.Log("curStep: " + currStep);
                        this.SetNewTarget(this.patrolPoints[currStep]);
                    }
                    break;
                }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void FixedUpdate()
    {
        if(this.canBeKnockedOut && Input.GetButtonDown("KnockOut"))
        {
            this.KnockOutGuard();
            Debug.Log("isKnockedOut");
        }

        if(this.canBeDisguised && Input.GetButtonDown("Disguise"))
        {
            this.DisguiseAsGuard();
        }

        if(this.canBeDragged && !this.isBeingDragged && Input.GetButtonDown("Drag"))
        {
           this.StartDragGuard();
        }

        if (this.isBeingDragged && Input.GetButtonUp("Drag"))
        {
            this.StopDragGuard();
        }
        if (this.FOV.visiblePlayers.Count > 0)
        {
            //this.gameStatus.GetComponent<GUIGameStatus>().SetGameStatus(GameStatus.Lost, true);
            this.SetHunting(this.FOV.visiblePlayers[0].position);
        }
        foreach (Transform guard in this.FOV.visibleGuards)
        {
            if(guard.GetComponent<GuardsBehaviour>().modus == GuardModus.KnockedOut)
            {
                Debug.Log("Found knocked out Guard!");
            }
        }
    }

    /// <summary>
    /// Changes both a Guard's modus and his sprite to "knocked out". Also deactivates the Guard's ViewRange (and its trigger).
    /// Activated the Guard's DisguiseRange (and its trigger).
    /// </summary>
    void KnockOutGuard()
    {
        this.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Enemy_KnockedOut");
        this.modus = GuardModus.KnockedOut;
        this.aiLerp.enabled = false;
        this.SetCanBeKnockedOut(false);
        this.transform.GetChild((int)GuardRanges.View).gameObject.SetActive(false);
        this.transform.GetChild((int)GuardRanges.KnockOut).gameObject.SetActive(false);
        this.transform.GetChild((int)GuardRanges.Disguise).gameObject.SetActive(true);
        this.transform.GetChild((int)GuardRanges.Drag).gameObject.SetActive(true);
    }

    /// <summary>
    /// Sets whether the Guard can be knocked out, because Player is in range from behind.
    /// </summary>
    /// <param name="canBeKnockedOut">True, if Guard can be knocked out. False, otherwise.</param>
    public void SetCanBeKnockedOut(bool canBeKnockedOut)
    {
        this.canBeKnockedOut = canBeKnockedOut;
        this.guiPlayerControl.GetComponent<GUIPlayerControl>().SetControlStatus(Controls.KnockOut, canBeKnockedOut);
        Debug.Log("canBeKnockedOut: " + canBeKnockedOut);
    }

    /// <summary>
    /// Disguise the Player as this Guard.
    /// </summary>
    public void DisguiseAsGuard()
    {
        this.transform.GetChild((int)GuardRanges.Disguise).gameObject.SetActive(false);
        this.SetCanBeDisguised(false);
        GameObject.Find("Player").gameObject.GetComponent<Player>().ToggleDisguise();
    }

    /// <summary>
    /// Sets whether the Player can disguise as this Guard, because Guard is knocked out and Player is close enough from any side.
    /// </summary>
    /// <param name="canBeDisguised"></param>
    public void SetCanBeDisguised(bool canBeDisguised)
    {
        this.canBeDisguised = canBeDisguised;
        this.guiPlayerControl.GetComponent<GUIPlayerControl>().SetControlStatus(Controls.Disguise, canBeDisguised);
        Debug.Log("canBeDisguised: " + canBeDisguised);
    }

    /// <summary>
    /// Player drags the knocked out Guard along.
    /// </summary>
    public void StartDragGuard()
    {
        Debug.Log("Guard is being dragged");
        this.isBeingDragged = true;
        this.transform.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        this.transform.GetComponent<SpringJoint2D>().enabled = true;
    }

    /// <summary>
    /// Player stops dragging the knocked out Guard along.
    /// </summary>
    public void StopDragGuard()
    {
        Debug.Log("Guard is not being dragged anymore");
        this.isBeingDragged = false;
        this.transform.GetComponent<SpringJoint2D>().enabled = false;
        this.transform.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        // Triggers need to be de- and re-activated after RigidBodyType has changed.
        this.transform.GetChild((int)GuardRanges.Disguise).gameObject.SetActive(false);
        this.transform.GetChild((int)GuardRanges.Drag).gameObject.SetActive(false);
        this.transform.GetChild((int)GuardRanges.Disguise).gameObject.SetActive(true);
        this.transform.GetChild((int)GuardRanges.Drag).gameObject.SetActive(true);
    }

    /// <summary>
    /// Sets whether the Player can drag this Guard, because Guard is knocked out and Player is close enough from any side.
    /// </summary>
    /// <param name="canBeDragged"></param>
    public void SetCanBeDragged(bool canBeDragged)
    {
        this.canBeDragged = canBeDragged;
        this.guiPlayerControl.GetComponent<GUIPlayerControl>().SetControlStatus(Controls.Drag, canBeDragged);
        Debug.Log("canBeDragged: " + canBeDragged);
    }

    /// <summary>
    /// Sets the Guard on a new path.
    /// </summary>
    /// <param name="newTarget">New target for the Guard.</param>
    private void SetNewTarget(Vector2 newTarget)
    {
        this.target.transform.position = newTarget;
        this.aiLerp.SearchPath();
    }

    /// <summary>
    /// Sets the Guard to be patrolling.
    /// </summary>
    public void SetPatrolling()
    {
        this.modus = GuardModus.Patrolling;
        this.aiLerp.speed = this.speed;
        this.SetNewTarget(this.patrolPoints[currStep]);
    }

    /// <summary>
    /// Sets the Guard to be seeking at a specified location.
    /// </summary>
    /// <param name="seekLocation">The location the Guard should move to and search at.</param>
    public void SetSeeking(Vector2 seekLocation)
    {
        this.modus = GuardModus.Seeking;
        this.aiLerp.speed = this.speed * this.seekingSpeedFactor;
        this.SetNewTarget(seekLocation);
        StartCoroutine(this.IsSeeking());
    }

    /// <summary>
    /// Set the Guard to be hunting after the Player.
    /// </summary>
    public void SetHunting(Vector2 playerLocation)
    {
        this.modus = GuardModus.Hunting;
        this.aiLerp.speed = this.speed * this.huntingSpeedFactor;
        //this.SetNewTarget(playerLocation);
        //StartCoroutine(this.ShootPlayer());
        this.aiLerp.target = GameObject.Find("Player").transform;
        this.aiLerp.SearchPath();
        StartCoroutine(this.HuntPlayer());
    }

    private IEnumerator IsSeeking()
    {
        while (!this.aiLerp.targetReached)
        {
            yield return null;
        }
        yield return new WaitForSeconds(4); // TODO: Co-Routine that makes the Guard rotate slightly.
        // If the Guard is still in Seeking mode, meaning: hasn't spotted the player: return to Patrolling.
        if (this.modus == GuardModus.Seeking)
        {
            this.SetPatrolling();
        }
    }

    private IEnumerator ShootPlayer()
    {
        yield return null;
    }

    private IEnumerator HuntPlayer()
    {
        while(!this.aiLerp.targetReached)
        {
            yield return null;
        }
        this.gameStatus.GetComponent<GUIGameStatus>().SetGameStatus(GameStatus.Lost, true);
    }
}
