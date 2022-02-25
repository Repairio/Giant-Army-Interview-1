// Dan J Tarazi
// Basic DOTS Setup provided by https://www.undefinedgames.org/2019/10/14/unity-ecs-dots-introduction/

using UnityEngine;
using Unity.Entities;

/// <summary>
/// Note that components are always structs
/// </summary>
public struct Ship : IComponentData
{

    public float length, width, height; //dimensions of box. X, Y, Z respectively.    
    public float mass; //mass of box
    public Vector3 thrusterAttachmentPoint;
    public Vector3 thrustVector; // I am normalizing this because I ramp it up later.

    public float thrust_ramp; // Seconds for thrust to ramp linearly from 0% to 100% of >thrustVector<. Seconds.
    public float thrust_factor; // The actual thrust force/accel multiplier which will lerp from 0 to 1 over >thrust_ramp< seconds.

    public Vector3 center; // Initially at the owl (0,0), but, you know, 3D (0,0,0).

    public float ang; // Uses the axis defined at the end

    public Vector3 vel;
    public float vel_ang; // Uses the axis defined at the end


    public Vector3 accel;
    public float accel_ang; // Uses the axis defined at the end

    public Vector3 accel_ang_axis; // Angle axis
}

