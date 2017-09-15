using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableObjectOnPlayer : MonoBehaviour {

    private float maxScalingFactor = 0.6f;

    private float minPower = 0.1f;

    private float maxPower = 5f;

    private float powerChangeSpeed = 0.08f;

    private float power;

    private float throwingSpeed = 2.0f;

    private float rotationSpeed = 100f;

    private Player player;

    private ThrowableObjectPowerMeter powerMeter;

    private bool isFlying;

    private bool powerIsBeingChanged;

    private bool powerIsIncreasing = true;

    private Vector2 startPosition;

    private Vector2 startPositionOnPlayer;

    private Vector2 startScaling;

    private Vector2 targetPosition;

    private LayerMask obstacleMask;

    private LayerMask guardMask;

    private float listeningRadius = 10.0f;

    private SoundEffect soundEffect;

	// Use this for initialization
	void Start () {
        this.player = this.gameObject.GetComponentInParent<Player>();
        this.startPositionOnPlayer = this.transform.localPosition;
        this.startScaling = this.transform.localScale;
        this.powerMeter = this.GetComponentInChildren<ThrowableObjectPowerMeter>();
        this.power = this.minPower;
        this.obstacleMask = LayerMask.GetMask("Obstacles");
        this.guardMask = LayerMask.GetMask("Guards");
        this.soundEffect = this.GetComponent<SoundEffect>();
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
            if (Input.GetButtonDown("Throw"))
            {
                this.powerIsBeingChanged = true;
                StartCoroutine(this.ChangePower());           
            }
            if (Input.GetButtonUp("Throw"))
            {
                this.powerIsBeingChanged = false;
                this.Throw();
            }
        }
    }

    /// <summary>
    /// Change the amount of power the object is being thrown with and displays the according power meter.
    /// </summary>
    /// <returns>Loop</returns>
    private IEnumerator ChangePower()
    {
        this.powerMeter.displayPowerMeter();
        while(this.powerIsBeingChanged)
        {
            if (this.powerIsIncreasing && this.power >= this.maxPower)
            {
                this.powerIsIncreasing = false;
            }
            if (!this.powerIsIncreasing && this.power <= this.minPower)
            {
                this.powerIsIncreasing = true;
            }

            if (this.powerIsIncreasing)
            {
                this.power += this.powerChangeSpeed;
            }
            else
            {
                this.power -= this.powerChangeSpeed;
            }
            this.powerMeter.updatePowerMeter(this.power / (this.maxPower - this.minPower));
            yield return null;
        }
        this.powerMeter.hidePowerMeter();
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

    /// <summary>
    /// Throw the object in the direction it is facing with the determined power.
    /// </summary>
    private void Throw()
    {
        this.isFlying = true;
        this.soundEffect.PlaySoundEffect(0);
        this.targetPosition = this.player.transform.position + Vector3.Normalize(this.transform.position - this.player.transform.position) * this.power;
        Debug.Log("Player Pos: " + this.player.transform.position);
        Debug.Log("Objekt Pos: " + this.transform.position);
        Debug.Log("Objekt Target: " + this.targetPosition);
        Vector2 impactPosition = this.targetPosition;

        // Check if there is an Obstacle (= a wall) in the way.
        RaycastHit2D[] hits = new RaycastHit2D[1];        
        if(Physics2D.RaycastNonAlloc(this.transform.position, this.targetPosition - (Vector2)this.transform.position, hits, Vector2.Distance(this.transform.position, this.targetPosition), this.obstacleMask) > 0 )
        {
            impactPosition = hits[0].point;
        }
        this.startPosition = this.transform.position;
        StartCoroutine(this.MoveObject(this.targetPosition, impactPosition));       
    }

    /// <summary>
    /// Moves the thrown object until it reaches its target location.
    /// Afterwards, returns the object itself to its original location on the Player body, so that it is ready for the next throw,
    /// and hides it again, so that the Player needs to pick up another one.
    /// </summary>
    /// <param name="originalTarget">How far the object would fly, based on the power. Doesn't include walls or obstacles.</param>
    /// <param name="impact">Target location of the throw, including obstacles.</param>
    /// <returns>Loop</returns>
    private IEnumerator MoveObject(Vector2 originalTarget, Vector2 impact)
    {
        this.transform.parent = null;
        Vector2 globalStartScale = this.transform.lossyScale;
        while ((Vector2.Distance(this.transform.position, impact) > 0.01f)) {
            this.transform.position = Vector2.MoveTowards(this.transform.position, impact, this.throwingSpeed * Time.deltaTime);
            this.transform.localScale = this.CalculateNewScaling(this.transform.position, this.startPosition, originalTarget, globalStartScale, this.maxScalingFactor);
            yield return null;
        }
        this.soundEffect.PlaySoundEffect(1);
        this.transform.parent = this.player.gameObject.transform;
        this.isFlying = false;
        this.player.ToggleHasThrowableObject();
        this.transform.localEulerAngles = new Vector2(1.0f, 0.0f);
        this.transform.localPosition = this.startPositionOnPlayer;
        this.transform.localScale = this.startScaling;
        this.CallGuards(this.listeningRadius);
        ThrowableObjectOnGround[] allObjects = GameObject.Find("Objects").gameObject.GetComponentsInChildren<ThrowableObjectOnGround>();
        foreach(ThrowableObjectOnGround objectOnGround in allObjects)
        {
            objectOnGround.Spawn();
        }       
    }

    private Vector2 CalculateNewScaling(Vector2 currentPosition, Vector2 startPosition, Vector2 targetPosition, Vector2 startScaling, float maxScalingFactor)
    {
        float distanceFromStart = Vector2.Distance(startPosition, currentPosition);
        float distanceBetweenStartAndTarget = Vector2.Distance(startPosition, targetPosition);
        float percentageTravelled = distanceFromStart * 2.0f / distanceBetweenStartAndTarget;
        if(percentageTravelled > 1.0f)
        {
            percentageTravelled = 2.0f - percentageTravelled;
        }
        return startScaling * (1.0f + maxScalingFactor * percentageTravelled);
    }

    /// <summary>
    /// Call all Guards within the specified listening radius to the thrown object's target location.
    /// </summary>
    /// <param name="listeningRadius">The specified listening radius in which Guards are located.</param>
    private void CallGuards(float listeningRadius)
    {
        GuardsBehaviour currentGuard;
        Collider2D[] targetsInListeningRadius = Physics2D.OverlapCircleAll(this.transform.parent.position, listeningRadius, this.guardMask);
        for (int i = 0; i < targetsInListeningRadius.Length; i++)
        {
            currentGuard = targetsInListeningRadius[i].gameObject.GetComponent<GuardsBehaviour>();
            currentGuard.SetSeeking(this.targetPosition);
        }
        if(targetsInListeningRadius.Length > 0)
        {
            this.soundEffect.PlaySoundEffectDelayed(2, 0.25f);
        }
    }
}
