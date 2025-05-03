using System;
using UnityEngine;
using UnityEngine.UI;

public class AudioToggle : MonoBehaviour
{
    private void Start()
    {
        var toggle = GetComponent<Toggle>();
        if (toggle)
        {
            AudioManager.ToggleMusic(toggle.isOn);
            toggle.onValueChanged.AddListener(AudioManager.ToggleMusic);
        }
    }
}
