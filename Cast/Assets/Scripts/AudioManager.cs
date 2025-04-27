using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    Dictionary<AudioClip, AudioSource> sources = new();
    AudioClip musicClip = null;

    private void Awake()
    {
        if (Instance) Destroy(Instance);
        Instance = this;
        
        var music = GetComponent<AudioSource>();
        if (music)
        {
            musicClip = music.clip;
            sources.Add(musicClip, music);
        }
    }

    public static void ToggleMusic(bool on)
    {
        if (Instance)
        {
            if (Instance.sources.ContainsKey(Instance.musicClip))
            {
                Instance.sources[Instance.musicClip].mute = !on;
            }
        }
    }

    private AudioSource GetSource(AudioClip clip)
    {
        AudioSource source;

        if (sources.ContainsKey(clip))
        {
            // restart the already existing clip
            source = sources[clip];
            source.Stop();
        }
        else
        {
            // create new source to use
            var newChild = Instantiate(new GameObject("Audio Source"), transform);
            source = newChild.AddComponent<AudioSource>();
            sources.Add(clip, source);
            source.loop = false;
        }
        return source;
    }
    
    // plays clip at position of provided parent (assumes movement of parent)
    public void PlayClip(AudioClip clip, Transform parent, float volume = 1F)
    {
        var source = GetSource(clip);
        source.volume = volume;
        if (parent != null) source.transform.SetParent(parent);
        source.transform.localPosition = Vector3.zero;
        source.Play();
    }

    // plays clip at provided position (assumes no movement)
    public void PlayClip(AudioClip clip, Vector3 globalPosition, float volume = 1F)
    {
        var source = GetSource(clip);
        source.volume = volume;
        source.transform.SetParent(null);
        source.transform.position = globalPosition;
        source.Play();
    }
    
    // no position specified == play at camera
    public void PlayClip(AudioClip clip, float volume = 1F)
    {
        var source = GetSource(clip);
        source.volume = volume;
        source.transform.SetParent(Camera.main.transform);
        source.transform.localPosition = Vector3.zero;
        source.Play();
    }
}
