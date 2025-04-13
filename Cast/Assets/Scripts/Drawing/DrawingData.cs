using Unity.Collections;
using UnityEngine;
// ReSharper disable All

public class DrawingData : ScriptableObject
{
    [SerializeField]
    string drawingName = "none";
    [SerializeField]
    Material drawingMaterial = null;
    [SerializeField]
    Drawing drawing = null;

    public Drawing Drawing => drawing;
    public Material DrawingMaterial => drawingMaterial;

    public void AddDrawing(Drawing newDrawing, string name)
    {
        drawing = newDrawing;
        drawingName = name;
    }
}
