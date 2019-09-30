using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{

    public float speed;
    public float turnSpeed;
    // Update is called once per frame
    void Update()
    {
        float forward = Input.GetAxis("Vertical") * speed;
        float right = Input.GetAxis("Horizontal") * turnSpeed;

        if (forward < 0)
        {
            right = -right;
        }

        transform.position += transform.forward * forward;
        transform.Rotate(Vector3.up, right);
    }
}
