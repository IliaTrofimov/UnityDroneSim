using System;
using UnityEngine;


namespace Utils
{
    public class VelocityDebug : MonoBehaviour
    {
        private Rigidbody body;

        public void Awake()
        {
            body = GetComponent<Rigidbody>();
        }

        public void Update()
        {
            var p0 = body.position;
            Debug.DrawRay(p0, body.linearVelocity, Color.red);
            Debug.DrawRay(p0, body.angularVelocity, Color.blue);
            
            Debug.DrawRay(p0, Vector3.forward, Color.yellow);
            Debug.DrawRay(p0, body.transform.forward, Color.magenta);

        }
    }
}