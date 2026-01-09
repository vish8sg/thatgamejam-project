using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FingerTargetMover : MonoBehaviour
{
    [SerializeField] Rigidbody2D palmRb;

    TargetJoint2D fingerTargetJoint;
    Rigidbody2D rb;

    bool isAnchored = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        fingerTargetJoint = GetComponent<TargetJoint2D>();
        fingerTargetJoint.enabled = true;
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isAnchored = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isAnchored = false;
        }
            
    }

    private void FixedUpdate()
    {
        if (!isAnchored)
        {
            Vector2 targetPosition = GetMouseWorldPosition();
            fingerTargetJoint.target = targetPosition;
        }
        //palmRb.AddForce(fingerTargetJoint.reactionForce);
        Debug.Log(fingerTargetJoint.reactionForce);
    }

    Vector2 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        return new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);
    }
}
