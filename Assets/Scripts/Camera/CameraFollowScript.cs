using Simple_Control;
using UnityEngine;


namespace Camera
{
    public class CameraFollowScript : MonoBehaviour
    {
        private Transform target;
        private DroneContinuousMovement movementScript;
        private Vector3 velocityCameraFollow;
        
        public Vector3 behindPosition = new(0, 2, -4);
        public float angle;
    
        private void Awake()
        {
            target = GameObject.FindGameObjectWithTag("Player").transform; 
            movementScript = GetComponent<DroneContinuousMovement>();
        }
    
        private void Update()
        {
            var heading = Vector3.up * Input.GetAxis("Vertical");
        
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                target.transform.TransformPoint(behindPosition) + heading, 
                ref velocityCameraFollow, 
                0.1f);
        
            transform.rotation = Quaternion.Euler(new Vector3(angle, movementScript.currentYRotation, 0));
        }
    }
}
