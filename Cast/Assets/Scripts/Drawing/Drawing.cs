using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

[System.Serializable]
public class Drawing
{
    // used to compare within Drawing
    struct InternalDrawingCompareData
    {
        public int baseDrawingIndex;
        public int compareDrawingIndex;
        public float distance;
    }

    // used to determine if drawings are within error bounds of matching externally
    public struct ExternalDrawingCompareData
    {
        public int pointCountDiff;
        public int largestModeCount;
        public float avgDistFromMatchingPoints;
    }

    // center will always be defined as (0, 0)
    //Vector2 center = Vector2.zero;
    [SerializeField]
    List<Vector2> normalPoints = new List<Vector2>();
    public List<Vector2> NormalPoints => normalPoints;

    Vector2 size = Vector2.zero;
    public Vector2 Size => size;

    public Drawing(List<Vector3> worldPoints, bool generateDebugs = false)
    {
        if (worldPoints == null) return;

        // DEBUG
        Transform debugContainer = null;

        // generate regression line along x/z:
        Vector3 worldCenter = Vector3.zero;
        foreach (var point in worldPoints)
        {
            worldCenter += point;
        }
        // worldCenter complete: (average of each)
        worldCenter /= worldPoints.Count;
        
        float topSum = 0F, botSum = 0F;
        foreach (var point in worldPoints)
        {
            var x = point.x - worldCenter.x;
            topSum += (x * (point.z - worldCenter.z));
            botSum += (x * x);
        }
        float slope = topSum / botSum;
        Vector3 xzDirectionVector = new Vector3(1, 0, slope);
        xzDirectionVector.Normalize();
        Vector3 xzPerpVector = Vector3.Cross(xzDirectionVector, Vector3.up);
        float angle = Mathf.Atan2(xzDirectionVector.x, xzDirectionVector.z);

        //DEBUG
        if (generateDebugs)
        {
            debugContainer = new GameObject("New Drawing (debug)").transform;
            var drawingDataContainer = new GameObject("Projection Points");
            drawingDataContainer.transform.SetParent(debugContainer);
            var dataPointsContainer = new GameObject("2D Data Points");
            dataPointsContainer.transform.SetParent(debugContainer);

            GameObject debugCenter = new GameObject("Drawing-WorldCenter");
            debugCenter.transform.SetParent(debugContainer);
            debugCenter.transform.position = worldCenter;

            Debug.DrawRay(worldCenter, xzDirectionVector, Color.blue, 500);
            Debug.DrawRay(worldCenter, xzPerpVector, Color.red, 500);
        }

        // now project all the points:
        for (int i = 0; i < worldPoints.Count; i++)
        {
            var worldAtY = new Vector3(worldCenter.x, worldPoints[i].y, worldCenter.z);
            var v = worldPoints[i] - worldAtY;
            var dist = Vector3.Dot(v, xzPerpVector);
            var proj = worldPoints[i] - dist * xzPerpVector;

            //DEBUG
            if (generateDebugs)
            {
                //DEBUG: create points to visualize if its right
                GameObject debug = new GameObject($"debugPoint_{i}");
                debug.transform.SetParent(debugContainer.GetChild(0));
                debug.transform.position = proj;
            }

            // finally, remap the points into 2D coordinates that represent their position on the xz plane with worldCenter == (0, 0)
            // will do this by rotating the 3D point around the worldCenter to the x axis to cancel out the z value
            var translatedProj = proj - worldAtY;
            normalPoints.Add(new Vector2(translatedProj.x * Mathf.Cos(-angle) - translatedProj.z * Mathf.Sin(-angle), proj.y - worldCenter.y));
        }

        //DEBUG: show processed 2D points
        if (generateDebugs)
        {
            foreach (var normal in normalPoints)
            {
                GameObject debug = new GameObject($"normalPoint");
                debug.transform.SetParent(debugContainer.GetChild(1));
                debug.transform.position = normal;
            }
        }

        GenerateBounds();
    }

    /// <summary>
    /// returns a block of data that is the results of a comparision between two drawings
    /// </summary>
    public ExternalDrawingCompareData MatchDrawing(Drawing drawingToCompare)
    {
        //todo: scaling - may want to scale by a ratio instead of both directions seperately
        //Vector2 scaler = size / drawingToCompare.size;
        Vector2 scaler = Vector2.one;
        List<Vector2> scaledComparePoints = new List<Vector2>();
        foreach (var compareNorms in drawingToCompare.normalPoints)
        {
            scaledComparePoints.Add(new Vector2(compareNorms.x * scaler.x, compareNorms.y * scaler.y));
        }

        ExternalDrawingCompareData results = new ExternalDrawingCompareData
        {
            pointCountDiff = Mathf.Abs(drawingToCompare.normalPoints.Count - normalPoints.Count)
        };

        List<InternalDrawingCompareData> compareData = new List<InternalDrawingCompareData>();
        bool mySetLarger = normalPoints.Count > drawingToCompare.normalPoints.Count ? true : false;

        // generate a list using the larger quantity of normalPoints that map each one to the closest point on the other drawing
        for (int i = 0; i < (mySetLarger ? normalPoints.Count : drawingToCompare.normalPoints.Count); i++)
        {
            InternalDrawingCompareData newCompare = new InternalDrawingCompareData
            {
                baseDrawingIndex = i,
                compareDrawingIndex = 0,
                distance = Vector2.Distance(normalPoints[i], drawingToCompare.normalPoints[0])
            };

            for (int j = 1; j < (mySetLarger ? drawingToCompare.normalPoints.Count : normalPoints.Count); j++)
            {
                var newDist = Vector2.Distance(normalPoints[i], drawingToCompare.normalPoints[j]);
                if (newDist < newCompare.distance)
                {
                    newCompare.compareDrawingIndex = j;
                    newCompare.distance = newDist;
                }
            }

            compareData.Add(newCompare);
        }

        results.avgDistFromMatchingPoints = (compareData.Sum(c => c.distance) / compareData.Count);
        results.largestModeCount = compareData.GroupBy(c => c.compareDrawingIndex).OrderByDescending(c => c.Count()).First().Count();

        return results;
    }

    void GenerateBounds()
    {
        float minX = normalPoints[0].x,
            minY = normalPoints[0].y,
            maxX = normalPoints[0].x,
            maxY = normalPoints[0].y;

        for (int i = 1; i < normalPoints.Count; i++)
        {
            if (minX > normalPoints[i].x)
            {
                minX = normalPoints[i].x;
            }
            if (maxX < normalPoints[i].x)
            {
                maxX = normalPoints[i].x;
            }

            if (minY > normalPoints[i].y)
            {
                minY = normalPoints[i].y;
            }
            if (maxY < normalPoints[i].y)
            {
                minY = normalPoints[i].y;
            }
        }

        size = new Vector2(maxX - minX, maxY - minY);
    }
}
