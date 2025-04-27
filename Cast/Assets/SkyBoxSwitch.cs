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

   public AudioClip nightSoundClip;
    private AudioManager audioManager;
    private AudioSource nightAudioSource;

    void Awake()
    {
        if (dayNightToggle == null)
        {
            Debug.LogError("SkyboxSwitcher: dayNightToggle reference is missing!", this);
            enabled = false;
            return;
        }

           audioManager = FindFirstObjectByType<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogError("SkyboxSwitcher: AudioManager not found in the scene!", this);
        }

        dayNightToggle.onValueChanged.AddListener(SetDayMode);

    
        SetDayMode(dayNightToggle.isOn);
    }

  
    public void SetDayMode(bool isDay)
    {
        RenderSettings.skybox = isDay ? dayMaterial : nightMaterial;
        DynamicGI.UpdateEnvironment();

            if (!isDay)
        {
         
            if (audioManager != null && nightSoundClip != null)
            {
               
                GameObject soundObject = new GameObject("NightSound");
                nightAudioSource = soundObject.AddComponent<AudioSource>();
                nightAudioSource.clip = nightSoundClip;
                nightAudioSource.loop = true; 
                nightAudioSource.volume = 1f;
                nightAudioSource.spatialBlend = 0f; 
                nightAudioSource.Play();
                DontDestroyOnLoad(soundObject); 
                
            }
        }
         else
        {
            // Switching back to day
            if (nightAudioSource != null)
            {
                nightAudioSource.Stop();
                Destroy(nightAudioSource.gameObject);
                nightAudioSource = null;
            }
        }
    }

    void OnDestroy()
    {
        if (dayNightToggle != null)
            dayNightToggle.onValueChanged.RemoveListener(SetDayMode);
    }
}