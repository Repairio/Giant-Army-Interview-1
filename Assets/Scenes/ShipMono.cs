// Dan J Tarazi

using UnityEngine;
using Unity.Entities;

public class ShipMono : MonoBehaviour
{
    protected readonly float epsilon = 1e-5f;

    public float length, width, height = 1.0f; //dimensions of box. X, Y, Z respectively.    
    public float mass = 1.0f; //mass of box
    public Vector3 thrusterAttachmentPoint = Vector3.zero;
    public Vector3 thrustVector = new Vector3(0, 1, 0);

    public float thrustRampTimeSecs = 0.1f; // Seconds for thrust to ramp linearly from 0% to 100% of >thrustVector<. Seconds.
    

    protected float thrust_factor = 0.0f; // The actual thrust force/accel multiplier which will lerp from 0 to 1 over >thrustRampTimeSecs< seconds.

    protected Vector3 pos = Vector3.zero; // Initially at the owl (0,0), but, you know, 3D (0,0,0).

    protected Vector3 ang = Vector3.zero; // Uses the axis defined at the end

    protected Vector3 vel = Vector3.zero;
    protected Vector3 vel_ang = Vector3.zero; // Uses the axis defined at the end


    protected Vector3 accel = Vector3.zero;
    protected Vector3 accel_ang = Vector3.zero; // Uses the axis defined at the end

    protected Vector3 torque = Vector3.zero; // Angle axis

    // Formula for a box on an axis normal to one of its axes
    protected float BoxInertia(float m, float perpendicular_size_1, float perpendicular_size_2)
    {
	    return m*(perpendicular_size_1*perpendicular_size_1 + perpendicular_size_2*perpendicular_size_2)/(12.0f);
    }

    protected void Start()
    {
        // BASIC ------------
        pos = Vector3.zero;
        vel_ang = Vector3.zero;
        accel_ang = Vector3.zero;
        ang = Vector3.zero;
        vel = Vector3.zero;
        torque = Vector3.zero;

        if(length <= epsilon)
        {	length = 1.0f;}
        if(height <= epsilon)
        {	height = 1.0f;}
        if(width <= epsilon)
        {	width = 1.0f;}
        if(mass <= epsilon)
        {	mass = 1.0f;}
        if(thrustRampTimeSecs <= epsilon)
        {	thrustRampTimeSecs = 0.1f;}
        if(thrustVector == Vector3.zero)
        {	thrustVector = new Vector3(0, 1, 0);}
	
       transform.localScale = new Vector3(length, height, width);

        // THRUST ------------
        // A force on a body can impart both linear momentum as well as angular momentum.
        // The amount of force directed towards the pos of mass (radial) is linear,
        // And force perpendicular to that (tangential) is angular.

        Vector3 thruster_radius = thrusterAttachmentPoint - pos;
        float thruster_radius_mag = thruster_radius.magnitude;    // I need this separately to avoid an expensive crossprod later
        if(Mathf.Abs(thruster_radius_mag) <= epsilon)
        {    thruster_radius_mag = 1.0f;}
        thruster_radius = thruster_radius / thruster_radius_mag;
  
        accel = Vector3.Project(thrustVector, thruster_radius);   // Force applied to translate the ship

        Vector3 thrust_tangential = thrustVector - accel; // Force applied to turn the ship

        accel /= mass; // Make >accel< actually acceleration rather than force.

        if (thrust_tangential == Vector3.zero || (Mathf.Abs(thruster_radius_mag) <= epsilon)) // Internal epsilon used
        {
            // Aiming at or away from pos of mass. Only linear force, no rotation.
            return;
        }
        else
        {
            // Right hand rule: Thumb is Vec1, Index is Vec2, Middle is Cross Result.
            torque = Vector3.Cross(thrust_tangential, thruster_radius * thruster_radius_mag);
            // INERTIA ------------
            // Simplification.
            // L    H    W
            // X    Y    Z
            float I_x = BoxInertia(mass, width, height); // Inertia for x-rot
            float I_y = BoxInertia(mass, width, length); // Inertia for y-rot
            float I_z = BoxInertia(mass, length, height); // Inertia for z-rot

            // As with the Poincare sphere and polarized light, we should allow rotations to create multimodal oscillations, i.e. Lissajous figures.
            // Basically - objects spin on each principal axis, not on the axis defined by the force. At least in this easy approximation.
           accel_ang = new Vector3(torque[0]/I_x, torque[1]/I_y, torque[2]/I_z);

        }
        if (accel == Vector3.zero)  // Internal epsilon used
        {
            // Aiming perpendicular to the radius from the ship's pos to the thruster. No linear force, only rotation.        
            accel = Vector3.zero; // Set it to definitely 0
            return;
        }
        
        return;
    }

    protected void Update() // Called every frame; I don't know the intended setup for lerp/physics framerate in Unity.
    {
        // Thrust ramp
        if ((thrust_factor + thrustRampTimeSecs - 1.0f) <= epsilon) // If we can still ramp up to 1.0f
        {
            thrust_factor += thrustRampTimeSecs; 
        }
        else if((thrust_factor - 1.0f) <= epsilon) // If the next ramp up would overshoot, set to 1.0f
        {
            thrust_factor = 1.0f;
        }
                
        // Linear 
        vel += accel * thrust_factor*Time.deltaTime; // Scale accel, add to vel
        pos += vel*Time.deltaTime; // Move
        transform.Translate(vel*Time.deltaTime);

        // Ang
        vel_ang += accel_ang * thrust_factor*Time.deltaTime; // Scale ang accel, add to ang vel
        ang += vel_ang*Time.deltaTime; // Spin
        transform.Rotate(vel_ang*Time.deltaTime*Mathf.Rad2Deg, Space.Self);
    }
}