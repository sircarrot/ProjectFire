using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private int mDelta = 1; // Pixels. The width border at the edge in which the movement work
    private float mSpeed = 3f; // Scale. Speed of the movement

    private Vector3 mRightDirection = Vector3.right; // Direction the camera should move when on the right edge


    void Update()
    {
        // Check if on the right edge
        if (Input.mousePosition.x >= Screen.width - mDelta)
        {
            //Debug.Log("Move");
            // Move the camera
            transform.position += mRightDirection * Time.deltaTime * mSpeed;
        }
    }
}
