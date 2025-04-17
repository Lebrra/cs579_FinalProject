using UnityEngine;
using UnityEngine.InputSystem;

public class SkyBoxSwitch : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
     public Material skybox_day;
    public Material skybox_night;

    private bool isDay = true;
    private SkyboxController inputActions; 

    private void Awake()
    {
        inputActions = new SkyboxController();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.ToggleSkybox.Skybox.performed += OnToggleSkybox; 
    }

    private void OnDisable()
    {
        inputActions.Disable();
        inputActions.ToggleSkybox.Skybox.performed -= OnToggleSkybox;
    }

    private void OnToggleSkybox(InputAction.CallbackContext context)
    {
        RenderSettings.skybox = isDay ? skybox_night : skybox_day;
        DynamicGI.UpdateEnvironment();
        isDay = !isDay;
    }
}
