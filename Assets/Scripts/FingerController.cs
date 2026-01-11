using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerController : MonoBehaviour
{
    public Rigidbody2D palmRb;
    public float maxDistance = 2f;
    public float forceMultiplier = 10f;
    public float dragWhenIdle = 0.95f;

    private Rigidbody2D fingerRb;
    private Vector2 lastMousePos;

    void Start()
    {
        fingerRb = GetComponent<Rigidbody2D>();
        lastMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    void FixedUpdate()
    {
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mouseDelta = mouseWorld - lastMousePos;
        lastMousePos = mouseWorld;

        if (mouseDelta.sqrMagnitude > 0.0001f)
        {
            fingerRb.AddForce(mouseDelta * forceMultiplier, ForceMode2D.Force);
        }
        else
        {
            // Kill drift/spin when idle
            fingerRb.velocity *= dragWhenIdle;
            fingerRb.angularVelocity *= dragWhenIdle;
        }

        // Clamp to max distance from palm
        Vector2 offset = fingerRb.position - palmRb.position;
        float dist = offset.magnitude;
        if (dist > maxDistance)
        {
            fingerRb.position = palmRb.position + offset.normalized * maxDistance;
            fingerRb.velocity = Vector2.zero; // Optional: prevent snap jitter
        }
    }
}
