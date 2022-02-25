// Dan J Tarazi
// Basic DOTS Setup provided by https://www.undefinedgames.org/2019/10/14/unity-ecs-dots-introduction/

using UnityEngine;
using Unity.Entities;
using System.Collections;
using Unity.Transforms;

/// <summary>
/// ComponentSystem allows for this code to be executed on its own,
/// without being attached to any entities or game objects in Unity
/// </summary>
public class ShipFunc : ComponentSystem
{
    public static readonly float epsilon = 1e-5f;
    public static readonly float dt = 1e-3f;

    // Formula for a box on an axis normal to one of its axes
    protected float BoxInertia(float m, float perpendicular_size_1, float perpendicular_size_2)
    {
	    return m*(perpendicular_size_1*perpendicular_size_1 + perpendicular_size_2*perpendicular_size_2)/(12.0f);
    }

// static const float EPSILON = 0.00000001f;
// Unneeded; Vector3 == Vector3 already has its own internal epsilon (1e-5).

    protected void Init(Ship ship)
	{
        // BASIC ------------
        ship.center = Vector3.zero;
        ship.vel_ang = 0.0f;
        ship.accel_ang = 0.0f;
        ship.ang = 0.0f;
        ship.vel = Vector3.zero;
        ship.accel_ang_axis = Vector3.zero;

        // THRUST ------------
        // A force on a body can impart both linear momentum as well as angular momentum.
        // The amount of force directed towards the center of mass (radial) is linear,
        // And force perpendicular to that (tangential) is angular.


        Vector3 thrust_radius = ship.thrusterAttachmentPoint - ship.center;
        float thrust_radius_mag = thrust_radius.magnitude;    // I need this separately to avoid an expensive crossprod later
        thrust_radius = thrust_radius / thrust_radius_mag;
  
        ship.accel = Vector3.Project(ship.thrustVector, thrust_radius);   // Force applied to translate the ship
        Vector3 thrust_tangential = ship.thrustVector - ship.accel; // Force applied to turn the ship

        ship.accel /= ship.mass; // Make >accel< actually acceleration rather than force.

        if (thrust_tangential == Vector3.zero) // Internal epsilon used
        {
            // Aiming at or away from center of mass. Only linear force, no rotation.
            return;
        }
        else
        {
            // Right hand rule: Thumb is Vec1, Index is Vec2, Middle is Cross Result.
            ship.accel_ang_axis = Vector3.Cross(thrust_tangential, thrust_radius * thrust_radius_mag);
            // INERTIA ------------
            // Simplification.
            // L    W    H
            // X    Y    Z
            float I_x = BoxInertia(ship.mass, ship.width, ship.height); // Inertia for x-rot
            float I_y = BoxInertia(ship.mass, ship.length, ship.height); // Inertia for y-rot
            float I_z = BoxInertia(ship.mass, ship.width, ship.length); // Inertia for z-rot
            // This is always in the positive-positive-positive quadrant. No sign corrections needed.
            Vector3 inertia = new Vector3(I_x, I_y, I_z);

            // accel_ang = torque/inertia
            // and torque = ||F cross R||
            // So, accel_ang = ||accel_ang_axis||/ inertia

            // It may seem that we're missing the chirality sign, but that choice is provided by the 'polarity' of the angle axis; if the angle axis were pointing in the exact opposite direction it initially points, the rotation would flip its handedness.
            ship.accel_ang = ship.accel_ang_axis.magnitude; // Will divide this by inertia in a moment

            // Using the acceleration magnitude here to normalize so I'm not doing it twice
            // Since >inertia< is always positive-positive-positive, the sign entirely relies on >accel_ang_axis<'s "polarity". So this dot product describes the rotation handedness via its sign.
            float I_ang = Vector3.Dot(inertia, ship.accel_ang_axis / ship.accel_ang);    // This is my entirely made-up approximation to save dev time
            
            // Turn our torque into accel
            ship.accel_ang /= I_ang;
        }
        if (ship.accel == Vector3.zero)  // Internal epsilon used
        {
            // Aiming perpendicular to the radius from the ship's center to the thruster. No linear force, only rotation.        
            ship.accel = Vector3.zero; // Set it to definitely 0
            return;
        }
        
        return;
	}

    protected void Move(Ship ship, ref Rotation rot, ref Translation trans) // Called every frame; I don't know the intended setup for lerp/physics framerate in Unity.
    {
        // Thrust ramp
        if ((ship.thrust_factor + ship.thrust_ramp - 1.0f) <= epsilon) // If we can still ramp up to 1.0f
        {
            ship.thrust_factor += ship.thrust_ramp; 
        }
        else if((ship.thrust_factor - 1.0f) <= epsilon) // If the next ramp up would overshoot, set to 1.0f
        {
            ship.thrust_factor = 1.0f;
		}
        
        // Linear 
        ship.vel += ship.accel * ship.thrust_factor; // Scale accel, add to vel
        ship.center += ship.vel; // Move
         transform.Translate(ship.vel*dt);

        // Ang
        ship.vel_ang += ship.accel_ang * ship.thrust_factor; // Scale ang accel, add to ang vel
        ship.ang += ship.vel_ang; // Spin
        //transform.Rotate(ship.vel_ang*dt);
    }


    protected override void OnCreate()
    {
        // Iterate through all entities containing a Ship
        Entities.ForEach((ref Ship ship) =>
        {
            Init(ship);
        });
    }

    protected override void OnUpdate()
    {
        // Iterate through all entities containing a Ship
        Entities.ForEach((ref Ship ship, ref Rotation rot, ref Translation trans) =>
        {
           Move(ship, rot, trans);
        });
    }
}
