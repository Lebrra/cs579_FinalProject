using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Random = UnityEngine.Random;

public class Bobber : MonoBehaviour
{
    [SerializeField]
    private GameObject fish = null;
    [SerializeField]
    private ParticleSystem waterParticles = null;
    [SerializeField]
    private Animator anim = null;
    
    [Space, SerializeField]
    private float minRandomCantCatch = 1F;
    [SerializeField]
    private float maxRandomCantCatch = 6F;
    [SerializeField]
    private float minRandomCatch = 1F;
    [SerializeField]
    private float maxRandomCatch = 3F;

    [Space, SerializeField]
    private float hapticAmplitude = 0.5F;
    
    private Rigidbody rb = null;
    public Rigidbody RB
    {
        get
        {
            if (!rb)
            {
                rb = GetComponent<Rigidbody>();
            }
            return rb;
        }
    }
    
    private bool waitingToFish = true;

    private bool catchFish = false;
    
    private Coroutine fishingState = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 4)
        {
            waitingToFish = true;
            if (fishingState != null) StopCoroutine(fishingState);
            fishingState = StartCoroutine(SetCanFish());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 4)
        {
            StopFishing();
        }
    }

    private void OnDisable()
    {
        StopFishing();
        fish?.SetActive(false);
    }
    
    void StopFishing()
    {
        waitingToFish = false;
        if (fishingState != null) StopCoroutine(fishingState);
        SetCatchState(false);
    }

    public bool TryCatchFish()
    {
        bool fishCaught = catchFish;
        PlayHaptics(-1F);
        StopFishing();
        return fishCaught;
    }

    public void ShowFish(bool active)
    {
        fish?.SetActive(active);
    }

    // delaying this set a bit just to ensure we are still in water when we stopped moving
    IEnumerator SetCanFish()
    {
        yield return new WaitUntil(() => Vector3.Magnitude(RB.linearVelocity) < 0.01F);

        if (waitingToFish)
        {
            fishingState = StartCoroutine(Fishing());
        }
    }

    IEnumerator Fishing()
    {
        while (true)
        {
            SetCatchState(false);
            PlayHaptics(-1F);
            yield return new WaitForSeconds(Random.Range(minRandomCantCatch, maxRandomCantCatch));
            
            SetCatchState(true);
            var canCatchTime = Random.Range(minRandomCatch, maxRandomCatch);
            PlayHaptics(canCatchTime - 0.2F);
            yield return new WaitForSeconds(canCatchTime);
        }
    }

    private void PlayHaptics(float duration)
    {
        if (WandDrawer.HeldHand == InteractorHandedness.None)
        {
            InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).StopHaptics();
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand).StopHaptics();
            return;
        }

        XRNode hand = WandDrawer.HeldHand == InteractorHandedness.Left ? XRNode.LeftHand : XRNode.RightHand;
        if (duration <= 0F)
        {
            InputDevices.GetDeviceAtXRNode(hand).StopHaptics();
        }
        else
        {
            InputDevices.GetDeviceAtXRNode(hand).SendHapticImpulse(0u, hapticAmplitude, duration);
        }
        
    }

    private void SetCatchState(bool canCatch)
    {
        catchFish = canCatch;
        Debug.Log("Fish on the line? " + catchFish);
        waterParticles?.gameObject.SetActive(canCatch);
        anim?.SetBool("shake", canCatch);

        // haptics queued in fishing instead since its timing-based
    }
    
    // this could totally be generalized as a static function - this copies the shrink from TubeRenderer.cs
    public IEnumerator TweenOut()
    {
        var shrinkCurve = DrawingManager.Instance?.ShrinkCurve ?? AnimationCurve.Linear(0F, 0F, 1F, 1F);
        
        // shrink
        float fullShrinkTime = DrawingManager.Instance?.CastTime ?? 2F;
        float shrinkTime = fullShrinkTime;
        while (shrinkTime > 0F)
        {
            shrinkTime = Mathf.Clamp(shrinkTime - Time.deltaTime, 0F, shrinkTime);
            var curvePercent = shrinkTime / fullShrinkTime;  // will be going through the curve backwards
            transform.localScale = Vector3.one * (shrinkCurve.Evaluate(curvePercent));
            yield return null;
        }
    }
}
