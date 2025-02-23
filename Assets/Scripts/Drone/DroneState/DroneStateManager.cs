using Unity.Mathematics;
using UnityEngine;
using Utils;
using UtilsDebug;


namespace Drone.DroneState
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class DroneStateManager : MonoBehaviour
    {
        private DroneState state;
        private Rigidbody rigidBody;
        
        private ContactPoint[] contactPoints;
        private int actualCollisions;
        private GizmoOptions gizmoOptions = new (Color.magenta);
        private GizmoOptions gizmoLegs = new (Color.cyan);

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            contactPoints = new ContactPoint[10];
            if (gameObject.TryGetDimensions(out Vector3 bounds))
            {
                gizmoOptions.capSize = math.cmin(bounds) / 4;
                gizmoOptions.vectSize = math.cmax(bounds) / 6;
                
                gizmoLegs.capSize = math.cmin(bounds) / 4;
                gizmoLegs.vectSize = math.cmax(bounds) / 6;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            actualCollisions = collision.GetContacts(contactPoints);
        }

        private void OnCollisionExit(Collision other)
        {
            actualCollisions = 0;
        }

        private void OnDrawGizmos()
        {
            for (var i = 0; i < actualCollisions; i++)
            {
                var contact = contactPoints[i];
                if (contact.thisCollider.isTrigger) 
                    VectorDrawer.DrawPoint(contact.point, options: gizmoLegs);
                else
                    VectorDrawer.DrawPoint(contact.point, options: gizmoOptions);

            }
        }
    }
    
}