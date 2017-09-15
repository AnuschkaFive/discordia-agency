using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffect : MonoBehaviour {

    private AudioSource audioSource;
    public AudioClip[] audioClips;

	// Use this for initialization
	void Start () {
        this.audioSource = this.GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlaySoundEffect(int noOfSoundEffect)
    {
        if(noOfSoundEffect < this.audioClips.Length)
        {
            Debug.Log("Soundeffect is played!");
            this.audioSource.clip = this.audioClips[noOfSoundEffect];
            this.audioSource.Play();
        }

    }

    public void PlaySoundEffectDelayed(int noOfSoundEffect, float delay)
    {
        if (noOfSoundEffect < this.audioClips.Length)
        {
            StartCoroutine(this.QueueSoundEffect(noOfSoundEffect, delay));
        }
    }

    private IEnumerator QueueSoundEffect(int noOfSoundEffect, float delay)
    {
        yield return new WaitForSeconds(delay);
        this.PlaySoundEffect(noOfSoundEffect);
    }

    public void PauseSoundEffect(int noOfSoundEffect)
    {
        if (noOfSoundEffect < this.audioClips.Length)
        {
            this.audioSource.Pause();
        }
    }

    public void ResumeSoundEffect(int noOfSoundEffect)
    {
        if (noOfSoundEffect < this.audioClips.Length)
        {
            this.audioSource.Pause();
        }
    }

    public void StopSoundEffect(int noOfSoundEffect)
    {
        if (noOfSoundEffect < this.audioClips.Length)
        {
            this.audioSource.Stop();
        }
    }
}
