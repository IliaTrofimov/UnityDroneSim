using System;
using Unity.Collections;
using UnityEngine;


namespace FreeSpace
{
    public class AutoDepthFieldSensor : DepthFieldSensor
    {
        public float[,] DepthMap { get; private set; }
        public float MaxDepth { get; private set; }
        public float MinDepth { get; private set; }
        public bool showRays = false;

        public void FixedUpdate()
        {
            DepthMap = GetDepthMap(out var MinDepth, out var MaxDepth);
        }

        public void OnDrawGizmos()
        {
            if (!showRays || DepthMap is null) return;

            var origin = transform.localPosition;
            var rotation = transform.rotation;
            var dx = angleHorizontal / raysHorizontal;
            var x0 = -angleHorizontal / 2f;
            var dy = angleVertical / raysVertical;
            var y0 = -angleVertical / 2f;
                
            for (var i = 0; i < raysVertical; i++)
            {
                var rotY = rotation * Quaternion.AngleAxis(y0 + i * dy, Vector3.up);
                for (var j = 0; j < raysHorizontal; j++)
                {
                    var rotX = Quaternion.AngleAxis(x0 + j * dx, Vector3.right);
                    var direction = rotY * rotX * Vector3.forward * MaxDepth;
                    var color = new Color(DepthMap[i, j], DepthMap[i, j], DepthMap[i, j], 1f);
                        
                    Gizmos.color = color;
                    Gizmos.DrawSphere(direction, 0.1f);
                    Gizmos.DrawRay(origin, direction);
                }       
            }

        }
    }
}