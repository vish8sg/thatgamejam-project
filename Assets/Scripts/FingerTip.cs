using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerTip : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody2D palmRigidbody;
    [SerializeField] Camera mainCamera;

    Rigidbody2D fingerTipRigidbody;
    DistanceJoint2D fingerLengthJoint;
    TargetJoint2D fingerGripJoint;

    [Header("Unanchored Mode: fingertip follows mouse")]
    [Tooltip("Higher = stronger pull toward mouse")]
    [SerializeField] float unanchoredPositionStiffness = 200f;

    [Tooltip("Higher = less overshoot / more damping")]
    [SerializeField] float unanchoredVelocityDamping = 25f;

    [Tooltip("Max force applied to fingertip in unanchored mode")]
    [SerializeField] float unanchoredMaxFingerForce = 2000f;

    [Header("Anchored Mode: palm moves while fingertip grips")]
    [Tooltip("Higher = palm responds faster while anchored")]
    [SerializeField] float anchoredPalmResponseFrequencyHz = 6f;

    [Tooltip("~1 = critically damped (minimal bounce)")]
    [SerializeField] float anchoredPalmResponseDampingRatio = 1f;

    [Tooltip("Cap internal pull force as a fraction of grip strength. <= 1 = mostly stick, > 1 = more slip.")]
    [SerializeField] float anchoredInternalForceLimitFactor = 0.9f;

    [Header("Grip / 'Friction' (TargetJoint2D on fingertip)")]
    [Tooltip("Max force the grip can apply before the fingertip slips")]
    [SerializeField] float gripMaxHoldingForce = 2500f;

    [Tooltip("Higher = stiffer grip (less fingertip movement)")]
    [SerializeField] float gripHoldFrequency = 25f;

    [Tooltip("~1 = minimal grip oscillation")]
    [SerializeField] float gripHoldDampingRatio = 1f;

    bool fingertipIsAnchored;

    // Mouse tracking (unanchored)
    Vector2 previousMouseWorldPosition;

    // Anchor state (anchored)
    Vector2 anchoredFingerWorldPoint;          // world point where fingertip is gripping
    Vector2 mouseWorldPositionAtAnchorStart;   // mouse world position when anchor started
    Vector2 palmToFingerVectorAtAnchorStart;   // (anchored finger point - palm position) at anchor start

    // For anchored feedforward
    Vector2 previousDesiredPalmToFingerVector;
    bool hasPreviousDesiredPalmToFingerVector;

    void Awake()
    {
        fingerTipRigidbody = GetComponent<Rigidbody2D>();
        fingerLengthJoint = GetComponent<DistanceJoint2D>();
        fingerGripJoint = GetComponent<TargetJoint2D>();

        if (!mainCamera) mainCamera = Camera.main;

        fingerGripJoint.autoConfigureTarget = false;
        fingerGripJoint.enabled = false;

        previousMouseWorldPosition = GetMouseWorldPosition();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            StartFingerAnchor();

        if (Input.GetMouseButtonUp(0))
            StopFingerAnchor();
    }

    void StartFingerAnchor()
    {
        fingertipIsAnchored = true;

        anchoredFingerWorldPoint = fingerTipRigidbody.position;
        mouseWorldPositionAtAnchorStart = GetMouseWorldPosition();
        palmToFingerVectorAtAnchorStart = anchoredFingerWorldPoint - palmRigidbody.position;

        fingerGripJoint.enabled = true;
        fingerGripJoint.target = anchoredFingerWorldPoint;
        fingerGripJoint.maxForce = gripMaxHoldingForce;
        fingerGripJoint.frequency = gripHoldFrequency;
        fingerGripJoint.dampingRatio = gripHoldDampingRatio;

        hasPreviousDesiredPalmToFingerVector = false;
    }

    void StopFingerAnchor()
    {
        fingertipIsAnchored = false;
        fingerGripJoint.enabled = false;
    }

    void FixedUpdate()
    {
        Vector2 currentMouseWorldPosition = GetMouseWorldPosition();

        if (!fingertipIsAnchored)
        {
            RunUnanchoredFingerFollow(currentMouseWorldPosition);
            return;
        }

        RunAnchoredPalmPull(currentMouseWorldPosition);
    }

    void RunUnanchoredFingerFollow(Vector2 currentMouseWorldPosition)
    {
        Vector2 mouseWorldVelocity = (currentMouseWorldPosition - previousMouseWorldPosition) / Time.fixedDeltaTime;
        previousMouseWorldPosition = currentMouseWorldPosition;

        // Optional: clamp target within finger length so the joint doesn't go taut and tug the palm.
        Vector2 desiredFingerWorldTarget = currentMouseWorldPosition;
        if (fingerLengthJoint != null && fingerLengthJoint.connectedBody != null)
        {
            float maxFingerLength = fingerLengthJoint.distance;
            Vector2 palmWorldPosition = fingerLengthJoint.connectedBody.position;

            desiredFingerWorldTarget =
                palmWorldPosition + Vector2.ClampMagnitude(currentMouseWorldPosition - palmWorldPosition, maxFingerLength - 0.001f);
        }

        Vector2 fingerToTarget = desiredFingerWorldTarget - fingerTipRigidbody.position;
        Vector2 fingerVelocityError = mouseWorldVelocity - fingerTipRigidbody.velocity;

        Vector2 fingerForce =
            unanchoredPositionStiffness * fingerToTarget +
            unanchoredVelocityDamping * fingerVelocityError;

        fingerForce = Vector2.ClampMagnitude(fingerForce, unanchoredMaxFingerForce);

        fingerTipRigidbody.AddForce(fingerForce, ForceMode2D.Force);

        // Intentionally no palm force here in unanchored mode (prevents unwanted recoil).
    }

    void RunAnchoredPalmPull(Vector2 currentMouseWorldPosition)
    {
        // Keep grip parameters live-tunable
        fingerGripJoint.maxForce = gripMaxHoldingForce;

        Vector2 mouseWorldDeltaSinceAnchor = currentMouseWorldPosition - mouseWorldPositionAtAnchorStart;

        float maxFingerLength = (fingerLengthJoint != null) ? fingerLengthJoint.distance : palmToFingerVectorAtAnchorStart.magnitude;

        // Desired palm->finger vector based on mouse delta
        Vector2 desiredPalmToFingerVector =
            Vector2.ClampMagnitude(palmToFingerVectorAtAnchorStart + mouseWorldDeltaSinceAnchor, maxFingerLength);

        // Desired relative velocity (feedforward) so palm doesn't lag
        Vector2 desiredPalmToFingerVelocity = Vector2.zero;
        if (hasPreviousDesiredPalmToFingerVector)
        {
            desiredPalmToFingerVelocity =
                (desiredPalmToFingerVector - previousDesiredPalmToFingerVector) / Time.fixedDeltaTime;
        }

        previousDesiredPalmToFingerVector = desiredPalmToFingerVector;
        hasPreviousDesiredPalmToFingerVector = true;

        Vector2 currentPalmToFingerVector = fingerTipRigidbody.position - palmRigidbody.position;
        Vector2 currentPalmToFingerVelocity = fingerTipRigidbody.velocity - palmRigidbody.velocity;

        Vector2 relativeVectorError = desiredPalmToFingerVector - currentPalmToFingerVector;
        Vector2 relativeVelocityError = desiredPalmToFingerVelocity - currentPalmToFingerVelocity;

        // Use reduced mass so response stays consistent with different mass ratios
        float fingerMass = fingerTipRigidbody.mass;
        float palmMass = palmRigidbody.mass;
        float reducedMass = (fingerMass * palmMass) / (fingerMass + palmMass);

        float angularFrequency = 2f * Mathf.PI * anchoredPalmResponseFrequencyHz;
        float anchoredStiffness = reducedMass * angularFrequency * angularFrequency;
        float anchoredDamping = 2f * anchoredPalmResponseDampingRatio * reducedMass * angularFrequency;

        Vector2 internalPullForce =
            anchoredStiffness * relativeVectorError +
            anchoredDamping * relativeVelocityError;

        // Cap by grip strength so we don't get controller wars
        float internalForceLimit = gripMaxHoldingForce * anchoredInternalForceLimitFactor;
        internalPullForce = Vector2.ClampMagnitude(internalPullForce, internalForceLimit);

        // Internal action-reaction pair
        fingerTipRigidbody.AddForce(internalPullForce, ForceMode2D.Force);
        palmRigidbody.AddForce(-internalPullForce, ForceMode2D.Force);
    }

    Vector2 GetMouseWorldPosition()
    {
        Vector3 mouseScreen = Input.mousePosition;
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen);
        return new Vector2(mouseWorld.x, mouseWorld.y);
    }
}
