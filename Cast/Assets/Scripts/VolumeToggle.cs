using System;
using UnityEngine;
using UnityEngine.UI;

public class VolumeToggle : MonoBehaviour
{
    Toggle toggle = null;

    private void Start()
    {
        toggle = GetComponent<Toggle>();
    }

    public void Toggle()
    {
        if (toggle)
        {
            toggle.isOn = !toggle.isOn;
        }
    }
}
