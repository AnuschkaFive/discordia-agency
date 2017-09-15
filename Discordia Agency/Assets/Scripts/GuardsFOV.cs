using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Based on https://www.youtube.com/watch?v=rQG9aUWarwE, https://www.youtube.com/watch?v=73Dc5JTCmKI.
/// </summary>
public class GuardsFOV : MonoBehaviour {
    [Range(0, 360)]
    public float viewAngle;
    [Range(0, 10)]
    public float viewRadius;
    [Range(0, 1)]
    public float reducedViewRadius;

    [Range(1, 3)]
    public float alertnessFactor;

    public float meshResolution;
    public int edgeResolveIterations;
    public float edgeDstThreshold;

    public MeshFilter viewMeshFilterRegular;
    private Mesh viewMeshRegular;
    public MeshFilter viewMeshFilterReduced;
    private Mesh viewMeshReduced;
    private MeshRenderer viewMeshRendererRegular;

    private LayerMask playerMask;
    private LayerMask obstacleMask;
    private LayerMask guardMask;


    private GameObject player;

    [HideInInspector]
    public List<Transform> visibleGuards = new List<Transform>();

    [HideInInspector]
    public List<Transform> visiblePlayers = new List<Transform>();

    // Use this for initialization
    void Start () {
        this.playerMask = LayerMask.GetMask("Player");
        this.obstacleMask = LayerMask.GetMask("Obstacles");
        this.guardMask = LayerMask.GetMask("Guards");
        this.player = GameObject.Find("Player").gameObject;
        this.viewMeshRegular = new Mesh();
        this.viewMeshRegular.name = "ViewMeshRegular";
        this.viewMeshFilterRegular.mesh = viewMeshRegular;
        this.viewMeshReduced = new Mesh();
        this.viewMeshReduced.name = "ViewMeshReduced";
        this.viewMeshFilterReduced.mesh = viewMeshReduced;
        this.viewMeshRendererRegular = this.GetComponentsInChildren<MeshRenderer>(true)[0];
    }
	
	/// <summary>
    /// Late update is called after inputs have been executed.
    /// </summary>
	void LateUpdate () {
        this.UpdateVisibleTargets();
        this.UpdateFieldsOfView();
	}

    /// <summary>
    /// Increase the view radius by a specific factor.
    /// </summary>
    public void SetAlertedFOV()
    {
        this.viewRadius *= this.alertnessFactor;
        this.reducedViewRadius *= this.alertnessFactor;
    }

    /// <summary>
    /// Updates the list of visible Players (with different radius, depending on whether they are disguised or not)
    /// and the list of knocked out Guards.
    /// </summary>
    private void UpdateVisibleTargets()
    {
        this.visiblePlayers = this.player.GetComponent<Player>().isDisguised ? 
            this.FindVisibleTargets(this.playerMask, this.viewRadius * this.reducedViewRadius) : 
            this.FindVisibleTargets(this.playerMask, this.viewRadius); 

        this.visibleGuards = this.FindVisibleTargets(this.guardMask, this.viewRadius);
    }

    private void UpdateFieldsOfView()
    {
        this.viewMeshRegular.Clear();
        this.viewMeshReduced.Clear();
        this.DrawFieldOfFiew(this.viewMeshRegular, this.viewRadius);
        if (this.player.GetComponent<Player>().isDisguised)
        {
            this.viewMeshRendererRegular.material = Resources.Load<Material>("Materials/Sprite-FOV-Disguised");
            this.DrawFieldOfFiew(this.viewMeshReduced, this.viewRadius * this.reducedViewRadius);
        }
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
    /// <param name="mask">The specified layer on which GameObjects are located.</param>
    /// <param name="viewRadius">The specified view radius in which GameObjects are located.</param>
    /// <returns>List with all found GameObjects.</returns>
    private List<Transform> FindVisibleTargets(LayerMask mask, float viewRadius)
    {
        List<Transform> visibleTargets = new List<Transform>();
        Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(this.transform.parent.position, viewRadius, mask);
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector2 dirToTarget = (target.position - this.transform.parent.position).normalized;
            if(Vector2.Angle(this.transform.parent.up, dirToTarget) < this.viewAngle / 2)
            {
                float distToTarget = Vector2.Distance(this.transform.parent.position, target.position);

                if(!Physics2D.Raycast(this.transform.parent.position, dirToTarget, distToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                    //Debug.Log(target.gameObject.name + " ran into " + this.transform.parent.gameObject.name + "'s View!");
                }
            }
        }
        return visibleTargets;
    }

    /// <summary>
    /// Visualises the FOV.
    /// </summary>
    private void DrawFieldOfFiew(Mesh meshToDraw, float viewRadius)
    {
        int stepCount = Mathf.RoundToInt(this.viewAngle * this.meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector2> viewPoints = new List<Vector2>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = this.transform.parent.eulerAngles.z - this.viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = this.ViewCast(angle, viewRadius);

            if(i > 0)
            {
                // Test against edge distance treshold is necessary in case two obstacles are behind each other.
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if(oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = this.FindEdge(oldViewCast, newViewCast, viewRadius);
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

        meshToDraw.Clear();
        meshToDraw.vertices = (Vector3[])vertices;
        meshToDraw.triangles = triangles;
        meshToDraw.RecalculateNormals();
    }

    /// <summary>
    /// Finds the exact edge (of an obstacle) between two rays that are cast.
    /// </summary>
    /// <param name="minViewCast">The ray to the left of the edge.</param>
    /// <param name="maxViewCast">The ray to the right of the edge.</param>
    /// <param name="viewRadius">The view radius for finding an edge.</param>
    /// <returns>The point closest to the right and to the left of the edge.</returns>
    private EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast, float viewRadius)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector2 minPoint = Vector2.zero;
        Vector2 maxPoint = Vector2.zero;

        for(int i = 0; i < this.edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle, viewRadius);
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
    /// <param name="viewRadius">The view radius for the ray.</param>
    /// <returns>Information about the Viewcast.</returns>
    private ViewCastInfo ViewCast(float globalAngle, float viewRadius)
    {
        Vector2 dir = DirFromAngle(globalAngle, true);
        RaycastHit2D[] hits = new RaycastHit2D[1];
        if(Physics2D.RaycastNonAlloc(this.transform.parent.position, dir, hits, viewRadius, this.obstacleMask) > 0)
        {
            return new ViewCastInfo(true, hits[0].point, hits[0].distance, globalAngle);
        } else
        {
            return new ViewCastInfo(false, (Vector2)this.transform.parent.position + dir * viewRadius, viewRadius, globalAngle);
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
