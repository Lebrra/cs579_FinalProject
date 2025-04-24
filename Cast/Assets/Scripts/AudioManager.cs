using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    Dictionary<AudioClip, AudioSource> sources = new Dictionary<AudioClip, AudioSource>();

    private void Awake()
    {
        if (Instance) Destroy(Instance);
        Instance = this;
    }

    public void PlayClip(AudioClip clip, Vector3 position, float volume = 1F)
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

        source.volume = volume;
        if (position == null) source.transform.localPosition = Vector3.zero;
        else source.transform.position = position;
        source.Play();
    }
}
