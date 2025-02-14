using System;
using Unity.Collections;
using UnityEngine;


namespace FreeSpace
{
    public class DepthFieldSensor : MonoBehaviour
    {
        private NativeArray<RaycastCommand> commands;
        
        [Range(10f, 180f)] public float angleHorizontal = 90f;
        [Range(0f, 180f)] public float angleVertical = 90f;
        [Range(5, 1000)] public int raysHorizontal = 25;
        [Range(1, 1000)] public int raysVertical = 25;
        [Range(1e-3f, 1e3f)] public float maxDistance = 100f;

        public int FieldSize => raysHorizontal * raysVertical; 
        
        
        public void Start() 
        {
            commands = new NativeArray<RaycastCommand>(FieldSize, Allocator.Persistent);
        }

        public float[,] GetDepthMap(out float min, out float max)
        {   
            var origin = transform.localPosition;
            var rotation = transform.rotation;
            
            var dx = angleHorizontal / raysHorizontal;
            var x0 = -angleHorizontal / 2f;
            var dy = angleVertical / raysVertical;
            var y0 = -angleVertical / 2f;
            
            for (int i = 0, k = 0; i < raysVertical; i++)
            {
                var y = y0 + i * dy;
                var rotY = rotation * Quaternion.AngleAxis(y, Vector3.up);
                for (var j = 0; j < raysHorizontal; j++, k++)
                {
                    var x = x0 + j * dx;
                    var rotX = Quaternion.AngleAxis(x, Vector3.right);
                    var direction = rotY * rotX * Vector3.forward;
                    Debug.DrawRay(origin, direction, Color.yellow, 0.1f);

                    commands[k] = new RaycastCommand(origin, direction, new QueryParameters(), maxDistance);
                }       
            }
            
            var rayHits = new NativeArray<RaycastHit>(FieldSize, Allocator.TempJob);
            var handle = RaycastCommand.ScheduleBatch(commands, rayHits, 1);
            handle.Complete();
            
            var map = new float[raysVertical, raysHorizontal];
            min = maxDistance;
            max = 0f;
            
            for (int i = 0, k = 0; i < raysVertical; i++)
            {
                for (var j = 0; j < raysHorizontal; j++, k++)
                {
                    var dist = rayHits[k].distance;
                    map[i, j] = dist;
                    if (map[i, j] < min) min = dist;
                    if (map[i, j] > max) max = dist;
                }       
            }
            
            for (var i = 0; i < raysVertical; i++)
            for (var j = 0; j < raysHorizontal; j++)
                map[i, j] /= max;

            return map;
        }
        
        private void OnDestroy()
        {
            commands.Dispose();
        }
    }
}