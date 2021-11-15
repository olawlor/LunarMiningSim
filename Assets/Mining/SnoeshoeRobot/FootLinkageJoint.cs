/*
 Drive a robot foot using a ConfigurableJoint, not transform.localPosition
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootLinkageJoint : MonoBehaviour
{
    private ExcahaulerDriveSide side; // side of the vehicle we follow
    private ConfigurableJoint joint; // our joint to the robot
    
    public float radius=0.1f; // foot motion radius, in meters
    public float phase=0.0f; // 0 == forward.  90 == foot lift.  180 == foot back. 270 == foot down.
    float phaseRate=300.0f; // degrees of foot phase per second of 100% speed travel
    
    // Start is called before the first frame update
    void Start()
    {
        side=GetComponentInParent<ExcahaulerDriveSide>();
        joint=GetComponent<ConfigurableJoint>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        // See what phase we're supposed to be in:
        float dir = phaseRate * side.targetSpeed/100.0f * side.direction;
        if (dir==0) { // user not commanding us--back to rest position (synchronizes feet)
            if (Mathf.Abs(phase)<0.01f) return; // nothing to do
            dir = (phase>0)?-phaseRate:phaseRate;
        }
        float phaseDelta = dt * dir;
        phase += phaseDelta;
        while (phase>=180.0f) phase-=360.0f;
        while (phase<-180.0f) phase+=360.0f;
        
        // Move the foot to match the target phase:
        float s = Mathf.Sin(phase*Mathf.Deg2Rad);
        float c = Mathf.Cos(phase*Mathf.Deg2Rad);
        //transform.localPosition=new Vector3(0,radius*s,radius*c);
        joint.targetPosition = //new Vector3(0,radius*s,radius*c); // rest position forward
            new Vector3(0,-radius*c,-radius*s); // rest position up
    }
}

