using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableObjectPowerMeter : MonoBehaviour {

    private float minScale = 0.1f;
    private float maxScale = 1.5f;

    private SpriteRenderer spriteRenderer;

    void Start () {
        spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
    }

    public void displayPowerMeter()
    {
        spriteRenderer.enabled = true;
    }

    public void hidePowerMeter()
    {
        spriteRenderer.enabled = false;
    }

    public void updatePowerMeter(float percentage)
    {
        this.transform.localScale = calcScaling(percentage);
    }

    private Vector2 calcScaling(float percentage)
    {
        float scaling = this.minScale + (this.maxScale - this.minScale) * percentage;
        return new Vector2(scaling, scaling);
    }
}
