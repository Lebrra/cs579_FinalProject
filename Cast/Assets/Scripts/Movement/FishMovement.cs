using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for fish movement. Inherits MonoBehavior from Unity
public class FishMovement : MonoBehaviour {

    // These variables apply to movement: speed, turn speed,
    // how often they turn direction, and what the current diection is.
    public float speed = .5f;
    public float turnSpeed = 2f;
    private float changeDirectionInterval;
    public Transform pond;
    public float minY = 0f;
    public float maxY = 0.2f;
    private Vector3 targetDirection;
    private float pondRadius;

    // Sets a random direction and changes it at a set interval.
    void Start() {
        pondRadius = (pond.localScale.x * 0.5f);
        changeDirectionInterval = Random.Range(0.5f, 5.0f);
        setRandomDirection();
        InvokeRepeating("setRandomDirection", changeDirectionInterval, changeDirectionInterval);
    }

    // Moves fish based on direction and turns
    void Update() {
        // Move fish forward continuously
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        // Rotate toward direction
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        KeepWithinCylinder();
    }

    // Sets a random direction
    public void setRandomDirection() {
        targetDirection = new Vector3(Random.Range(-1f,1f), 0f, Random.Range(-1f, 1f)).normalized;
    }

    void KeepWithinCylinder() {
        Vector3 pos = transform.position;
        Vector3 pondCenterXZ = new Vector3(pond.position.x, 0, pond.position.z);
        Vector3 fishXZ = new Vector3(pos.x, 0, pos.z);
        float distanceFromCenter = Vector3.Distance(fishXZ, pondCenterXZ);

        if (distanceFromCenter > pondRadius) {
            // Steer fish back toward the center if it's outside the radius
            Vector3 toCenter = (pond.position - pos).normalized;
            toCenter.y = targetDirection.y; // Keep vertical movement as is
            targetDirection = toCenter;
        }

        // Clamp vertical position within pond height
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }
}
