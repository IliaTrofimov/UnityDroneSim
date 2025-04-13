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
        [ReadOnlyField] 
        public int enterCollisionsCount;
        private readonly ContactPoint[] _enterCollisionPoints = new ContactPoint[20];
        private GizmoOptions _normalColliderGizmo, _triggerColliderGizmo;

        private void Awake()
        {
            gameObject.TryGetDimensions(out Vector3 dimensions);

            var capSize = math.cmin(dimensions) / 9;
            _normalColliderGizmo = new GizmoOptions(Color.green, capSize);
            _triggerColliderGizmo = new GizmoOptions(Color.magenta, capSize);
        }

        private void OnCollisionEnter(Collision collision)
        {
            enterCollisionsCount = collision.GetContacts(_enterCollisionPoints);
        }

        private void OnCollisionExit(Collision other) { enterCollisionsCount = 0; }

        private void OnDrawGizmos()
        {
            for (var i = 0; i < enterCollisionsCount; ++i)
            {
                VectorDrawer.DrawPoint(
                    _enterCollisionPoints[i].point,
                    options: _enterCollisionPoints[i].thisCollider.isTrigger
                        ? _triggerColliderGizmo
                        : _normalColliderGizmo
                );
            }
        }
    }
}