using System;
using UnityEngine;


namespace Navigation
{
    /// <summary>
    /// Waypoint that contains its name, position and radius.
    /// </summary>
    [Serializable]
    public struct Waypoint
    {
        [Tooltip("Name of the waypoint.")] 
        public string name;

        [Range(0.01f, 10f)]
        [Tooltip("Waypoint can be activated in this radius.")]
        public float radius;

        [Tooltip("Position (world coordinates) of the waypoint.")]
        public Vector3 position;
        
        public Waypoint(string name) : this(name, 0.5f, Vector3.zero) { }

        public Waypoint(string name, Vector3 position) : this(name, 0.5f, position) { }

        public Waypoint(string name, float radius, Vector3 position)
        {
            this.name = name;
            this.radius = radius;
            this.position = position;
        }
        
        public bool ComparePosition(Transform current, out float distance)
        {
            distance = Vector3.Distance(current.position, position);
            return distance <= radius;
        }
    }
}