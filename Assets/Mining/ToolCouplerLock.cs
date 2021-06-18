/* 
 Attached to the Excahauler coupler horn,
 this latches onto nearby tools.
*/
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolCouplerLock : MonoBehaviour
{
    // Talk to our robot and UI
    public ExcahaulerDriver excahauler;
    
    
    // This is the object held in the coupler, or null if none.
    public GameObject coupled;
    public FixedJoint coupledJoint;
    public GameObject lastCoupled;

    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    
    // This is called when something enters our locking box.
    void OnTriggerEnter (Collider c)
     {
         GameObject newTool=c.gameObject;
         //Debug.Log("OnCollisionEnter checking tags ToolCouplerLock\n");
         if (newTool.tag=="CouplerTool") 
         {
            Debug.Log("OnCollisionEnter hit in ToolCouplerLock\n");
            if (coupled) { 
                Debug.Log(" ... but ToolCouplerLock already coupled\n"); 
                return;
            }
            
            Rigidbody RB=newTool.GetComponentInParent<Rigidbody>();
            if (!RB) { 
                Debug.Log(" ... but ToolCouplerLock couldn't find RB\n"); 
                return;
            }
            
            // See: https://answers.unity.com/questions/867610/adding-joints-through-script.html
            //coupledJoint = gameObject.AddComponent<FixedJoint>();  
            //coupledJoint.connectedBody = RB;
            excahauler.toolRB=RB;
            coupled = newTool;
         }
     }
     
     // FIXME: UI tells us to let go.
}

