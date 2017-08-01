using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityBase : MonoBehaviour {
    const int fieldHeight = 9;
    const int fieldWidth = 8;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    protected void ResizeRelativeToField()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        transform.localScale = new Vector3(1, 1, 1);

        float width = sr.sprite.bounds.size.x;
        float height = sr.sprite.bounds.size.y;


        float worldScreenHeight = Camera.main.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        Vector3 xWidth = transform.localScale;
        xWidth.x = worldScreenWidth / width;

        Vector3 yHeight = transform.localScale;
        yHeight.y = worldScreenHeight / height;

        Vector3 scale = new Vector3(1, 1, 1);
        scale.x = ((worldScreenHeight > worldScreenWidth) ? worldScreenWidth / width : worldScreenHeight / height) / fieldWidth;
        scale.y = ((worldScreenHeight > worldScreenWidth) ? worldScreenWidth / width : worldScreenHeight / height) / fieldHeight;

        transform.localScale = scale;
    }
}
