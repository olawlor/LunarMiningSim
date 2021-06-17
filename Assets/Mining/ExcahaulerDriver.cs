﻿/*
 Hook this script to a robot to make it walk-up driveable:
   - A Trigger collider is used to detect player walk-ups.
 
 
 WASD drive the robots' 2 wheels.
 E raises the elbow
 W lowers the elbow
 

 Orion Lawlor, lawlor@alaska.edu, 2020-07 (Public Domain)
*/
using System.Collections;
using System.Collections.Generic;
using Assets.Source.Controls;
using UnityEngine;

public class ExcahaulerDriver : MonoBehaviour, IVehicleMotionScheme
{
    public float driveTorque=30.0f; // N-m wheel torque at normal driving speed
    
    // This is how we drive around
    public ExcahaulerDriveSide driveL, driveR;
    private RobotDriveWheels _drive;
    
    // These are the arm links:
    public GameObject boom;
    public GameObject stick;
    public GameObject coupler;
    public GameObject wrist;
    public GameObject cameraArm;
    
    // This gives the rotation of each link:
    public float[] config=new float[5]; // {0.0f,0.0f,0.0f,0.0f,0.0f};
    
    // Start is called before the first frame update
    void Start()
    {
        config[1]=0.4f;
        
        _drive=gameObject.GetComponent<RobotDriveWheels>();
    }
    
    private int _restartCooldown=0; // suppress vehicle entry if we just exited it
    
    // OnTriggerEnter is called when our "walk up to drive" trigger gets hit
    void OnTriggerEnter(Collider other) 
    {
        if (_restartCooldown<=0 && other.gameObject.tag=="Mobile")
        { // Player has entered our "begin driving" trigger area
            Debug.Log("Start driving ground vehicle!");
            StartDriving(other.gameObject);
        }
    }
    
    // This object would like to drive us around
    void StartDriving(GameObject player) {
        if (_isDriving) return; // we're already driving
        
        _player=player;
        
        // Register our motion with the player's vehicle manager
        _mgr = player.GetComponent<IVehicleManager>();
        if (_mgr!=null) {
            _mgr.PushVehicle(this);
            Debug.Log("Found vehicle manager");
        } else {
            Debug.Log("WARNING: NO vehicle manager found");
        }
        _isDriving=true;
        _justStarted=true;
        
        _smoothCamera=_player.transform.position;
        _player.GetComponent<Rigidbody>().velocity=new Vector3(0,0,0); // stop player momentum
        _player.GetComponent<Collider>().enabled=false; // stop player collisions
        
    }
    
    // State variables used while driving (only in Start/Stop driving)
    private bool _isDriving=false;
    private bool _justStarted=false;
    private GameObject _player;
    private IVehicleManager _mgr;
    private Vector3 _smoothCamera;

    
    // Stop driving this vehicle (jump out)
    void StopDriving() {
        if (!_isDriving) return; // we've already stopped
        
        _restartCooldown=30;
        
        // Teleport player back a bit (this is tricky, collisions are incredibly violent)
        _player.transform.position=gameObject.transform.position 
            - 2.0f*gameObject.transform.forward  // move just behind the robot (safest place?)
            + (new Vector3(0,3.0f,0));
        
        // re-enable player collisions (*after* pushing them back.)
        _player.GetComponent<Collider>().enabled=true; 
        
        // Unregister from the player's controls
        _mgr.PopVehicle(); 
        
        _isDriving=false;
    }
    
    void FixedUpdate() {
        if (_restartCooldown>0) _restartCooldown--;
    }

    // IVehicleMotionScheme interface

