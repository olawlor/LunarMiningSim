using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExcahaulerDriveSide : MonoBehaviour
{
    // Vehicles write data here
    // Wheels read this data to talk to their motors
    public float targetSpeed;

    // Start is called before the first frame update
    void Start()
    {
        targetSpeed=0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

