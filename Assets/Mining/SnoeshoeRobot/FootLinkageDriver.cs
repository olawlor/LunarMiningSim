using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootLinkageDriver : MonoBehaviour
{
    private ExcahaulerDriveSide side; // side of the vehicle we follow
    public float radius=0.1f; // foot motion radius, in meters
    public float phase=0.0f; // 0 == forward.  90 == foot lift.  180 == foot back. 270 == foot down.
    float phaseRate=180.0f; // degrees of foot phase per second of 100% speed travel
    
    // Start is called before the first frame update
    void Start()
    {
        side=GetComponentInParent<ExcahaulerDriveSide>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        // See what phase we're supposed to be in:
        float phaseDelta = dt * phaseRate * side.targetSpeed/100.0f * side.direction;
        phase += phaseDelta;
        while (phase>=360.0f) phase-=360.0f;
        while (phase<0.0f) phase+=360.0f;
        
        // Move the foot to match the target phase:
        float s = Mathf.Sin(phase*Mathf.Deg2Rad);
        float c = Mathf.Cos(phase*Mathf.Deg2Rad);
        transform.localPosition=new Vector3(0,radius*s,radius*c);
    }
}

