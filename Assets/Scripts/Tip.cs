using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tip : MonoBehaviour
{

    [Tooltip("Increase to increase pulling force")] [SerializeField] float springConstant = 200f;
    [Tooltip("Increase to prevent un-anchored overshooting")][SerializeField] float damperConstant = 25f;
    [Tooltip("max pull force that can be exerted on finger rigidbody")] [SerializeField] float maxPullForce = 2000f;


    [SerializeField] Rigidbody2D palmRb;

    Rigidbody2D rb;
    DistanceJoint2D dJ;
    TargetJoint2D tJ;

    const float gravity = -9.81f;

    Vector2 prevMousePosition = Vector2.zero;
    bool isAnchored = false;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        dJ = GetComponent<DistanceJoint2D>();
        tJ = GetComponent<TargetJoint2D>();
        tJ.autoConfigureTarget = false;
        tJ.enabled = false;
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isAnchored = true;
            tJ.enabled = true;
            tJ.target = rb.position;
            damperConstant *= 2;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isAnchored = false;
            tJ.enabled = false;
            damperConstant /= 2;
        }
 

    }

    void FixedUpdate()
    {
        Vector2 mousePosition = GetMouseWorldPosition();
        Vector2 mouseVelocity = (mousePosition - prevMousePosition) / Time.fixedDeltaTime;
        prevMousePosition = mousePosition;

        //calculate PD spring force to apply on finger
        Vector2 position = rb.position;
        Vector2 velocity = rb.velocity;

        Vector2 positionDifference = mousePosition - position;
        Vector2 velocityDifference = mouseVelocity - velocity;

        Vector2 force = springConstant * positionDifference + damperConstant * velocityDifference;
        force = Vector2.ClampMagnitude(force, maxPullForce);

        rb.AddForce(force);
        Vector2 palmForce = (isAnchored) ? tJ.reactionForce : -force;
        palmRb.AddForce(palmForce);
    }

    Vector2 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        return new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawLine(Vector2.zero, Camera.main.ScreenToWorldPoint(Input.mousePosition));
    //    Gizmos.DrawLine(transform.position, (Vector3)((Vector2) transform.position + superForce));
    //}
}
