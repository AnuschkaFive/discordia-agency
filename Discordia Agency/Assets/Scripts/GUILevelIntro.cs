using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GUILevelIntro : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
        this.transform.GetChild(SceneManager.GetAllScenes()[0].buildIndex).gameObject.SetActive(true);
	}
}
