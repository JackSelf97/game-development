using UnityEngine.Audio;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public static AudioManager aMan;

    // Start is called before the first frame update
    void Awake()
    {
        if (aMan == null)
        {
            aMan = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
            
        DontDestroyOnLoad(gameObject);

        foreach (Sound item in sounds)
        {
            item.source = gameObject.AddComponent<AudioSource>();
            item.source.clip = item.clip;
            item.source.volume = item.volume;
            item.source.pitch = item.pitch;
            item.source.loop = item.loop;
        }
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Level 1")
            Play("Theme");
    }

    public void Play(string name)
    {
        Sound item = Array.Find(sounds, sound => sound.name == name);

        if (item == null)
        {
            Debug.LogError("Sound: " + name + " not found! Have you spelt it correctly?");
            return;
        }
            
        item.source.Play();

        // trigger sound in code
        //FindObjectOfType<AudioManager>().Play("PlayerDeath");
    }

    public void Stop(string name)
    {
        Sound item = Array.Find(sounds, sound => sound.name == name);
        if (item == null)
        {
            Debug.LogError("Sound: " + name + " not found! Have you spelt it correctly?");
            return;
        }
        item.source.Stop();
    }
}
