using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Based on https://www.youtube.com/watch?v=rQG9aUWarwE, https://www.youtube.com/watch?v=73Dc5JTCmKI.
/// </summary>
public class GuardsFOV : MonoBehaviour {
    [Range(0, 360)]
    public float viewAngle;
    [Range(0,10)]
    public float viewRadius;

    public float meshResolution;
    public int edgeResolveIterations;
    public float edgeDstThreshold;

    public MeshFilter viewMeshFilter;
    private Mesh viewMesh;

    private LayerMask playerMask;
    private LayerMask obstacleMask;
    private LayerMask guardMask;

    private GameObject guiGameStatus;

    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

	// Use this for initialization
	void Start () {
        this.playerMask = LayerMask.GetMask("Player");
        this.obstacleMask = LayerMask.GetMask("Obstacles");
        this.guardMask = LayerMask.GetMask("Guards");
        this.guiGameStatus = GameObject.Find("Canvas_GUIGameStatus").gameObject;
        this.viewMesh = new Mesh();
        this.viewMesh.name = "ViewMesh";
        this.viewMeshFilter.mesh = viewMesh;
    }
	
	/// <summary>
    /// Late update is called after inputs have been executed.
    /// </summary>
	void LateUpdate () {
        this.FindVisibleTargets();
        this.DrawFieldOfFiew();
	}

    /// <summary>
    /// Takes the angle the FOV is facing in and calculates the corresponding directional vector.
    /// </summary>
    /// <param name="angleInDegrees">The angle the FOV is facing in in degrees.</param>
    /// <param name="angleIsGlobal">True, if the global angle is given. False, if the offset from the FOV's facing angle is given.</param>
    /// <returns>Directional vector.</returns>
    public Vector2 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += this.transform.parent.eulerAngles.z;
        }
        return new Vector2(Mathf.Sin(-angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    /// <summary>
    /// Finds all GameObjects on a specific layer that are 
    /// (a) within the specified view radius, 
    /// (b) within the specified view angle and 
    /// (c) not occluded by an obstacle (= anything on the Layer "Obstacles").
    /// </summary>
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

    /// <summary>
    /// Visualises the FOV.
    /// </summary>
    private void DrawFieldOfFiew()
    {
        int stepCount = Mathf.RoundToInt(this.viewAngle * this.meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector2> viewPoints = new List<Vector2>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = this.transform.parent.eulerAngles.z - this.viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = this.ViewCast(angle);

            if(i > 0)
            {
                // Test against edge distance treshold is necessary in case two obstacles are behind each other.
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if(oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = this.FindEdge(oldViewCast, newViewCast);
                    if(edge.pointA != Vector2.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector2.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }


            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector2.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = this.transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = (Vector3[])vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    /// <summary>
    /// Finds the exact edge (of an obstacle) between two rays that are cast.
    /// </summary>
    /// <param name="minViewCast">The ray to the left of the edge.</param>
    /// <param name="maxViewCast">The ray to the right of the edge.</param>
    /// <returns>The point closest to the right and to the left of the edge.</returns>
    private EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector2 minPoint = Vector2.zero;
        Vector2 maxPoint = Vector2.zero;

        for(int i = 0; i < this.edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);
            // Test against edge distance treshold is necessary in case two obstacles are behind each other.
            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            } else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }
        return new EdgeInfo(minPoint, maxPoint);
    }

    /// <summary>
    /// Casts a ray and returns information about the first object that was hit, if any.
    /// </summary>
    /// <param name="globalAngle">The global angle, in degrees, in which the ray is cast.</param>
    /// <returns>Information about the Viewcast.</returns>
    private ViewCastInfo ViewCast(float globalAngle)
    {
        Vector2 dir = DirFromAngle(globalAngle, true);
        RaycastHit2D[] hits = new RaycastHit2D[1];
        if(Physics2D.RaycastNonAlloc(this.transform.parent.position, dir, hits, this.viewRadius, this.obstacleMask) > 0)
        {
            return new ViewCastInfo(true, hits[0].point, hits[0].distance, globalAngle);
        } else
        {
            return new ViewCastInfo(false, (Vector2)this.transform.parent.position + dir * this.viewRadius, this.viewRadius, globalAngle);
        }
    }

    /// <summary>
    /// Saves the necessary information about the Viewcast, in order to correctly visualise the FOV.
    /// </summary>
    public struct ViewCastInfo
    {
        public bool hit;
        public Vector2 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool hit, Vector3 point, float dst, float angle)
        {
            this.hit = hit;
            this.point = point;
            this.dst = dst;
            this.angle = angle;
        }
    }

    /// <summary>
    /// Saves the point closest to the left and right of an obstacle edge.
    /// </summary>
    public struct EdgeInfo
    {
        public Vector2 pointA;
        public Vector2 pointB;

        public EdgeInfo(Vector2 pointA, Vector2 pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;
        }
    }
}
