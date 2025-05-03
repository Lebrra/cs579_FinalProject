using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// idk how any of Unity's stuff is setup, so let's just use this little thing for our needs :)
public class VRUIClicker : MonoBehaviour
{
    [SerializeField]
    LayerMask uiLayer;
    
    [SerializeField]
    InteractorHandedness handedness;
    InputDevice trackingHand;
    bool triggerDown = false;

    VolumeToggle activeVolume = null;
    
    Coroutine raycasting;
    
    [SerializeField]
    AudioClip clickSFX;

    private void Start()
    {
        XRNode hand = handedness == InteractorHandedness.Left ? XRNode.LeftHand : XRNode.RightHand;
        trackingHand = InputDevices.GetDeviceAtXRNode(hand);
    }

    private void OnEnable()
    {
        if (raycasting != null) StopCoroutine(raycasting);
        raycasting = StartCoroutine(DelayedPhysicsCheck());
    }

    private void OnDisable()
    {
        if (raycasting != null) StopCoroutine(raycasting);
        raycasting = null;
        triggerDown = false;
    }


    // in an ienumerator to delay calls
    IEnumerator DelayedPhysicsCheck()
    {
        // test current click state:
        bool clicked = false;
        trackingHand.TryGetFeatureValue(CommonUsages.triggerButton, out clicked);
        bool doClick = !clicked && triggerDown;
        triggerDown = clicked;
        
        
        var hits = Physics.RaycastAll(transform.position, transform.forward, uiLayer);

        if (activeVolume)
        {
            // check if no longer hovering
            if (hits.All(h => h.transform.GetComponent<VolumeToggle>() != activeVolume))
            {
                activeVolume = null;
            }
        }
        
        // hovering any?
        foreach (var hit in hits)
        {
            var volume = hit.transform.GetComponent<VolumeToggle>();
            if (volume)
            {
                // select the first one 
                activeVolume = volume;
                break;
            }
        }

        // now see if we are clicking on it
        if (activeVolume && doClick)
        {
            activeVolume.Toggle();
            if (clickSFX) AudioManager.Instance?.PlayClip(clickSFX);
        }
        
        yield return new WaitForSeconds(0.1F);
    }
}
