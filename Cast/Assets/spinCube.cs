using UnityEngine;

public class spinCube : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Vector3 RotateAmount;
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
         transform.Rotate(RotateAmount);
    }
}
