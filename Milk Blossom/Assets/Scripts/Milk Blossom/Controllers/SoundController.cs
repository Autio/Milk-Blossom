using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{

    float soundCooldown = 0;
    float soundCooldownReset = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        soundCooldown -= Time.deltaTime;

    }

    public void PlayerMoveSound()
    { 

        //Debug.Log("Playing player move sound");

        if(soundCooldown >0)
        {
            return;
        }
        AudioClip ac = Resources.Load<AudioClip>("Sounds/PlayerMove");
        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        soundCooldown = soundCooldownReset;
    }

    public void AIMoveSound()
    {

        if (soundCooldown > 0)
        {
            return;
        }
        AudioClip ac = Resources.Load<AudioClip>("Sounds/AIMove");
        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        soundCooldown = soundCooldownReset;
    }

    public void AIPlaceSound()
    {

        if (soundCooldown > 0)
        {
            return;
        }
        AudioClip ac = Resources.Load<AudioClip>("Sounds/AIPlace");
        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        soundCooldown = soundCooldownReset;
    }
    public void MenuMusic()
    {
        AudioClip ac = Resources.Load<AudioClip>("Sounds/AIMove");
        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        soundCooldown = soundCooldownReset;

    }

    public void GamePlayMusic()
    {
        GetComponent<AudioSource>().clip = Resources.Load<AudioClip>("Sounds/GameplayMusic");
    }
}
