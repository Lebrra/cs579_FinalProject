using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for fish movement. Inherits MonoBehavior from Unity
public class FishMovement : MonoBehaviour {

    // These variables apply to movement: speed, turn speed,
    // how often they turn direction, and what the current diection is.
    public float speed = .5f;
    public float turnSpeed = 2f;
    public float changeDirectionInterval = 1.5f;
    public Transform pond;

    private Vector3 targetDirection;
    private Vector3 minBounds;
    private Vector3 maxBounds;

    // Sets a random direction and changes it at a set interval.
    void Start() {
        Vector3 size = pond.localScale / 2;
        minBounds = pond.position - size;
        maxBounds = pond.position + size;
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
        KeepWithinBounds();
    }

    // Sets a random direction
    public void setRandomDirection() {
        targetDirection = new Vector3(Random.Range(-1f,1f), Random.Range(-0.2f, 0.2f), Random.Range(-1f, 1f)).normalized;
    }

    void KeepWithinBounds() {
        Vector3 pos = transform.position;
        if (pos.x < minBounds.x || pos.x > maxBounds.x || pos.y < minBounds.y || pos.y > maxBounds.y || pos.z < minBounds.z || pos.z > maxBounds.z) {
            targetDirection = (pond.position - pos).normalized;
        }

        pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);
        transform.position = pos;
    }
}
