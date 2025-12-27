using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FingerTip : MonoBehaviour
{

    /*pull out your notebook to figure out how you want this to work. What I want is for the control while anchored to be precise, but for momentum to be conserved so you can 
    swing when you let go. Figure out how moving the hand invesrse the mouse position (within the constraints) applies to velocity, then to force, and apply it. 
    */

    [SerializeField] Rigidbody2D palmRb;
    [SerializeField] float fingerLength;
    [SerializeField] float fingerSpeed;
    [SerializeField] float pullSoftness;
    [SerializeField] float maxPullStrength;
    [Range(0,1)] [SerializeField] float palmVelocityDampening;

    bool isAnchored = false;
    Vector3 anchoredPosition = Vector3.zero;
    Vector3 mousePosition = Vector3.zero;
    Vector3 anchoredMousePosition = Vector3.zero;

    private void Update()
    {

        //getting target position and making sure it doesn't excede finger 
        mousePosition = GetMouseWorldPosition();
        //Vector3 mouseDelta = mousePosition - prevMousePosition;

        //getting input
        if (Input.GetMouseButtonDown(0))
        {
            isAnchored = true;
            anchoredPosition = transform.position;
            anchoredMousePosition = mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isAnchored = false;
        }

        if (!isAnchored)
        {
            Vector2 direction = (Vector2)mousePosition - palmRb.position;
            if (direction.magnitude > fingerLength)
            {
                direction = direction.normalized * fingerLength;
            }
            Vector2 targetPosition = palmRb.position + direction;
            transform.position = Vector3.Lerp(transform.position, targetPosition, fingerSpeed * Time.deltaTime);
        }

    }

    private void FixedUpdate()
    {
        if (isAnchored)
        {
            Vector2 mouseDelta = mousePosition - anchoredMousePosition;
            Debug.Log(mouseDelta);
            //Vector2 targetPosition = palmRb.position - mouseDelta;
            Vector2 targetPosition = (Vector2) anchoredPosition - mouseDelta;
            Vector2 offsetFromAnchor = targetPosition - (Vector2)anchoredPosition;
            if (offsetFromAnchor.magnitude > fingerLength)
            {
                offsetFromAnchor = offsetFromAnchor.normalized * fingerLength;
            }
            Vector2 clampedTargetPositon = (Vector2)anchoredPosition + offsetFromAnchor;

            Vector2 palmDisplacement = clampedTargetPositon - palmRb.position;

            //everything above is just calculating the intended displacent, change below to change how the force is set
            Vector2 force = palmDisplacement * pullSoftness;

            force = Vector2.ClampMagnitude(force, maxPullStrength);

            palmRb.AddForce(force, ForceMode2D.Force);
            palmRb.velocity *= palmVelocityDampening; //optional drag to prevent buildup

        }
    }


    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        mouseWorldPosition.z = 0;
        return mouseWorldPosition;
    }
}
