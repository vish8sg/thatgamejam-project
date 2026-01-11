using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    [SerializeField] Rigidbody2D[] fingerTips;
    [SerializeField] Rigidbody2D palm;
    [SerializeField] float fingerLength;

    [Tooltip("Increase to increase pulling force")][SerializeField] float springConstant = 200f;
    [Tooltip("Increase to prevent un-anchored overshooting")][SerializeField] float damperConstant = 25f;
    [Tooltip("max pull force that can be exerted on finger rigidbody")][SerializeField] float maxPullForce = 2000f;

    Vector2 prevMousePositon = Vector2.zero;

    private void FixedUpdate()
    {
        Vector2 mousePosition = GetMouseWorldPosition();
        Vector2 mouseDelta = mousePosition - prevMousePositon;
        Vector2 mouseVelocity = mouseDelta / Time.fixedDeltaTime;
        prevMousePositon = mousePosition;

        Vector2 mouseDirection = mousePosition.normalized;

        //calculate PD spring force to apply on finger
        Vector2 position = fingerTips[0].position;
        Vector2 velocity = fingerTips[0].velocity;

        Vector2 positionDifference = (mouseDirection * fingerLength + palm.position) - position;
        Vector2 velocityDifference = mouseVelocity - velocity;


        Vector2 force = springConstant * positionDifference + damperConstant * velocityDifference;
        force = Vector2.ClampMagnitude(force, maxPullForce);

        AddForceToFinger(force, 0);

        Debug.Log(mousePosition);
        Debug.Log(Vector2.ClampMagnitude(mousePosition, fingerLength));
        Debug.Log(positionDifference);
        
        
    }

    void AddForceToFinger(Vector2 force)
    {
        for (int i = 0; i < fingerTips.Length; i++)
        {
            fingerTips[i].AddForce(force);
        }
        palm.AddForce(-force * fingerTips.Length);
    }

    void AddForceToFinger(Vector2 force, int fingerIndex)
    {
        fingerTips[fingerIndex].AddForce(force);
        palm.AddForce(-force);
    }

    Vector2 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        return new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);
    }
}

