using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class DrawingManager : MonoBehaviour
{
    public static DrawingManager Instance = null;
    public static bool CanDraw = true;
    
    [SerializeField]
    List<DrawingData> drawingsToMatch = new List<DrawingData>();
    
    [SerializeField, ReadOnly]
    List<Drawing.ExternalDrawingCompareData> latestCompareData = new();
    
    [Space]
    [SerializeField, Tooltip("The highest % that a match can have before its considered not good enough for the drawing to match")]
    float maxValidUnmatchedPercent = 0.5F;
    [SerializeField, Tooltip("The max amount of points that can map to a single point on the actual drawing that is good enough for the drawing to match")]
    int maxValidMode = 14;
    [SerializeField, Tooltip("The highest average distance from a point to its assigned matching point that is good enough for the drawing to match")]
    float maxValidDist = 3F;
    
    [Space]
    [SerializeField, Header("Casting Animations")]
    AnimationCurve castingShrinkCurve;
    [SerializeField]
    float castingTime = 2F;
    
    public AnimationCurve ShrinkCurve => castingShrinkCurve;
    public float CastTime => castingTime;

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
        //DebugDrawer.Instance?.DrawAllSavedDrawings(drawingsToMatch);
    }

    public DrawingData TestForDrawingMatch(Drawing drawing, Vector3 position)
    {
        // iterate through every drawingsToMatch to test for a match
        // return best-matched drawing data (or none if bad)
        // ideally this disregards size

        latestCompareData = new List<Drawing.ExternalDrawingCompareData>();
        foreach (var savedDrawing in drawingsToMatch)
        {
            latestCompareData.Add(savedDrawing.Drawing.MatchDrawing(drawing));
        }
        
        int distMatch = 0, vertsMatch = 0, modeMatch = 0;
        for (int i = 1; i < latestCompareData.Count; i++)
        {
            if (latestCompareData[i].avgDistFromMatchingPoints < latestCompareData[distMatch].avgDistFromMatchingPoints)
            {
                distMatch = i;
            }
            
            // smaller unmatched points == more points matched with drawing points
            if (latestCompareData[i].unmatchedPointsCountPercent < latestCompareData[vertsMatch].unmatchedPointsCountPercent)
            {
                vertsMatch = i;
            }
            
            // smallest mode == more points matched uniquely
            if (latestCompareData[i].largestModeCount < latestCompareData[modeMatch].largestModeCount)
            {
                modeMatch = i;
            }
        }

        int match = -1;
        // first check if there are no good options (=> no drawing picked)
        if (latestCompareData[distMatch].avgDistFromMatchingPoints < maxValidDist &&
            latestCompareData[vertsMatch].largestModeCount < maxValidMode &&
            latestCompareData[modeMatch].unmatchedPointsCountPercent < maxValidUnmatchedPercent)
        {
            // if majority on any, then choose that one:
            if (distMatch == vertsMatch || distMatch == modeMatch)
            {
                match = distMatch;
            }
            else if (vertsMatch == modeMatch)
            {
                match = vertsMatch;
            }
            else
            {
                // we are conflicted ! 
                Debug.Log("help");
            }
        }
        
        
        if (match < 0)
        {
            Debug.Log("I don't think any match");
            return null;
        }
        else
        {
            Debug.Log($"I think the closest match is the {drawingsToMatch[match].name}");
            Debug.Log(latestCompareData[match].ToString);
            if (DebugDrawer.Instance)
            {
                DebugDrawer.Instance.DrawDrawing(drawingsToMatch[match].Drawing, position, match);
            }
            return drawingsToMatch[match];
        }
    }
}
