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
    private List<Bobber> bobbers = new List<Bobber>();
    [SerializeField]
    private float bobberThrowForce = 1F;
    
    [SerializeField, Tooltip("Point that is also used in drawing")]
    private Transform wandPoint;
    [SerializeField]
    private float hookThreshold = 1F;
    
    [Space, SerializeField]
    private float bobberReturnThreshold = 0.2F;
    [SerializeField]
    private float bobberReturnSpeed = 2F;
    
    [SerializeField]
    private float fishShowOffTime = 1F;
    
    private bool isFishing = false;
    private Bobber activeBobber = null;
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

    private void FixedUpdate()
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
        isFishing = false;
        if (returnTween != null) StopCoroutine(returnTween);

        // todo: different bobbers for different fish
        switch (type)
        {
            case MagicParticleManager.Particle.Mountain:
                activeBobber = bobbers[0];
                break;
            case MagicParticleManager.Particle.Wind:
                activeBobber = bobbers[1];
                break;
            default:
                activeBobber = bobbers[2];
                break;
        }
        
        activeBobber.RB.linearVelocity = Vector3.zero;
        activeBobber.RB.useGravity = true;
        activeBobber.transform.position = startPos;
        activeBobber.transform.localScale = Vector3.one;
        activeBobber.ShowFish(false);
        activeBobber.gameObject.SetActive(true);
        
        var upwardForward = wandPoint.up + Vector3.one * 0.1F;
        upwardForward.Normalize();
        activeBobber.RB.AddForce(upwardForward * bobberThrowForce);

        // if bobber thrown:
        isFishing = true;
        lastWandPos = new Vector3(wandPoint.position.x, 0F, wandPoint.position.z);
    }

    IEnumerator ReturnBobber(Bobber bobber)
    {
        // but first did we catch?
        var caught = bobber.TryCatchFish();
        yield return null;

        bobber.RB.useGravity = false;
        if (caught) bobber.ShowFish(true);
        
        // let's accelerate for fun
        float orgDist = Vector3.Distance(bobber.transform.position, wandPoint.position);
        
        float dist = orgDist;
        while (dist > bobberReturnThreshold)
        {
            float percentMoved = 1F - (dist / orgDist);
            
            bobber.transform.position = Vector3.MoveTowards(bobber.transform.position, wandPoint.position, bobberReturnSpeed * (1F + percentMoved));
            dist = Vector3.Distance(bobber.transform.position, wandPoint.position);
            yield return null;
        }
        
        // todo: maybe catch a fish here idk
        if (caught)
        {
            float waitTime = 0F;
            while (waitTime < fishShowOffTime)
            {
                waitTime += Time.deltaTime;
                
                // keep bobber position at wand point
                bobber.transform.position = wandPoint.position;
                yield return null;
            }
            // keep the hovering fish until the tweenout is done
            StartCoroutine(bobber.TweenOut());
            waitTime = DrawingManager.Instance?.CastTime ?? 2F;
            while (waitTime > 0F)
            {
                waitTime -= Time.deltaTime;
                bobber.transform.position = wandPoint.position;
                yield return null;
            }

        }
        
        bobber.gameObject.SetActive(false);
        DrawingManager.CanDraw = true;
    }
}
