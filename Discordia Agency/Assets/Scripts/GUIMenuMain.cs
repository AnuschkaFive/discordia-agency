using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GUIMenuMain : MonoBehaviour {

    public Button start;
    public Button help;
    public Button exit;
    public Button gotIt;
    public GameObject startMenu;
    public GameObject helpMenu;

    void Start()
    {
        //this.startMenu = GameObject.Find("Canvas_Main");
        //this.helpMenu = GameObject.Find("Canvas_Help");
        this.start.GetComponent<Button>().onClick.AddListener(ClickStart);
        this.help.GetComponent<Button>().onClick.AddListener(ClickHelp);
        this.exit.GetComponent<Button>().onClick.AddListener(ClickExit);
        this.gotIt.GetComponent<Button>().onClick.AddListener(ClickGotIt);
    }

    void ClickStart()
    {
        if (this.startMenu.activeSelf)
        {
            Debug.Log("Level startet!");
            SceneManager.LoadScene("Level_01");
        }
    }

    void ClickHelp()
    {
        if (this.startMenu.activeSelf)
        {
            this.startMenu.SetActive(false);
            this.helpMenu.SetActive(true);
        }
    }

    void ClickExit()
    {
        if (this.startMenu.activeSelf)
        {
            Debug.Log("Spiel beendet sich");
            Application.Quit();
        }
    }

    void ClickGotIt()
    {
        if (this.helpMenu.activeSelf)
        {
            this.helpMenu.SetActive(false);
            this.startMenu.SetActive(true);
        }
    }
}
