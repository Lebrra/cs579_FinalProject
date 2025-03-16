using System.Collections.Generic;
using UnityEngine;

public class Drawing
{
    // center will always be defined as (0, 0)
    //Vector2 center = Vector2.zero;
    List<Vector2> normalPoints = new List<Vector2>();

    Transform debugContainer = null;

    public Drawing(List<Vector3> worldPoints, bool generateDebugs = false)
    {
        if (worldPoints == null) return;

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
    }
}
