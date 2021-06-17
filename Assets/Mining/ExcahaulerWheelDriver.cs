using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExcahaulerWheelDriver : MonoBehaviour
{
    public ExcahaulerDriveSide side; // side of the vehicle we follow
    private HingeJoint axle; // axle we should apply power to

    // Start is called before the first frame update
    void Start()
    {
        axle=gameObject.GetComponent<HingeJoint>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Get our hinge joint, apply our speed
        float speed=side.targetSpeed;
        
        // Make the hinge motor rotate with 90 degrees per second and a strong force.
        JointMotor motor = axle.motor;
        motor.force = 4; //<- wheels are light, it doesn't take much?
        motor.targetVelocity = speed*2.0f; // velocity in radians/sec?
        motor.freeSpin = false;
        axle.motor = motor;
    }
}

