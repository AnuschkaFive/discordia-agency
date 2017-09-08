using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameStatus
{
    Running, Paused, Lost, Won, Length
}

public class GUIGameStatus : MonoBehaviour {

    GameStatus gameStatus;
    Scene currentLevel;
    Scene pauseMenu;

	// Use this for initialization
	void Start () {
		currentLevel = SceneManager.GetActiveScene();
        Debug.Log(gameStatus);   
    }
	
	// Update is called once per frame
	void Update () {
		if ((this.gameStatus == GameStatus.Lost || this.gameStatus == GameStatus.Won) && Input.GetButtonDown("Restart"))
        {
            Debug.Log("Game is restarted");
            SceneManager.LoadScene(currentLevel.name);
            Time.timeScale = 1.0f;
        }

        if(this.gameStatus <= GameStatus.Paused && Input.GetButtonDown("Pause"))
        {
            this.gameStatus = this.gameStatus == GameStatus.Paused ? GameStatus.Running : GameStatus.Paused;
            Debug.Log("Game is paused: " + (this.gameStatus == GameStatus.Paused));
            Debug.Log("Game is unpaused: " + (this.gameStatus == GameStatus.Running));
            Time.timeScale = (this.gameStatus == GameStatus.Paused) ? 0.0f : 1.0f;
            if (this.gameStatus == GameStatus.Paused)
            {
                SceneManager.LoadScene("Menu_Pause", LoadSceneMode.Additive);
            }
            else
            {
                SceneManager.SetActiveScene(currentLevel);
                SceneManager.UnloadSceneAsync("Menu_Pause");
            }
        }

        if(Input.GetButtonDown("Exit"))
        {
            Application.Quit();
        }

        if(this.gameStatus == GameStatus.Won && Input.GetButtonDown("NextLevel"))
        {
            Debug.Log("Next Level is starting.");
            Time.timeScale = 1.0f;
        }
	}

    public void SetGameStatus(GameStatus gameStatusToChange, bool newStatus)
    {
        if (this.gameStatus == GameStatus.Running && gameStatusToChange == GameStatus.Lost)
        {
            Time.timeScale = 0.0f;
            SceneManager.LoadScene("Menu_Lost", LoadSceneMode.Additive);
        }
        if (this.gameStatus == GameStatus.Running && gameStatusToChange == GameStatus.Won)
        {
            Time.timeScale = 0.0f;
            SceneManager.LoadScene("Menu_Won", LoadSceneMode.Additive);
        }
        this.gameStatus = gameStatusToChange;
    }
}
