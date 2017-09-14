using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GUIMenuMain : MonoBehaviour {

    public Button start;
    public Button exit;

    void Start()
    {
        start.GetComponent<Button>().onClick.AddListener(ClickStart);
        exit.GetComponent<Button>().onClick.AddListener(ClickExit);
    }

    void ClickStart()
    {
        Debug.Log("Level startet!");
        SceneManager.LoadScene("Level_01");
    }

    void ClickExit()
    {
        Debug.Log("Spiel beendet sich");
        Application.Quit();
    }
}
