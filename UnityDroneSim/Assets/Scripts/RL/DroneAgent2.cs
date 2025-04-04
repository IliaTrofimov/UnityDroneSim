using System;
using Drone;
using Drone.Stability;
using Exceptions;
using Navigation;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Utils;


namespace RL
{
    public class DroneAgent2 : Agent
    {
        /// <summary>Drone flight computer.</summary>
        [Header("Drone")]
        public QuadcopterComputer drone;
        
        /// <summary>Waypoint navigation manager.</summary>
        public WaypointNavigator navigator;
        
        private DroneInputsController inputsController;
        private DroneState droneState;
        private Rigidbody droneRigidBody;
        private DroneControlSettings droneControlSettings;
        private int currentWaypointIndex;
        
        [Header("Rewards")] 
        [Range(0f, 1000f)] public float finishReward;
        [Range(0f, 10f)] public float waypointReward;
        [Range(-1000f, 0f)] public float destructionPenalty;

        
        public override void Initialize()
        {
            ExceptionHelper.ThrowIfComponentIsMissing(this, drone, nameof(drone));
            //ExceptionHelper.ThrowIfComponentIsMissing(this, navigator, nameof(navigator));
            
           // if (navigator.drone != drone)
            //    throw new UnityException("WaypointNavigator's target drone is not the same as Agent's drone.");

            inputsController = drone.GetComponent<DroneInputsController>();
            droneState = drone.GetComponent<DroneState>();
            droneRigidBody = drone.rigidBody;
            droneControlSettings = drone.controlSettings;
            
            inputsController.manualInput = false;//
        }
        

        public override void CollectObservations(VectorSensor sensor)
        {
            /*
            if (navigator && navigator.IsFinished)
            {
                ResetTraining(finishReward);
                return;
            }
            */
            if (droneState.AnyMotorsDestroyed)
            {
                //ResetTraining(destructionPenalty);
                //return;
            }
            /*
            if (currentWaypointIndex != navigator.CurrentWaypointIndex)
            {
                currentWaypointIndex = navigator.CurrentWaypointIndex;
                AddReward(waypointReward);
            }
            */
            
            //var distance = (drone.transform.position - navigator.CurrentWaypoint.position).magnitude;
            //var direction = drone.transform.NormalizedHeadingTo(navigator.CurrentWaypoint.position);
            
            sensor.AddObservation(droneState.Landed ? 1 : 0);
            sensor.AddObservation(0);
            sensor.AddObservation(0);
            sensor.AddObservation(droneRigidBody.linearVelocity.x);
            sensor.AddObservation(droneRigidBody.linearVelocity.y);
            sensor.AddObservation(droneRigidBody.linearVelocity.z);
            sensor.AddObservation(droneRigidBody.angularVelocity.x);
            sensor.AddObservation(droneRigidBody.angularVelocity.y);
            sensor.AddObservation(droneRigidBody.angularVelocity.z);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            inputsController.manualInput = false;
            inputsController.SetInputs(
                actions.ContinuousActions[0],
                actions.ContinuousActions[1],
                actions.ContinuousActions[2], 
                actions.ContinuousActions[3]
                );
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            inputsController.manualInput = true;
            actionsOut.ContinuousActions.Array[0] = inputsController.throttle;
            actionsOut.ContinuousActions.Array[1] = inputsController.pitch;
            actionsOut.ContinuousActions.Array[2] = inputsController.yaw;
            actionsOut.ContinuousActions.Array[3] = inputsController.roll;
        }

        private void ResetTraining(float finalReward)
        {
            AddReward(finalReward);
            EndEpisode();
            //navigator.ResetWaypoint();
            droneState.RepairAllMotors();
        }
    }
}