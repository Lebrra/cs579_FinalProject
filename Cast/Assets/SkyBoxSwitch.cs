using UnityEngine;
using UnityEngine.UI;    

public class SkyboxSwitcher : MonoBehaviour
{
    [Header("Skybox Materials")]
    [Tooltip("Assign your Day skybox Material here")]
    public Material dayMaterial;
    [Tooltip("Assign your Night skybox Material here")]
    public Material nightMaterial;

    [Header("UI")]
    [Tooltip("Drag your Day/Night Toggle here")]
    public Toggle dayNightToggle;

    void Awake()
    {
        if (dayNightToggle == null)
        {
            Debug.LogError("SkyboxSwitcher: dayNightToggle reference is missing!", this);
            enabled = false;
            return;
        }

    
        dayNightToggle.onValueChanged.AddListener(SetDayMode);

    
        SetDayMode(dayNightToggle.isOn);
    }

  
    public void SetDayMode(bool isDay)
    {
        RenderSettings.skybox = isDay ? dayMaterial : nightMaterial;
        DynamicGI.UpdateEnvironment();
    }

    void OnDestroy()
    {
        if (dayNightToggle != null)
            dayNightToggle.onValueChanged.RemoveListener(SetDayMode);
    }
}