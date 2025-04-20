using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingManager : MonoBehaviour
{
    // Attached to wand; while fishing, listening for 'hook' gesture 
    // on fishing start, select correct bobber and throw it out of spell
    
    public static FishingManager Instance;
    
    [SerializeField]
    private List<Rigidbody> bobbers = new List<Rigidbody>();
    [SerializeField]
    private float bobberThrowForce = 1F;
    
    [SerializeField, Tooltip("Point that is also used in drawing")]
    private Transform wandPoint;
    [SerializeField]
    private float hookThreshold = 1F;
    
    private bool isFishing = false;
    private Rigidbody activeBobber = null;
    private Vector3 lastWandPos = Vector3.zero;
    
    private Coroutine returnTween = null;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(Instance);
        }
        Instance = this;
    }

    private void Start()
    {
        // detach all bobbers from the wand so they don't grab any rotations
        foreach (var bobber in bobbers)
        {
            bobber.transform.SetParent(null);
            bobber.transform.rotation = Quaternion.identity;
            bobber.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isFishing) // && activeBobber != null
        {
            // 'hook' gesture: pull back the wand 'fast enough'
            var currentWandPos = new Vector3(wandPoint.position.x, 0F, wandPoint.position.z);

            var currentBobberPos = new Vector3(activeBobber.transform.position.x, 0F, activeBobber.transform.position.z);
            var direction = lastWandPos - currentBobberPos;
            
            var w = currentWandPos - currentBobberPos;
            
            var projection = (Vector3.Dot(direction, w) / Vector3.Dot(direction, direction)) * direction;
            var point = currentBobberPos + projection;
            
            var dist = Vector3.Distance(currentBobberPos, point) - Vector3.Distance(currentBobberPos, lastWandPos);
            Debug.Log($"dist = {dist}");
            if (dist > hookThreshold)
            {
                isFishing = false;
                returnTween = StartCoroutine(ReturnBobber(activeBobber));
                activeBobber = null;
            }
            
            lastWandPos = currentWandPos;
        }
    }

    public void ThrowBobber(Vector3 startPos, MagicParticleManager.Particle type)
    {
        // todo: different bobbers for different fish
        activeBobber = bobbers[0];
        
        activeBobber.linearVelocity = Vector3.zero;
        activeBobber.transform.position = startPos;
        activeBobber.gameObject.SetActive(true);
        
        var upwardForward = wandPoint.up + Vector3.one * 0.2F;
        upwardForward.Normalize();
        activeBobber.AddForce(upwardForward * bobberThrowForce);

        // if bobber thrown:
        isFishing = true;
        lastWandPos = new Vector3(wandPoint.position.x, 0F, wandPoint.position.z);
    }

    IEnumerator ReturnBobber(Rigidbody bobber)
    {
        // todo: tween bobber back
        yield return new WaitForSeconds(0.5F);
        
        bobber.gameObject.SetActive(false);
    }
}
