using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExcahaulerWheelDriver : MonoBehaviour
{
    public ExcahaulerDriveSide side;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Get our hinge joint, apply side.motorTorque.
        float torque=side.motorTorque;
    }
}
