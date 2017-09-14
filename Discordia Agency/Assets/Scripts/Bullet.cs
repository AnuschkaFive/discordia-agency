using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Based on https://www.youtube.com/watch?v=rZAnnyensgs&index=3&list=PLFt_AvWsXl0ctd4dgE1F8g3uec4zKNRV0, 
/// https://www.youtube.com/watch?v=UnPZyFjUvOM&index=4&list=PLFt_AvWsXl0ctd4dgE1F8g3uec4zKNRV0, 
/// https://www.youtube.com/watch?v=v0zVBtZpB-8&index=5&list=PLFt_AvWsXl0ctd4dgE1F8g3uec4zKNRV0.
/// </summary>
public class Bullet : MonoBehaviour {

    // The movement speed of the Bullet.
    float speed = 50;

    // The layers the Bullet can collide with.
    private LayerMask collisionMask;

    // The amount of damage the Bullet does.
    int damage = 1;

    private void Start()
    {
        this.collisionMask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Obstacles"));
    }

    // Update is called once per frame
    void Update ()
    {
        float moveDistance = this.speed * Time.deltaTime;
        this.CheckCollisions(moveDistance);
        transform.Translate(Vector2.up * Time.deltaTime * speed);
	}

    /// <summary>
    /// Changes the speed of the Bullet.
    /// </summary>
    /// <param name="newSpeed">The new speed of the Bullet.</param>
    public void SetSpeed(float newSpeed)
    {
        this.speed = newSpeed;
    }

    /// <summary>
    /// Finds the target the Bullet hits.
    /// </summary>
    /// <param name="moveDistance">The distance the Bullet will travel this frame.</param>
    private void CheckCollisions(float moveDistance)
    {
        RaycastHit2D[] hit = new RaycastHit2D[1];            
        if(Physics2D.RaycastNonAlloc(this.transform.position, this.transform.up, hit, moveDistance, collisionMask) == 1)
        {
            this.OnHitObject(hit[0]);
        }
    }

    /// <summary>
    /// Tries do damage the object that the Bullet hit. Then despawns the Bullet.
    /// </summary>
    /// <param name="hit">Information about the object that was hit.</param>
    private void OnHitObject(RaycastHit2D hit)
    {
        IDamageable damageableObject = hit.collider.GetComponent<IDamageable>();
        if(damageableObject != null)
        {
            damageableObject.TakeHit(damage, hit);
        }
        GameObject.Destroy(this.gameObject);
    }
}
