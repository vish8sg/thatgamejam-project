using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FingerTip : MonoBehaviour
{

    [SerializeField] Rigidbody2D palmRb;
    [SerializeField] float fingerLength;
    [SerializeField] float fingerSpeed;
    [SerializeField] float pullStrength;
    [Range(0,1)] [SerializeField] float palmVelocityDampening;

    bool isAnchored = false;
    Vector3 anchoredPosition = Vector3.zero;
    Vector3 mousePosition = Vector3.zero;
    Vector3 prevMousePosition = Vector3.zero;

    private void Update()
    {

        //getting target position and making sure it doesn't excede finger length
        prevMousePosition = mousePosition;
        mousePosition = GetMouseWorldPosition();
        //Vector3 mouseDelta = mousePosition - prevMousePosition;

        //getting input
        if (Input.GetMouseButtonDown(0))
        {
            isAnchored = true;
            anchoredPosition = transform.position;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isAnchored = false;
        }    

        if (isAnchored)
        {
            Vector2 toPalm = palmRb.position - (Vector2) anchoredPosition;
            Vector2 mouseDelta = mousePosition - anchoredPosition;
            Vector2 targetPosition = palmRb.position - mouseDelta;

            Vector2 offsetFromAnchor = targetPosition - (Vector2) anchoredPosition;
            if (offsetFromAnchor.magnitude > fingerLength)
            {
                offsetFromAnchor = offsetFromAnchor.normalized * fingerLength;
            }

            Vector2 clampedTargetPositon = (Vector2) anchoredPosition + offsetFromAnchor;

            Vector2 correction = clampedTargetPositon - palmRb.position;
            palmRb.velocity *= palmVelocityDampening; //optional drag to prevent buildup
            palmRb.AddForce(correction * pullStrength, ForceMode2D.Force);

            
        }
        else
        {
            Vector2 direction = (Vector2) mousePosition - palmRb.position;
            if (direction.magnitude > fingerLength)
            {
                direction = direction.normalized * fingerLength;
            }
            Vector2 targetPosition = palmRb.position + direction;
            transform.position = Vector3.Lerp(transform.position, targetPosition, fingerSpeed * Time.deltaTime);
        }

        Debug.Log(transform.position.magnitude);
        Debug.Log(transform.position.magnitude);

    }


    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        mouseWorldPosition.z = 0;
        return mouseWorldPosition;
    }
}