    // Physics forces
    public void VehicleFixedUpdate(ref UserInput ui,Transform playerTransform,Transform cameraTransform)
    {
        if (ui.jump) { // || gameObject.transform.up.y<0.0f) {
            StopDriving();
            return;
        }
        
        // Handle driving commands
        float L=0.0f, R=0.0f;
        if (ui.move.magnitude>0.01f) {
            float forward=ui.move.x; //WS throttle
            float turn=ui.move.z*0.5f; //AD steering
            L=driveTorque*(forward-turn); // torque for left wheel
            R=driveTorque*(forward+turn); // right wheel
            if (ui.sprint) {
                L*=3.0f;
                R*=3.0f;
            }
            // Debug.Log("Motor drive torques: "+L+", "+R);
        }
        if (driveL) driveL.targetSpeed=-L; //<- left side motors are rotated around other way
        if (driveR) driveR.targetSpeed=R;
        
        // Keyboard control for the arm is the H through L keys (moving "up"),
        //   and the ones below that (moving "down").
        /*
           I O     <- spin wrist ccw, cw
         J  K  L  <- raise
         M  ;  ,  <- lower
               boom
            stick
         coupler
        */
        
        float angleSpeed=0.003f;
        float boomScale=0.4f,stickScale=0.4f;
        
        float boomD=0.0f, stickD=0.0f, couplerD=0.0f, wristD=0.0f, camD=0.0f; 
        if (Input.GetKey(KeyCode.L)) boomD+=boomScale*angleSpeed;
        if (Input.GetKey(KeyCode.Period)) boomD-=boomScale*angleSpeed;
        
        if (Input.GetKey(KeyCode.K)) stickD+=stickScale*angleSpeed;
        if (Input.GetKey(KeyCode.Comma)) stickD-=stickScale*angleSpeed;
        
        if (Input.GetKey(KeyCode.J)) couplerD+=angleSpeed;
        if (Input.GetKey(KeyCode.M)) couplerD-=angleSpeed;
        
        if (Input.GetKey(KeyCode.I)) wristD+=angleSpeed;
        if (Input.GetKey(KeyCode.O)) wristD-=angleSpeed;
        
        if (Input.GetKey(KeyCode.F)) camD+=angleSpeed;
        if (Input.GetKey(KeyCode.V)) camD-=angleSpeed;
        
        config[0]+=boomD;
        config[1]+=stickD;
        config[2]+=couplerD;
        config[3]+=wristD;
        config[4]+=camD;
        
        // don't clamp wrist, but clamp everything else
        for (int i=0;i<3;i++) config[i]=Mathf.Clamp(config[i],0.0f,1.0f);
        config[4]=Mathf.Clamp(config[4],-1.0f,1.0f);
        
        boom.transform.localRotation=Quaternion.Euler(-125.0f*config[0],0.0f,0.0f);
        stick.transform.localRotation=Quaternion.Euler(-150.0f+125.0f*config[1],0.0f,0.0f);
        coupler.transform.localRotation=Quaternion.Euler(140.0f*config[2],0.0f,0.0f);
        wrist.transform.localRotation=Quaternion.Euler(0.0f,0.0f,180.0f*config[3]);
        cameraArm.transform.localRotation=Quaternion.Euler(0.0f,0.0f,100.0f*config[4]);
        //if (Input.GetKey(KeyCode.K)) stick.transform.Rotate(new Vector3(-1.0f,0.0f,0.0f));
        //if (Input.GetKey(KeyCode.J)) coupler.transform.Rotate(new Vector3(-1.0f,0.0f,0.0f));
        //if (Input.GetKey(KeyCode.H)) wrist.transform.Rotate(new Vector3(0.0f,0.0f,-1.0f));
        
        /*
        // Manually keyboard-command the robot arm
        float elbow=0, wrist=0;
        if (Input.GetKey(KeyCode.R) && _arm.elbowTarget<272.0f) elbow=+1.0f;
        if (Input.GetKey(KeyCode.F) && _arm.elbowTarget>40.0f) elbow=-1.0f;
        
        if (Input.GetKey(KeyCode.Q)) wrist=+1.0f;
        if (Input.GetKey(KeyCode.E)) wrist=-1.0f;
        
        _arm.elbowTarget+=elbow*_arm.elbowMoveRate*Time.deltaTime;
        _arm.wristTarget+=wrist*_arm.wristMoveRate*Time.deltaTime;
         */
    }

    private Quaternion _smoothRot;
    
    public void VehicleUpdate(ref UserInput ui,Transform playerTransform,Transform cameraTransform)
    {
        if (_justStarted) {
            ui.yaw=0; // robot is oriented differently than you
            _justStarted=false;
        }
        ui.pitch=Mathf.Clamp(ui.pitch,-80,+80);
        ui.yaw=Mathf.Clamp(ui.yaw%360.0f,-180,+180);
    
        // The player essentially is the robot now
        float speedZoomout=gameObject.GetComponent<Rigidbody>().velocity.magnitude;
        //ui.yaw*=Mathf.Clamp(1.0f-speedZoomout*Time.deltaTime,0.5f,1.0f); // drop yaw to zero when driving
        float radius=2.4f + 0.1f*speedZoomout;
        Quaternion look_yaw=Quaternion.Euler(0.0f, ui.yaw-90.0f, 0.0f); // Player is +X forward, we are +Z forward
        playerTransform.position=gameObject.transform.position
            +(gameObject.transform.forward*0.7f) // in main work area
            +(new Vector3(0.0f,-0.5f,0)); // compensate for camera's +1.8m in y
        
        // Smooth camera rotations
        float smoothR=1.0f-3.0f*Time.deltaTime;
        _smoothRot = Quaternion.Lerp(gameObject.transform.rotation,_smoothRot,smoothR);
        playerTransform.rotation=_smoothRot*look_yaw;
        playerTransform.position-=radius*playerTransform.right;
        
        // Smooth camera moves
        Vector3 newPosition=playerTransform.position;
        float smooth=1.0f-3.0f*Time.deltaTime;
        _smoothCamera=_smoothCamera*smooth+(1.0f-smooth)*newPosition;
        playerTransform.position=_smoothCamera;
        
        
        // The camera is the only thing that moves for mouse look
        cameraTransform.localRotation = Quaternion.Euler(ui.pitch,90,0);
        
        
    }
    
}
