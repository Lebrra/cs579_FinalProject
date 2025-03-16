using System.Collections.Generic;
using UnityEngine;

public class DrawingManager : MonoBehaviour
{
    public static DrawingManager Instance = null;

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
        return null;
    }
}
