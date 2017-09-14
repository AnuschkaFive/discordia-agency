using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Based on https://www.youtube.com/watch?v=rZAnnyensgs&index=3&list=PLFt_AvWsXl0ctd4dgE1F8g3uec4zKNRV0.
/// </summary>
public class Gun : MonoBehaviour {
    // Blueprint for the Bullet-Gameobjects that will spawn.
    public Bullet bullet;

    // The rate of fire.
    public float msBetweenShots = 100;

    // The speed of the Bullets fired from this Gun.
    public float gunVelocity = 35;

    // Delay until the next Bullet is fired; depending on msBetweenShots.
    private float nextShotTime;

    /// <summary>
    /// Shoots a Bullet from the Gun.
    /// </summary>
    public void Shoot()
    {
        if (Time.time > nextShotTime)
        {
            nextShotTime = Time.time + msBetweenShots / 1000;
            Bullet newBullet = Instantiate<Bullet>(this.bullet, this.transform.position, this.transform.rotation);
            newBullet.SetSpeed(this.gunVelocity);
        }
    }
}
