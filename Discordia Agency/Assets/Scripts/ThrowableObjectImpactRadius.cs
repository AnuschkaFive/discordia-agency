using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableObjectImpactRadius : MonoBehaviour {
    private float minScaling = 0.1f;
    private float maxScaling = 10.0f;
    private float scalingDuration = 0.5f;
    private float scalePerSecond;

	// Use this for initialization
	void Start () {
        this.transform.localScale = new Vector2(this.minScaling, this.minScaling);
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.localScale += new Vector3(this.scalePerSecond, this.scalePerSecond, 0.0f) * Time.deltaTime;
        if(this.transform.localScale.x >= maxScaling)
        {
            GameObject.Destroy(this.gameObject);
        }
	}

    public void SetMaxScaling(float maxScaling)
    {
        this.maxScaling = maxScaling;
        this.scalePerSecond = (this.maxScaling - this.minScaling) / this.scalingDuration;
    }
}
