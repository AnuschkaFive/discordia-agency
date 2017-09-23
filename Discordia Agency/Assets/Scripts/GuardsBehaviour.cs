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

    public float rotationSpeed;

    // How much faster the Guard moves when in Seeking-Mode.
    public float seekingSpeedFactor;

    // How much faster the Guard moves when in Hunting-Mode.
    public float huntingSpeedFactor;    

    // The patrol points the guard continuously follows while Patrolling.
    public Vector2[] patrolPoints;

    // The modus the guard is currently in.
    public GuardModus modus;

    // Whether the Guard is stationary or not.
    public bool isStationary;

    // The direction the Guard is looking when stationary, in degrees.
    public float stationaryLookDirection;

    // Whether the Guard can currently be knocked out, because Player is close enough from behind.
    private bool canBeKnockedOut;

    // Whether the Player can disguise as this Guard, because Guard is knocked out and Player is close enough from any side.
    private bool canBeDisguised;

    // Whether the Player can drag this Guard, because Guard is knocked out and Player is close enough from any side.
    private bool canBeDragged;

    // Whether the Guard is currently being dragged.
    private bool isBeingDragged;

    private bool isAlerted = false;

    private GameObject guiPlayerControl;

    private GuardsFOV FOV;

    private GameObject gameStatus;

    private AILerp aiLerp;

    private Seeker seeker;

    private GameObject target;

    private Player player;

    private LayerMask collisionMask;

    private LayerMask obstacleMask;

    private Gun gun;

    private Transform lastKnownPlayerPosition;

    private SoundEffect soundEffect;

    private bool isRotatingStationary = false;

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start () {
        this.currStep = 0;
        this.guiPlayerControl = GameObject.Find("Canvas_GUIPlayerControl").gameObject;
        this.FOV = this.GetComponentInChildren<GuardsFOV>();
        this.gameStatus = GameObject.Find("GameStatus").gameObject;
        this.aiLerp = this.GetComponent<AILerp>();
        this.target = this.aiLerp.target.gameObject;
        this.seeker = this.GetComponent<Seeker>();
        this.player = GameObject.Find("Player").GetComponent<Player>();
        this.collisionMask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Obstacles"));
        this.obstacleMask = LayerMask.GetMask("Obstacles");
        this.gun = this.GetComponentInChildren<Gun>();
        this.soundEffect = this.GetComponent<SoundEffect>();
        // Set the Guard's target to the initial patrol point.
        if (!this.isStationary)
        {
            this.SetPatrolling();
        }
        else
        {
            this.aiLerp.repathRate = float.PositiveInfinity;
            this.aiLerp.canMove = false;
            this.transform.eulerAngles = new Vector3(0f, 0f, this.stationaryLookDirection);
        }
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update() {
        switch (this.modus)
        {
            case GuardModus.Patrolling:
                {
                    if (!isStationary)
                    {
                        // If the patrol point is reached, set the next patrol point as target and calculate path towards it.
                        if (this.aiLerp.targetReached)
                        {
                            Debug.Log("Patrolling-Target reached!");
                            currStep = (currStep + 1) % this.patrolPoints.Length;
                            Debug.Log("curStep: " + currStep);
                            this.SetNewTarget(this.patrolPoints[currStep]);
                        }
                    }
                    else
                    {
                        if (Vector2.Distance((Vector2)this.transform.position, this.patrolPoints[0]) < 0.1f)
                        {
                            //Debug.Log("Stationary Guard is back home!");
                            this.aiLerp.canMove = false;
                            this.transform.position = this.patrolPoints[0];
                            if (!this.isRotatingStationary)
                            {
                                StartCoroutine(this.StationaryGuardLooksAround(this.stationaryLookDirection));
                            }
                            //this.transform.eulerAngles = new Vector3(0f, 0f, this.stationaryLookDirection);
                        }
                    }
                          
                 
                 break;
             }
        }
        if (this.canBeKnockedOut && Input.GetButtonDown("KnockOut"))
        {
            this.KnockOutGuard();
            Debug.Log("isKnockedOut");
        }

        if (this.canBeDisguised && Input.GetButtonDown("Disguise"))
        {
            this.DisguiseAsGuard();
        }

        if (this.canBeDragged && !this.isBeingDragged && Input.GetButtonDown("Drag"))
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
        if (this.FOV.visibleKnockedOutGuards.Count > 0)
        {
            this.AlertAllGuards();
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
        this.gameObject.layer = LayerMask.NameToLayer("Knocked Out Guards");
        this.soundEffect.PlaySoundEffect(0);
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
        this.soundEffect.PlaySoundEffect(1);
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
        this.aiLerp.canMove = true;
        this.aiLerp.SearchPath();
    }

    /// <summary>
    /// Sets the Guard to be patrolling. Stationary Guards go to their stationary spot, face the correct direction and stop.
    /// </summary>
    public void SetPatrolling()
    {
        this.modus = GuardModus.Patrolling;
        this.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Enemy");
        this.aiLerp.speed = this.speed;
        this.aiLerp.rotationSpeed = this.rotationSpeed;
        this.aiLerp.repathRate = float.PositiveInfinity;
        this.SetNewTarget(this.patrolPoints[currStep]);        
    }

    /// <summary>
    /// Sets the Guard to be seeking at a specified location.
    /// </summary>
    /// <param name="seekLocation">The location the Guard should move to and search at.</param>
    public void SetSeeking(Vector2 seekLocation)
    {
        if (this.modus != GuardModus.Seeking && this.modus != GuardModus.Hunting && this.modus != GuardModus.KnockedOut)
        {
            this.modus = GuardModus.Seeking;
            this.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Enemy_Curious");
            this.aiLerp.speed = this.speed * this.seekingSpeedFactor;
            this.aiLerp.rotationSpeed = this.rotationSpeed * this.seekingSpeedFactor;
            this.aiLerp.repathRate = float.PositiveInfinity;
            this.transform.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            StartCoroutine(this.IsSeeking(seekLocation));
        }
    }
    
    private IEnumerator IsSeeking(Vector2 seekLocation)
    {
        this.aiLerp.canMove = false;
        yield return new WaitForSeconds(2);
        this.SetNewTarget(seekLocation);
        // while (!this.aiLerp.targetReached)
        while (!this.CanSeeImpactLocation(seekLocation) || Vector2.Distance((Vector2)this.transform.position, seekLocation) > 0.5f)
        {
            yield return null;
        }
        this.aiLerp.canMove = false;
        float seekingTime = 6.0f;
        float startTime = Time.time;
        float startRotation = this.transform.eulerAngles.z;
        float targetRotationOffset = Random.Range(40.0f, 60.0f) * Mathf.Sign(Random.Range(-1.0f, 1.0f));
        float currTime = 0.0f;
        while (this.modus == GuardModus.Seeking && seekingTime > Time.time - startTime)
        {
            //Debug.Log("Wache rotiert!");
            if(Mathf.Abs(this.transform.eulerAngles.z - ((startRotation + targetRotationOffset) % 360)) > 1.0f)
            {
                currTime += (Time.deltaTime * (this.rotationSpeed * 0.01f));
                Vector3 rotation = this.transform.eulerAngles;
                rotation.z = Mathf.LerpAngle(rotation.z, startRotation + targetRotationOffset, currTime);
                this.transform.eulerAngles = rotation;
            } else
            {
                targetRotationOffset = Random.Range(40.0f, 60.0f) * (Mathf.Sign(targetRotationOffset) * -1.0f);
                currTime = 0.0f;
                yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
            }
            yield return null;
        }
        // If the Guard is still in Seeking mode, meaning: hasn't spotted the player: return to Patrolling.
        if (this.modus == GuardModus.Seeking)
        {
            this.SetPatrolling();
        }
    }

    /// <summary>
    /// Checks whether the Guard currently has line of sight to the impact location of the thrown object.
    /// </summary>
    /// <param name="seekLocation"></param>
    /// <returns></returns>
    private bool CanSeeImpactLocation(Vector2 seekLocation)
    {
        RaycastHit2D[] hit = new RaycastHit2D[1];
        return (Physics2D.RaycastNonAlloc(this.transform.position, 
            seekLocation - (Vector2)this.transform.position, 
            hit, 
            Vector2.Distance(seekLocation, (Vector2)this.transform.position) - 0.1f, // Subtract 0.1, since Impact Location might be slightly on top of wall = behind obstacle.
            this.obstacleMask) == 0);
    }

    /// <summary>
    /// Set the Guard to be hunting after the Player. Also alerts all other Guards.
    /// </summary>
    public void SetHunting(Vector2 playerLocation)
    {
        if (this.modus != GuardModus.Hunting)
        {
            this.modus = GuardModus.Hunting;
            if (!this.gameStatus.GetComponent<GUIGameStatus>().playerIsBeingHunted)
            {
                this.soundEffect.PlaySoundEffect(2);
            }
            this.gameStatus.GetComponent<GUIGameStatus>().SetSoundtrackToHunting();
            this.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Enemy_Hunting");
            
            this.aiLerp.speed = this.speed * this.huntingSpeedFactor;
            this.aiLerp.rotationSpeed = this.rotationSpeed * this.huntingSpeedFactor;
            this.aiLerp.repathRate = float.PositiveInfinity;
            this.aiLerp.target = this.player.transform;
            this.AlertAllGuards();
            StartCoroutine(this.HuntPlayer());
        }
    }

    /// <summary>
    /// Shoot the Player, when line of sight to him. Otherwise, run shortest path towards him.
    /// </summary>
    /// <returns>Wait</returns>
    private IEnumerator HuntPlayer()
    {
        while (true)
        {
            this.aiLerp.canMove = false;
            while (this.CanSeePlayer())
            {
                Debug.Log("Player is seen!");
                this.RotateTowards(this.lastKnownPlayerPosition.position);
                this.gun.Shoot();
                yield return null;
            }
            this.aiLerp.canMove = true;
            this.aiLerp.SearchPath();
            while (!this.CanSeePlayer())
            {
                Debug.Log("Player is not seen!");
                this.aiLerp.SearchPath();
                yield return null;
            }
            yield return null;
        }
    }

    /// <summary>
    /// Check whether there is a line of sight to the Player.
    /// </summary>
    /// <returns>True, if there is a line of sight to the Player. False, otherwise.</returns>
    private bool CanSeePlayer()
    {
        RaycastHit2D[] hit = new RaycastHit2D[1];
        bool foundPlayer = 
            (Physics2D.RaycastNonAlloc(this.transform.position, this.player.transform.position - this.transform.position, hit, 10f, this.collisionMask) == 1) && 
            hit[0].collider.GetComponent<IDamageable>() != null &&
            hit[0].collider.GetComponent<IDamageable>().IsPlayer();
        if(foundPlayer)
        {
            this.lastKnownPlayerPosition = hit[0].transform;
        }
        return foundPlayer;
    }
    
    /// <summary>
    /// Rotate towards a target position.
    /// </summary>
    /// <param name="target">The target to rotate towards.</param>
    private void RotateTowards(Vector2 target)
    {
        Vector2 direction = target - (Vector2)this.transform.position;
        float angle = Mathf.Atan2(direction.x, -direction.y) * Mathf.Rad2Deg + 180;
        Vector3 euler = this.transform.eulerAngles;
        euler.z = Mathf.LerpAngle(euler.z, angle, Time.deltaTime * this.aiLerp.rotationSpeed);
        this.transform.eulerAngles = euler;
    }

    /// <summary>
    /// Sets the Guard to be alerted, which increases his FOV.
    /// </summary>
    public void SetAlerted()
    {
        if (this.modus != GuardModus.Hunting && this.modus != GuardModus.KnockedOut)
        {
            this.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Enemy_Curious");
        }
        this.isAlerted = true;
        this.FOV.SetAlertedFOV();
    }

    /// <summary>
    /// Sets all Guards to be alerted, which increases their FOV.
    /// </summary>
    public void AlertAllGuards()
    {
        GuardsBehaviour[] allGuards = this.transform.parent.GetComponentsInChildren<GuardsBehaviour>();
        foreach(GuardsBehaviour guard in allGuards)
        {
            guard.SetAlerted();
        }
    }

    /// <summary>
    /// Makes a stationary Guard that has returned to his position rotate in the direction he is meant to look.
    /// Then lets him rotate slightly left and right.
    /// </summary>
    /// <param name="stationaryLookDirection"></param>
    /// <returns></returns>
    private IEnumerator StationaryGuardLooksAround(float stationaryLookDirection)
    {
        this.isRotatingStationary = true;
        float startTime = 0.0f;
        while (this.modus == GuardModus.Patrolling && Mathf.Abs(this.transform.eulerAngles.z - stationaryLookDirection) > 1.0f)
        {
            //Debug.Log("Wache rotiert in Ausgangposition");
            startTime += Time.deltaTime * (this.rotationSpeed * 0.01f);
            Vector3 rotation = this.transform.eulerAngles;
            rotation.z = Mathf.LerpAngle(rotation.z, stationaryLookDirection, startTime);
            this.transform.eulerAngles = rotation;
            yield return null;
        }
        float targetOffsetRotation = Random.Range(10.0f, 30.0f) * Mathf.Sign(Random.Range(-1.0f, 1.0f));
        startTime = 0.0f;
        while(this.modus == GuardModus.Patrolling && this.isStationary)
        {
            //Debug.Log("Wache rotiert zufällig: derzeitiger Winkel " + this.transform.eulerAngles.z + ", Zielwinkel: " + (stationaryLookDirection + targetOffsetRotation) % 360 );
            if (Mathf.Abs(this.transform.eulerAngles.z - ((stationaryLookDirection + targetOffsetRotation) % 360)) > 1.0f)
            {
                startTime += Time.deltaTime * (this.rotationSpeed * 0.01f);
                Vector3 rotation = this.transform.eulerAngles;
                rotation.z = Mathf.LerpAngle(rotation.z, stationaryLookDirection + targetOffsetRotation, startTime);
                this.transform.eulerAngles = rotation;
            }
            else
            {
                yield return new WaitForSeconds(Random.Range(0.5f, 2.0f));
                targetOffsetRotation = Random.Range(10.0f, 30.0f) * (Mathf.Sign(targetOffsetRotation) * -1.0f);
                startTime = 0.0f;
            }
            yield return null;
        }
        this.isRotatingStationary = false;
    }
}
