using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardsFOV : MonoBehaviour {
    [Range(0, 360)]
    public float viewAngle;
    [Range(0,10)]
    public float viewRadius;

    private LayerMask playerMask;
    private LayerMask obstacleMask;
    private LayerMask guardMask;

    private GameObject guiGameStatus;

    public List<Transform> visibleTargets = new List<Transform>();

	// Use this for initialization
	void Start () {
        this.playerMask = LayerMask.GetMask("Player");
        this.obstacleMask = LayerMask.GetMask("Obstacles");
        this.guardMask = LayerMask.GetMask("Guards");
        this.guiGameStatus = GameObject.Find("Canvas_GUIGameStatus").gameObject;
    }
	
	// Update is called once per frame
	void Update () {
        this.FindVisibleTargets();
	}

    public Vector2 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += this.transform.parent.eulerAngles.z;
        }
        return new Vector2(Mathf.Sin(-angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    private void FindVisibleTargets()
    {
        this.visibleTargets.Clear();
        Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(this.transform.parent.position, this.viewRadius, playerMask);
        for(int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector2 dirToTarget = (target.position - this.transform.parent.position).normalized;
            if(Vector2.Angle(this.transform.parent.up, dirToTarget) < viewAngle / 2)
            {
                float distToTarget = Vector2.Distance(this.transform.parent.position, target.position);

                if(!Physics2D.Raycast(this.transform.parent.position, dirToTarget, distToTarget, obstacleMask))
                {
                    this.visibleTargets.Add(target);
                    this.guiGameStatus.GetComponent<GUIGameStatus>().setGameStatus(GameStatus.Lost, true);
                    Debug.Log(target.gameObject.name + " ran into " + this.transform.parent.gameObject.name + "'s View!");
                }
            }
        }
    }
}
