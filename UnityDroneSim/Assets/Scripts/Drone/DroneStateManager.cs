using System;
using System.Collections.Generic;
using Drone.Motors;
using UnityEngine;


namespace Drone
{
    /// <summary>Script that controls drone's state. Determines where drone is flying, landed or destroyed.</summary>
    /// <remarks>
    /// Works with simple <see cref="DroneMotor"/> and special <see cref="DestructibleMotor"/> scripts.
    /// Although <see cref="DestructibleMotor"/> provides collider for its propeller and special FX after breaking.
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(DroneComputerBase))]
    public class DroneStateManager : MonoBehaviour
    {
        private DroneComputerBase _droneComputer;
        private readonly Dictionary<int, MotorDestructionInfo> _motorsLookup = new(4);
        private int _destroyedMotorsCount;
        private bool _landed;

        private TagHandle? _instantDeathColliderTagHandle;

        
        /// <summary>All motors have been destroyed.</summary>
        public bool AllMotorsDestroyed => _destroyedMotorsCount == _motorsLookup.Count;

        /// <summary>Some motors have been destroyed.</summary>
        public bool AnyMotorsDestroyed => _destroyedMotorsCount > 0;

        /// <summary>Drone has landed on the ground safely.</summary>
        public bool Landed => _landed;
        

        [Header("Debug")] 
        public bool enableDebugMessages = true;

        
        [Header("Physics")]
        [Tooltip("Colliders with this tag will instantly destroy drone upon collision.")]
        public string instantDeathColliderTag;
        
        [Range(0.1f, 1f)]
        [Tooltip("Minimal value for the dot product of world UP and drone UP vectors to make safe landing.")]
        public float landingDotProduct = 0.55f;

        [Range(0f, 20f)]
        [Tooltip("Maximal collision speed (m/s) that drone hull can survive.")]
        public float hullBreakSpeed = 7f;

        [Range(0f, 20f)]
        [Tooltip("Maximal collision speed (m/s) that motor can survive.")]
        public float motorBreakSpeed = 7f;

        [Tooltip("Add rigidbodies to destroyed motors.")]
        public bool enableBrokenMotorsPhysics = true;

        [Tooltip("Each broken motor will have a Rigidbody attached with such mass (kg).")]
        [Range(0f, 5f)]
        public float brokenMotorMass = 0.1f;

        [Tooltip("Each broken motor will have a Rigidbody attached with such interpolation mode.")]
        public RigidbodyInterpolation brokenMotorInterpolation = RigidbodyInterpolation.None;

        [Tooltip("Each broken motor will have a Rigidbody attached with such collision detection mode.")]
        public CollisionDetectionMode brokenMotorCollisions = CollisionDetectionMode.Continuous;


        [Header("Destruction FX")]
        [Tooltip("Enable destruction effects.")]
        public bool enableEffects = true;

        [Tooltip("This object will appear after motor destruction.")]
        public GameObject motorDestructionPrefab;

        [Tooltip("This object will appear after whole drone destruction.")]
        public GameObject hullDestructionPrefab;


        private void Awake()
        {
            _droneComputer = GetComponent<QuadcopterComputer>();
            if (!string.IsNullOrEmpty(instantDeathColliderTag))
                _instantDeathColliderTagHandle = TagHandle.GetExistingTag(instantDeathColliderTag);
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(instantDeathColliderTag)) return;

            try
            {
                _instantDeathColliderTagHandle = TagHandle.GetExistingTag(instantDeathColliderTag);
            }
            catch (UnityException)
            {
                _instantDeathColliderTagHandle = null;
            }
        }

        private void OnEnable()
        {
            foreach (var motor in _droneComputer.GetAllMotors())
                _motorsLookup[motor.gameObject.GetInstanceID()] = new MotorDestructionInfo(motor);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!enabled || AllMotorsDestroyed || collision.contactCount == 0)
                return;

            if (CheckInstantDeath(collision.gameObject))
                return;

            var contact = collision.GetContact(0);
            var contactSpeed = collision.relativeVelocity.magnitude;
            
            if (CheckDestruction(contactSpeed, contact.thisCollider.gameObject, contact.otherCollider.gameObject))
                return;

            CheckLanding(contactSpeed, contact.normal);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (!Landed) return;

            _landed = false;
            DebugLog("Take-off [{0}]: drone.vel={1:F2} m/s", gameObject.name, collision.relativeVelocity.magnitude);
        }

        private bool CheckInstantDeath(GameObject otherCollider)
        {
            if (!_instantDeathColliderTagHandle.HasValue ||
                !otherCollider.CompareTag(_instantDeathColliderTagHandle.Value))
                return false;

            BreakAllMotors();
            DebugLog("Instant death [{0}] -> [{1}]", gameObject.name, otherCollider.name);
            return true;
        }
        
        private void CheckLanding(float speed, Vector3 contactNormal)
        {
            var dotContact = Vector3.Dot(contactNormal, Vector3.up);
            var dotLocal = Vector3.Dot(transform.up, Vector3.up);

            if (dotContact >= landingDotProduct && dotLocal >= landingDotProduct)
            {
                if (!_landed)
                {
                    DebugLog("Landed [{0}]: drone.vel={1:F2} m/s, normal={2:F2}, dotContact={3:F2}, dotLocal={4:F2} > {5:F2}",
                        gameObject.name,
                        speed,
                        contactNormal,
                        dotContact,
                        dotLocal,
                        landingDotProduct
                    );   
                }
                _landed = true;
            }
            else
            {
                _landed = false;
            }
        }

        private bool CheckDestruction(float contactSpeed, GameObject thisCollider, GameObject otherCollider)
        {
            var colliderParent = thisCollider.transform.parent?.gameObject;

            if (colliderParent && _motorsLookup.TryGetValue(colliderParent.gameObject.GetInstanceID(), out var motorInfo))
            {
                if (motorInfo.Motor.PropellerLinearSpeed + contactSpeed < motorBreakSpeed) 
                    return false;

                DebugLog("Collision (Propeller) [{0}.{1}] -> [{2}]: drone.vel={3:F2} m/s, propeller.vel={4:F2} m/s",
                    colliderParent.name,
                    thisCollider.name,
                    otherCollider.name,
                    contactSpeed,
                    motorInfo.Motor.PropellerLinearSpeed + contactSpeed
                );
                OnMotorCollided(motorInfo);
            }
            else if (_motorsLookup.TryGetValue(thisCollider.GetInstanceID(), out motorInfo))
            {
                if (contactSpeed < motorBreakSpeed) 
                    return false;

                DebugLog("Collision (Motor) [{0}] -> [{1}]: vel={2:F2} m/s",
                    thisCollider.name,
                    otherCollider.name,
                    contactSpeed
                );
                OnMotorCollided(motorInfo);
            }
            else
            {
                if (contactSpeed < hullBreakSpeed) 
                    return false;

                DebugLog("Collision (Hull) [{0}] -> [{1}]: vel={2:F2} m/s",
                    thisCollider.name,
                    otherCollider.name,
                    contactSpeed
                );
                BreakAllMotors();
            }
            
            return true;
        }

        private void OnMotorCollided(MotorDestructionInfo motorInfo, bool addEffects = true)
        {
            if (motorInfo.IsDestroyed) return;

            _destroyedMotorsCount++;
            motorInfo.Motor.enabled = false;
            motorInfo.IsDestroyed = true;

            if (enableBrokenMotorsPhysics)
            {
                motorInfo.Motor.name = "[X] " + motorInfo.Motor.name;
                motorInfo.Motor.transform.parent = null;
                motorInfo.AttachedRigidbody = motorInfo.Motor.gameObject.AddComponent<Rigidbody>();
                
                if (motorInfo.AttachedRigidbody is not null)
                {
                    motorInfo.AttachedRigidbody.mass = brokenMotorMass;
                    motorInfo.AttachedRigidbody.interpolation = brokenMotorInterpolation;
                    motorInfo.AttachedRigidbody.collisionDetectionMode = brokenMotorCollisions;
                    motorInfo.AttachedRigidbody.linearDamping = 0.2f;
                    motorInfo.AttachedRigidbody.angularDamping = 0.1f;
                    motorInfo.AttachedRigidbody.AddRelativeForce(Vector3.up * motorInfo.Motor.liftForce / 5,
                        ForceMode.Force
                    );
                    motorInfo.AttachedRigidbody.AddRelativeTorque(Vector3.up * motorInfo.Motor.liftForce / 5,
                        ForceMode.VelocityChange
                    );
                }   
            }

            if (enableEffects && addEffects)
            {
                if (motorDestructionPrefab)
                    AddMotorDestructionFx(motorInfo.Motor.transform);

                if (AllMotorsDestroyed)
                    AddHullDestructionFx();
            }

            if (motorInfo.Motor is DestructibleMotor destructibleMotor)
                destructibleMotor.OnMotorBroken();
        }

        private void RepairMotor(MotorDestructionInfo motorInfo)
        {
            if (!motorInfo.IsDestroyed) return;

            _destroyedMotorsCount--;
            motorInfo.Motor.transform.SetParent(motorInfo.InitialParent, true);
            motorInfo.Motor.transform.localPosition = motorInfo.InitialLocalPosition;
            motorInfo.Motor.transform.localScale = motorInfo.InitialLocalScale;
            motorInfo.Motor.transform.localRotation = motorInfo.InitialLocalRotation;
            motorInfo.Motor.name = motorInfo.Motor.name.Replace("[X] ", "");
            motorInfo.Motor.enabled = true;
            motorInfo.IsDestroyed = false;

            if (motorInfo.AttachedRigidbody is not null)
                Destroy(motorInfo.AttachedRigidbody);

            if (motorInfo.Motor is DestructibleMotor destructibleMotor)
                destructibleMotor.OnMotorRepaired();

            DebugLog("Motor [{0}] was repaired and reattached to drone [{1}]", motorInfo.Motor.name, gameObject.name);
        }

        [ContextMenu("Repair All Motors")]
        public void RepairAll()
        {
            if (!AnyMotorsDestroyed) return;
    
            foreach (var motorInfo in _motorsLookup.Values)
                RepairMotor(motorInfo);
        }

        [ContextMenu("Break All Motors")]
        public void BreakAllMotors()
        {
            foreach (var motorInfo in _motorsLookup.Values)
                OnMotorCollided(motorInfo, addEffects: false);

            if (enableEffects)
                AddHullDestructionFx();
        }

        private void AddHullDestructionFx()
        {
            if (hullDestructionPrefab)
            {
                var hullFx = Instantiate(hullDestructionPrefab, transform);
                hullFx.name = "[FX] Hull destruction";
            }
            else if (motorDestructionPrefab)
            {
                var hullFx = Instantiate(motorDestructionPrefab, transform);
                hullFx.name = "[FX] Hull destruction";
            }
        }

        private void AddMotorDestructionFx(Transform parent)
        {
            var motorFx = Instantiate(motorDestructionPrefab, parent);
            motorFx.name = "[FX] Motor destruction";
        }

        private void DebugLog(string format, params object[] args)
        {
            if (enableDebugMessages)
                Debug.LogFormat(format, args);
        }
    }
}