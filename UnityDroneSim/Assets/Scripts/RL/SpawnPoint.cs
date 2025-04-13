using Unity.Mathematics;
using UnityEngine;
using Utils;
using UtilsDebug;
using Random = UnityEngine.Random;


namespace RL
{
    [ExecuteInEditMode]
    public class SpawnPoint : MonoBehaviour
    {
        [Header("Create from reference")]
        [Range(0.01f, 20f)]
        [Tooltip("Spawn point bounds will use this value as its height.")]
        public float defaultHeight = 1f;

        [Tooltip("Spawn point bounds will use XZ dimensions of this object.")]
        public GameObject referenceObject;


        [Header("Create manually")]
        [Tooltip("Spawn point bounds. Objects will be spawned at random position inside these bounds.")]
        public Bounds spawnBounds = new(Vector3.zero, Vector3.one);

        [Range(0f, 360f)]
        [Tooltip("Default object spawn rotation.")]
        public float neutralYRotation;

        [Range(0f, 360f)]
        [Tooltip("Possible deviation of object spawn rotation.")]
        public float spawnRotationDeviation = 30f;


        private readonly GizmoOptions _gizmoOptions = new()
        {
            Color = Color.cyan, LabelColor = Color.cyan, CapSize = 0.1f, LabelOutline = true
        };

        private void OnValidate()
        {
            if (referenceObject && referenceObject.TryGetDimensions(out Vector3 dimensions))
            {
                neutralYRotation = referenceObject.transform.eulerAngles.y;

                spawnBounds.center.Set(
                    referenceObject.transform.position.x,
                    referenceObject.transform.position.y + defaultHeight / 2,
                    referenceObject.transform.position.z
                );

                spawnBounds.extents.Set(
                    dimensions.x / 2,
                    defaultHeight / 2,
                    dimensions.z / 2
                );

                referenceObject = null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            var color = Gizmos.color;
            Gizmos.color = _gizmoOptions.Color;

            Gizmos.DrawWireCube(spawnBounds.center, spawnBounds.size);

            var direction = new Vector3(math.cos(neutralYRotation * math.TORADIANS),
                0,
                math.sin(neutralYRotation * math.TORADIANS)
            );

            VectorDrawer.DrawDirection(spawnBounds.center,
                direction,
                options: new GizmoOptions { Color = _gizmoOptions.Color, CapSize = 0 }
            );

            Gizmos.color = color;
        }

        private void OnDrawGizmos()
        {
            VectorDrawer.DrawPointCube(spawnBounds.center, $"Spawn Point: {name}", _gizmoOptions);
        }

        public void MoveInsideSpawnPoint(Transform targetTransform)
        {
            targetTransform.position = new Vector3
            {
                x = Random.Range(spawnBounds.min.x, spawnBounds.max.x),
                y = Random.Range(spawnBounds.min.y, spawnBounds.max.y),
                z = Random.Range(spawnBounds.min.z, spawnBounds.max.z)
            };

            targetTransform.eulerAngles = new Vector3
            {
                x = 0,
                y = Random.Range(neutralYRotation - spawnRotationDeviation / 2,
                    neutralYRotation + spawnRotationDeviation / 2
                ),
                z = 0
            };
        }
    }
}