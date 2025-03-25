using System;
using UnityEngine;


namespace Navigation
{
    [Serializable]
    public struct Waypoint 
    {
        public string name;
        [Range(0.01f, 10f)] 
        public float radius;
        public Vector3 position;

        
        public Waypoint(string name) : this(name, 0.5f, Vector3.zero)
        {}
        
        public Waypoint(string name, Vector3 position) : this(name, 0.5f, position)
        {}

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