/*
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
    public bool grabsCamera=true;
    public float driveTorque=30.0f; // N-m wheel torque at normal driving speed
    public float drivePower=50.0f; // user-configurable drive power adjustment (start slow)
    
    // This is how we drive around
    public ExcahaulerDriveSide driveL, driveR;
    
    // These are the arm links:
    public GameObject boom;
    public GameObject stick;
    public GameObject coupler;
    public GameObject wrist;
    public GameObject cameraArm;
    
    // Rigidbody for currently held tool, or null if none.
    public Rigidbody toolRB;
    
    // Link to our ToolCouplerLock here.
    
    public void endCoupled()
    {
        // FIXME: let go of the thing
    }
    
    // This gives the rotation of each link:
    public float[] config=new float[5]; // {0.0f,0.0f,0.0f,0.0f,0.0f};
    
    // Start is called before the first frame update
    void Start()
    {
        //config[1]=0.4f; // stick needs to start halfway up
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
        
        _player.GetComponent<Rigidbody>().velocity=new Vector3(0,0,0); // stop player momentum
        if (grabsCamera) {
            _smoothCamera=_player.transform.position;
            _player.GetComponent<Collider>().enabled=false; // stop player collisions
        }
        
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
        
        if (grabsCamera) {
            // Teleport player back a bit (this is tricky, collisions are incredibly violent)
            _player.transform.position=gameObject.transform.position 
                - 2.0f*gameObject.transform.forward  // move just behind the robot (safest place?)
                + (new Vector3(0,3.0f,0));
            
            // re-enable player collisions (*after* pushing them back.)
            _player.GetComponent<Collider>().enabled=true; 
        }
        
        // Unregister from the player's controls
        _mgr.PopVehicle(); 
        
        _isDriving=false;
    }
    
    void FixedUpdate() {
        if (_restartCooldown>0) _restartCooldown--;
    }


    private void MoveJoint(HingeJoint j,float target)
    {
     // Springs seem to jello no matter how big the spring constant.
        JointSpring s=j.spring;
        s.spring=100000; // good stiff actuation in arm
        s.damper=1000;
        s.targetPosition=target;
        j.spring=s; //<- C# won't let you set one field of a struct (why not?)
        j.useSpring = true;
        j.useLimits = false;
    
    /*
        // https://docs.unity3d.com/ScriptReference/HingeJoint-limits.html
        JointLimits limits = j.limits;
        limits.min = target;
        limits.bounciness = 0;
        limits.bounceMinVelocity = 0;
        limits.max = target+1;
        j.limits = limits;
        j.useSpring = false;
        j.useLimits = true;
        */
    }

    // Physics forces
    public void VehicleFixedUpdate(ref UserInput ui,Transform playerTransform,Transform cameraTransform)
    {
        if (ui.jump) { // || gameObject.transform.up.y<0.0f) {
            StopDriving();
            return;
        }
        if (Input.GetKey(KeyCode.P)) // adjusting drive power with P-number
        {
            if (Input.GetKey(KeyCode.Alpha1)) drivePower=10.0f; 
            if (Input.GetKey(KeyCode.Alpha2)) drivePower=20.0f; 
            if (Input.GetKey(KeyCode.Alpha3)) drivePower=30.0f; 
            if (Input.GetKey(KeyCode.Alpha4)) drivePower=40.0f; 
            if (Input.GetKey(KeyCode.Alpha5)) drivePower=50.0f; 
            if (Input.GetKey(KeyCode.Alpha6)) drivePower=60.0f; 
            if (Input.GetKey(KeyCode.Alpha7)) drivePower=70.0f; 
            if (Input.GetKey(KeyCode.Alpha8)) drivePower=80.0f; 
            if (Input.GetKey(KeyCode.Alpha9)) drivePower=90.0f; 
            if (Input.GetKey(KeyCode.Alpha0)) drivePower=100.0f;
        }
        // Handle driving commands
        float L=0.0f, R=0.0f;
        if (ui.move.magnitude>0.01f) {
            float forward=ui.move.x; //WS throttle
            float turn=ui.move.z*0.5f; //AD steering
            L=drivePower/100.0f*driveTorque*(forward-turn); // torque for left wheel
            R=drivePower/100.0f*driveTorque*(forward+turn); // right wheel
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
        
        if (Input.GetKey(KeyCode.Space)) endCoupled();
        
        config[0]+=boomD;
        config[1]+=stickD;
        config[2]+=couplerD;
        config[3]+=wristD;
        config[4]+=camD;
        
        // don't clamp wrist, but clamp everything else
        for (int i=0;i<3;i++) config[i]=Mathf.Clamp(config[i],0.0f,1.0f);
        config[4]=Mathf.Clamp(config[4],-1.0f,1.0f);
        
        // FIXME: instead of just modifying rotation, apply target to a joint?  Might be more physics-friendly.
        boom.transform.localRotation=Quaternion.Euler(-125.0f*config[0],0.0f,0.0f);
        stick.transform.localRotation=Quaternion.Euler(-150.0f+125.0f*config[1],0.0f,0.0f);
        coupler.transform.localRotation=Quaternion.Euler(140.0f*config[2],0.0f,0.0f);
        wrist.transform.localRotation=Quaternion.Euler(0.0f,0.0f,180.0f*config[3]);
        cameraArm.transform.localRotation=Quaternion.Euler(0.0f,0.0f,100.0f*config[4]);
        
        // After updating all our transforms, make the wrist and the tool be the same.
        ///   (DANG THIS SUCKS -- nothing seems to let me pick up tools)
        ///   HACK: just parent the tool on the end of the arm, bogus but easy and works fine.
        if (toolRB)
        {
            toolRB.MovePosition(wrist.transform.position);
            //toolRB.MoveRotation(wrist.transform.rotation);
        }
        
        /* This seems like it should work better, but results in jello joints.  No.
        MoveJoint(boom,-125.0f*config[0]);
        MoveJoint(stick,-50.0f+125.0f*config[1]);
        */
        
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
        if (!grabsCamera) return; 
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
