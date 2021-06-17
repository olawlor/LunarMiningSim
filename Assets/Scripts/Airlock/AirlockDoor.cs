﻿/**
 Animate an airlock door moving up and down.

 Original by Orion Lawlor, 2020-07, for Nexus Aurora (Public Domain)
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirlockDoor : MonoBehaviour
{
    public Vector3 closePos=new Vector3(0.0f,0f,0f);
    public Vector3 openPos=new Vector3(0.75f,2.2f,0f);
    public Quaternion closeRot=Quaternion.Euler(0f,0f,0f);
    public Quaternion openRot=Quaternion.Euler(0f,0f,90f);
    
    public float openState=0.0f; // current open state, from 0 to 1
    public float openDir=0.0f; // + for opening, - for closing.
    public float openSpeed=0.8f; // constant open/close speed (in cycles/second)
    private GameObject door; /// The instantiated door object
    
    private bool lastOpen=false;
    private void play(bool opening) {
        if (opening==lastOpen) return; // <- remove repeated calls
        else lastOpen=opening;
        
        AudioSource[] s=door.GetComponentsInChildren<AudioSource>();
        int index=opening?0:1; // index into door's list of AudioSource objects
        if (s[index]) s[index].Play();
    }
    
    public void Open() {
        openDir=+openSpeed;
        play(true);
    }
    public void Close() {
        openDir=-openSpeed;
        play(false);
    }
    
    // Move the graphical door to this location
    private void setOpenClose(float openness)
    {
        door.transform.localPosition=Vector3.Lerp(closePos,openPos,openness);
        door.transform.localRotation=Quaternion.Lerp(closeRot,openRot,openness);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        door=gameObject; // This script is attached to the door itself.
        
        // Move our positions to be relative to our start configuration
        Transform tf=door.transform;
        closePos+=tf.localPosition;
        openPos+=tf.localPosition;
        closeRot=closeRot*tf.localRotation;
        openRot=openRot*tf.localRotation;
        
        
        // Our setup is all in the airlock
        setOpenClose(openState);
    }

    // Update is called once per graphics frame
    void Update()
    {
        float lastOpen=openState;
        openState+=openDir*Time.deltaTime;
        if (openState>1.0f) openState=1.0f;
        if (openState<0.0f) openState=0.0f;
        
        if (openState!=lastOpen) // update the animation
            setOpenClose(openState);
    }
}
