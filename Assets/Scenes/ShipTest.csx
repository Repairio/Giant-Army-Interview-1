// Dan J Tarazi
// Basic DOTS Setup provided by https://www.undefinedgames.org/2019/10/14/unity-ecs-dots-introduction/

using UnityEngine;
using Unity.Entities;

public class ShipTest : MonoBehaviour
{
    private void Start()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        Entity shipEnt = entityManager.CreateEntity(typeof(Ship));


        float length = 1;
        float width = 1;
        float height = 1; //dimensions of box    
        float mass = 1; //mass of box
        Vector3 thrusterAttachmentPoint = new Vector3(2, 2, 2);
        Vector3 thrustVector = new Vector3(2, 2, 2);
        float thrust_ramp = 1.0f/10; // 1/N: takes N seconds to reach peak thrust

        entityManager.SetComponentData(shipEnt, new Ship { length = length, width = width, height = height, mass = mass, thrusterAttachmentPoint = thrusterAttachmentPoint, thrustVector = thrustVector, thrust_ramp = thrust_ramp });
    }
}