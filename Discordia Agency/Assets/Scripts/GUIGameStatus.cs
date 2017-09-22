using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameStatus
{
    Running, Paused, Starting, Lost, Won, Length
}

public enum Features
{
    KnockOut, Drag, Disguise, Length
}

public class GUIGameStatus : MonoBehaviour {

    GameStatus gameStatus;
    Scene currentLevel;
    Scene pauseMenu;
    AudioSource audioSource;
    public AudioClip huntingClip;
    public AudioClip wonClip;
    public AudioClip lostClip;
    public float soundFadePerFrameInMenu;
    public float soundFadePerFrameChangeBackgroundMusic;
    public float minFadedBackgroundMusicInMenu;
    public float maxBackgroundMusic;
    public bool playerIsBeingHunted = false;
    public bool[] activatedFeatures = new bool[(int)Features.Length];
    

	// Use this for initialization
	void Start () {
		currentLevel = SceneManager.GetActiveScene();
        Time.timeScale = 0f;
        SceneManager.LoadScene("Menu_LevelIntro", LoadSceneMode.Additive);
        this.gameStatus = GameStatus.Starting;
        this.audioSource = this.GetComponent<AudioSource>();
        this.audioSource.volume = minFadedBackgroundMusicInMenu;
    }
	
	// Update is called once per frame
	void Update () {
        if(this.gameStatus == GameStatus.Starting && Input.GetButtonDown("Next"))
        {
            this.gameStatus = GameStatus.Running;
            Time.timeScale = 1.0f;
            SceneManager.UnloadSceneAsync("Menu_LevelIntro");
            StartCoroutine(this.TuneUp(soundFadePerFrameInMenu, maxBackgroundMusic));
        }

		if ((this.gameStatus == GameStatus.Lost && Input.GetButtonDown("Next")) || (this.gameStatus == GameStatus.Won && Input.GetButtonDown("Restart")))
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
                StartCoroutine(this.TuneDown(soundFadePerFrameInMenu, minFadedBackgroundMusicInMenu));
            }
            else
            {
                SceneManager.SetActiveScene(currentLevel);
                SceneManager.UnloadSceneAsync("Menu_Pause");
                StartCoroutine(this.TuneUp(soundFadePerFrameInMenu, maxBackgroundMusic));
            }
        }

        if(Input.GetButtonDown("Exit"))
        {
            Application.Quit();
        }

        if(this.gameStatus == GameStatus.Won && Input.GetButtonDown("Next"))
        {
            Debug.Log("Load next level: " + this.currentLevel.buildIndex);
            SceneManager.LoadScene((this.currentLevel.buildIndex + 1) % 7);
            Time.timeScale = 1.0f;
        }
	}

    public void SetGameStatus(GameStatus gameStatusToChange, bool newStatus)
    {
        if (this.gameStatus == GameStatus.Running && gameStatusToChange == GameStatus.Lost)
        {
            Time.timeScale = 0.0f;
            SceneManager.LoadScene("Menu_Lost", LoadSceneMode.Additive);
            StopAllCoroutines();
            StartCoroutine(this.ChangeBackgroundMusic(this.lostClip, soundFadePerFrameChangeBackgroundMusic));
        }
        if (this.gameStatus == GameStatus.Running && gameStatusToChange == GameStatus.Won)
        {
            Time.timeScale = 0.0f;
            SceneManager.LoadScene("Menu_Won", LoadSceneMode.Additive);
            StopAllCoroutines();
            StartCoroutine(this.ChangeBackgroundMusic(this.wonClip, soundFadePerFrameChangeBackgroundMusic));
        }
        this.gameStatus = gameStatusToChange;
    }

    public void SetSoundtrackToHunting()
    {
        if (!this.playerIsBeingHunted)
        {
            Debug.Log("Soundtrack is being changed!");
            this.playerIsBeingHunted = true;
            StartCoroutine(this.ChangeBackgroundMusic(this.huntingClip, soundFadePerFrameChangeBackgroundMusic));
        }
    }


    private IEnumerator ChangeBackgroundMusic(AudioClip newClip, float fadePerFrame)
    {
        Debug.Log("Coroutine is started!");
        while (this.audioSource.volume > 0.1f)
        {
            this.audioSource.volume -= fadePerFrame;
            yield return null;
        }
        this.audioSource.clip = newClip;
        Debug.Log(this.audioSource.clip.name);
        this.audioSource.volume = 0.0f;
        this.audioSource.Play();
        while (this.audioSource.volume < maxBackgroundMusic)
        {
            this.audioSource.volume += fadePerFrame;
            yield return null;
        }
    }

    private IEnumerator TuneDown(float fadePerFrame, float minVolume)
    {
        while(this.audioSource.volume > minVolume)
        {
            this.audioSource.volume -= fadePerFrame;
            yield return null;
        }
    }

    private IEnumerator TuneUp(float fadePerFrame, float maxVolume)
    {
        while (this.audioSource.volume < maxVolume)
        {
            this.audioSource.volume += fadePerFrame;
            yield return null;
        }
    }
}
