using InspectorTools;
using Unity.Mathematics;
using UnityEngine;
using Utils;


namespace UtilsDebug
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class CollisionDebug : MonoBehaviour
    {
        [ReadOnlyField] public int enterCollisionsCount;
        private readonly ContactPoint[] enterCollisionPoints = new ContactPoint[20];
        private GizmoOptions normalColliderGizmo, triggerColliderGizmo;
        
        private void Awake()
        {
            gameObject.TryGetDimensions(out Vector3 dimensions);

            var capSize = math.cmin(dimensions) / 9;
            normalColliderGizmo = new GizmoOptions(Color.green, capSize);
            triggerColliderGizmo = new GizmoOptions(Color.magenta, capSize);
        }

        private void OnCollisionEnter(Collision collision)
        {
            enterCollisionsCount = collision.GetContacts(enterCollisionPoints);
        }

        private void OnCollisionExit(Collision other)
        {
            enterCollisionsCount = 0;
        }

        private void OnDrawGizmos()
        {
            for (var i = 0; i < enterCollisionsCount; ++i)
            {
                VectorDrawer.DrawPoint(
                    enterCollisionPoints[i].point,
                    options: enterCollisionPoints[i].thisCollider.isTrigger
                        ? triggerColliderGizmo
                        : normalColliderGizmo);
            }
        }
    }
}