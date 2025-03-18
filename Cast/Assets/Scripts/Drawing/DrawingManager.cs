using System.Collections.Generic;
using UnityEngine;

public class DrawingManager : MonoBehaviour
{
    public static DrawingManager Instance = null;


    [SerializeField, Tooltip("Value to be under for a drawing to be considered 'matching' to another")] // TODO: reword this when the number makes sense
    float matchError = 0.01F;
    [SerializeField]
    List<DrawingData> drawingsToMatch = new List<DrawingData>();

    private void Awake()
    {
        if (Instance)
        {
            Destroy(Instance);
        }
        Instance = this;
    }

    public DrawingData TestForDrawingMatch(Drawing drawing)
    {
        // iterate through every drawingsToMatch to test for a match
        // return best-matched drawing data (or none if bad)
        // ideally this disregards size

        List<Drawing.ExternalDrawingCompareData> matchData = new List<Drawing.ExternalDrawingCompareData>();
        foreach (var savedDrawing in drawingsToMatch)
        {
            matchData.Add(savedDrawing.Drawing.MatchDrawing(drawing));
        }

        Debug.Log("done");

        return null;
    }
}
