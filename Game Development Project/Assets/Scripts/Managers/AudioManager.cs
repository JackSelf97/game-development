using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager aMan { get; private set; }

    // Start is called before the first frame update
    void Awake()
    {
        aMan = this;
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

    public void OnSelectButtonSound()
    {
        Play("Select");
    }
}