using UnityEngine;


namespace Utils
{
    public static class GameObjectUtils
    {
        /// <summary>Try to measure GameObject's size using its meshes.</summary>
        /// <remarks>
        /// This method searches through any Mesh in this object and its children and sets
        /// <c>minPoint</c> and <c>maxPoint</c> as vertices with minimal and maximal coordinates in all meshes.
        /// </remarks>
        /// <returns>True if object has mesh and size can be calculated, otherwise false.</returns>
        public static bool TryGetDimensions(this GameObject gameObject, out Vector3 minPoint, out Vector3 maxPoint)
        {
            minPoint = maxPoint = Vector3.zero;
            MeshRenderer[] meshes = gameObject?.GetComponents<MeshRenderer>();
            MeshRenderer[] childMeshes = gameObject?.GetComponentsInChildren<MeshRenderer>();

            if ((meshes == null || meshes.Length == 0) && (childMeshes == null || childMeshes.Length == 0))
                return false;

            minPoint = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            maxPoint = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            if (meshes?.Length > 0)
            {
                minPoint = meshes[0].bounds.min;
                maxPoint = meshes[0].bounds.max;
                for (var i = 1; i < meshes.Length; i++)
                {
                    var bounds = meshes[i].bounds;
                    minPoint = Vector3.Min(minPoint, bounds.min);
                    maxPoint = Vector3.Max(maxPoint, bounds.max);
                }
            }

            if (childMeshes?.Length > 0)
                for (var i = 0; i < childMeshes.Length; i++)
                {
                    var bounds = childMeshes[i].bounds;
                    minPoint = Vector3.Min(minPoint, bounds.min);
                    maxPoint = Vector3.Max(maxPoint, bounds.max);
                }

            return true;
        }

        /// <summary>Try to measure GameObject's size using its meshes in local coordinates.</summary>
        /// <remarks>
        /// This method searches through any Mesh in this object and its children and sets
        /// <c>minPoint</c> and <c>maxPoint</c> as vertices with minimal and maximal coordinates in all meshes.
        /// </remarks>
        /// <returns>True if object has mesh and size can be calculated, otherwise false.</returns>
        public static bool TryGetLocalDimensions(this GameObject gameObject, out Vector3 minPoint, out Vector3 maxPoint)
        {
            if (TryGetDimensions(gameObject, out minPoint, out maxPoint))
            {
                minPoint = gameObject.transform.InverseTransformPoint(minPoint);
                maxPoint = gameObject.transform.InverseTransformPoint(maxPoint);
                return true;
            }

            return false;
        }

        /// <summary>Try to measure GameObject's size using its meshes in local coordinates.</summary>
        /// <remarks>
        /// This method searches through any Mesh in this object and its children and sets
        /// <c>minPoint</c> and <c>maxPoint</c> as vertices with minimal and maximal coordinates in all meshes.
        /// </remarks>
        /// <returns>True if object has mesh and size can be calculated, otherwise false.</returns>
        public static bool TryGetLocalDimensions(this GameObject gameObject, out Vector3 axis)
        {
            if (!TryGetLocalDimensions(gameObject, out var minPoint, out var maxPoint))
            {
                axis = Vector3.zero;
                return false;
            }

            axis = (maxPoint - minPoint).Abs();
            return true;
        }

        /// <summary>Try to measure GameObject's size using its meshes.</summary>
        /// <remarks>Resulting vector <c>axis</c> is length of given object in XYZ dimensions.</remarks>
        /// <returns>True if object has mesh and size can be calculated, otherwise false.</returns>
        public static bool TryGetDimensions(this GameObject gameObject, out Vector3 axis)
        {
            if (!TryGetDimensions(gameObject, out var minPoint, out var maxPoint))
            {
                axis = Vector3.zero;
                return false;
            }

            axis = (maxPoint - minPoint).Abs();
            return true;
        }

        /// <summary>Try to measure GameObject's size using its meshes.</summary>
        /// <remarks>Resulting <c>length</c> is diagonal of box bounding object.</remarks>
        /// <returns>True if object has mesh and size can be calculated, otherwise false.</returns>
        public static bool TryGetDimensions(this GameObject gameObject, out float length)
        {
            var result = TryGetDimensions(gameObject, out Vector3 axis);
            length = axis.magnitude;
            return result;
        }
    }
}