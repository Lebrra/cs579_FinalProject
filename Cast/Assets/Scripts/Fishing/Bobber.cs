using System;
using System.Collections;
using UnityEngine;

public class Bobber : MonoBehaviour
{
    [SerializeField]
    private GameObject fish = null;
    
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
            yield return new WaitForSeconds(1F);    // todo: randomize these
            
            SetCatchState(true);
            yield return new WaitForSeconds(3F);
        }
    }

    private void SetCatchState(bool canCatch)
    {
        catchFish = canCatch;
        Debug.Log("Fish on the line? " + catchFish);
        // todo: haptics and other fun things
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
