using System;
using System.Collections.Generic;
using System.Globalization;
using Drone;
using Exceptions;
using Inputs;
using Navigation;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;


namespace UI
{
    [DisallowMultipleComponent]
    public class FlightHud : MonoBehaviour
    {
        #region UI Elements
        private bool elementsAreInitialized;
        
        private Label lbl_MovementFallback;
        private Label lbl_ControlsFallback;
        private Label lbl_NavigationFallback;

        private Foldout fold_Navigation;
        private Foldout fold_Movement;
        private Foldout fold_Controls;

        private Label lbl_Position;
        private Label lbl_WaypointIndex;
        private Label lbl_WaypointPosition;
        private Label lbl_WaypointDistance;
        private Label lbl_WaypointDirection;
        
        private Label lbl_Velocity;
        private Label lbl_YSpdTarget;
        private Label lbl_YSpdActual;
        private Label lbl_PitchTarget;
        private Label lbl_PitchActual;
        private Label lbl_YawTarget;
        private Label lbl_YawActual;
        private Label lbl_RollTarget;
        private Label lbl_RollActual;

        private Toggle tgl_Stabilization;
        private Slider sld_Throttle;
        private Slider sld_Pitch;
        private Slider sld_Yaw;
        private Slider sld_Roll;
        #endregion

        #region Tracked UI values
        private const float MIN_DIRECTION_CHANGE = 0.1F;
        private const float FLOAT_CHANGE_EPS_PRECISE = 0.001f;
        private const float FLOAT_CHANGE_EPS = 0.01f;

        private readonly TrackedVector3         val_position = new(FLOAT_CHANGE_EPS_PRECISE);
        private readonly TrackedVector3         val_waypointPos = new(FLOAT_CHANGE_EPS_PRECISE);
        private readonly TrackedObject<int>     val_waypointNumber = new();
        private readonly TrackedObject<int>     val_waypointsCount = new();
        private readonly TrackedFloat           val_waypointDist = new(FLOAT_CHANGE_EPS);
        private readonly TrackedObject<Vector3> val_waypointDirection = new();

        private readonly TrackedVector3 val_velocity = new(FLOAT_CHANGE_EPS_PRECISE);
        private readonly TrackedFloat   val_ySpdTarget = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   val_ySpdActual = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   val_pitchTarget = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   val_pitchActual = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   val_yawTarget = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   val_yawActual = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   val_rollTarget = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   val_rollActual = new(FLOAT_CHANGE_EPS);

        private readonly TrackedFloat val_throttle = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat val_pitch = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat val_yaw = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat val_roll = new(FLOAT_CHANGE_EPS);
        private readonly TrackedObject<bool> val_stabilization = new();
        #endregion

        #region Public settings
        
        [Header("UI")]
        public UIDocument hudUIDocument;
        public bool startExpanded = true;
        public bool enableMovementPanel = true;
        public bool enableControlsPanel = true;
        public bool enableNavigationPanel = true;
        private DroneControls controls;
        
        [Header("Cameras")]
        public bool isFirstPersonView;
        public Camera cameraFpv;
        public Camera cameraTpv;
        
        [Header("Target")]
        public QuadcopterComputer drone;                
        public WaypointNavigator navigator;
        private DroneInputsController inputsController;
        private Rigidbody droneRigidbody;
        
        #endregion

        private void OnEnable()
        {
            controls = new DroneControls();
            controls.Enable();
            
            ExceptionHelper.ThrowIfComponentIsMissing(this, hudUIDocument, nameof(hudUIDocument));
            ExceptionHelper.ThrowIfComponentIsMissing(this, drone, nameof(drone));
            
            inputsController = drone.GetComponent<DroneInputsController>();
            droneRigidbody = drone.rigidBody;
            if (!navigator) navigator = GetComponent<WaypointNavigator>();

            if (cameraFpv && cameraTpv)
            {
                cameraFpv.enabled = isFirstPersonView;
                cameraTpv.enabled = !isFirstPersonView;
                cameraFpv.targetDisplay = 0;
                cameraTpv.targetDisplay = 0;
            }
            
            InitializeElements();
        }
        
        private void OnDisable()
        {
            ClearValuesChangedEventHandlers();
            controls.Disable();
        }

        private void Start()
        {
            if (!elementsAreInitialized) return;

            fold_Controls.value = startExpanded;
            fold_Movement.value = startExpanded;
            fold_Navigation.value = startExpanded;
        }

        private void OnValidate()
        {
            if (cameraFpv && cameraTpv)
            {
                cameraFpv.enabled = isFirstPersonView;
                cameraTpv.enabled = !isFirstPersonView;
            }
            if (elementsAreInitialized)
            {
                fold_Controls.visible = enableControlsPanel;
                fold_Movement.visible = enableMovementPanel;
                fold_Navigation.visible = enableNavigationPanel;
            }
        }

        private void Update()
        {
            if (!enabled) return;

            if (controls.Default.CameraMode.WasPressedThisFrame() && cameraFpv && cameraTpv)
            {
                cameraFpv.enabled = !cameraFpv.enabled;
                cameraTpv.enabled = !cameraTpv.enabled;
                isFirstPersonView = cameraFpv.enabled;
            }
            
            if (!elementsAreInitialized) InitializeElements();

            if (UpdateControlsPanel())
            {
                UpdateMovementPanel();
                UpdateNavigationPanel();   
            }
        }

        private bool UpdateControlsPanel()
        {
            if (!drone)
            {
                fold_Navigation.value = false;
                fold_Navigation.enabledSelf = false;
                fold_Controls.value = false;
                fold_Controls.enabledSelf = false;
                fold_Movement.value = false;
                fold_Movement.enabledSelf = false;
                return false;
            }
            
            fold_Controls.enabledSelf = true;
            if (controls.Default.ControlsPanel.WasPressedThisFrame())
                fold_Controls.value = !fold_Controls.value;
            
            val_throttle.Value = inputsController.throttle;
            val_pitch.Value = inputsController.pitch;
            val_yaw.Value = inputsController.yaw;
            val_roll.Value = inputsController.roll;
            val_stabilization.Value = inputsController.IsFullStabilization();

            return true;
        }
        
        private bool UpdateMovementPanel()
        {
            if (drone.controlSettings && droneRigidbody)
            {
                fold_Movement.enabledSelf = true;
                if (controls.Default.MovementPanel.WasPressedThisFrame())
                    fold_Movement.value = !fold_Movement.value;
                
                val_velocity.Value = droneRigidbody.linearVelocity;
                val_ySpdTarget.Value = inputsController.throttle * drone.controlSettings.maxLiftSpeed;
                val_ySpdActual.Value = droneRigidbody.linearVelocity.y;
                val_pitchTarget.Value = inputsController.pitch * drone.controlSettings.maxPitchAngle;
                val_pitchActual.Value = drone.transform.rotation.eulerAngles.x;
                val_yawTarget.Value = inputsController.yaw * drone.controlSettings.maxYawSpeed;
                val_yawActual.Value = droneRigidbody.YawVelocity();
                val_rollTarget.Value = inputsController.pitch * drone.controlSettings.maxRollAngle;
                val_rollActual.Value = drone.transform.rotation.eulerAngles.z;

                return true;
            }
            else
            {
                fold_Movement.value = false;
                fold_Movement.enabledSelf = false;
                return false;
            }
        }

        private bool UpdateNavigationPanel()
        {
            if (navigator && navigator.enabled)
            {
                fold_Navigation.enabledSelf = true;
                if (controls.Default.NavigationPanel.WasPressedThisFrame())
                    fold_Navigation.value = !fold_Navigation.value;
                
                val_position.Value = drone.transform.position;
                val_waypointsCount.Value = navigator.WaypointsCount;
                
                if (navigator.IsFinished || navigator.WaypointsCount == 0)
                {
                    val_waypointNumber.Value = 0;
                    val_waypointPos.Value = Vector3.zero;
                    val_waypointDist.Value = float.NaN;
                    val_waypointDirection.Value = Vector3.zero;
                }
                else
                {
                    val_waypointNumber.Value = navigator.CurrentWaypointIndex + 1;
                    val_waypointPos.Value  = navigator.CurrentWaypoint.position;
                    val_waypointDist.Value = (drone.transform.position - navigator.CurrentWaypoint.position).magnitude;
                    val_waypointDirection.Value = GetWaypointDirection();
                }

                return true;
            }
            else
            {
                fold_Navigation.value = false;
                fold_Navigation.enabledSelf = false;
                return false;
            }
        }
        
        private Vector3 GetWaypointDirection()
        {
            var dr = drone.transform.InverseTransformPoint(navigator.CurrentWaypoint.position);
            var x = dr.x > MIN_DIRECTION_CHANGE ? 1f : (dr.x < -MIN_DIRECTION_CHANGE ? -1f : 0f); 
            var y = dr.y > MIN_DIRECTION_CHANGE ? 1f : (dr.y < -MIN_DIRECTION_CHANGE ? -1f : 0f); 
            var z = dr.z > MIN_DIRECTION_CHANGE ? 1f : (dr.z < -MIN_DIRECTION_CHANGE ? -1f : 0f); 
            return new Vector3(x, y, z);
        }


        private void InitializeElements()
        {
            fold_Navigation = hudUIDocument.rootVisualElement.Q<Foldout>("fold_Navigation");
            fold_Movement = hudUIDocument.rootVisualElement.Q<Foldout>("fold_Movement");
            fold_Controls = hudUIDocument.rootVisualElement.Q<Foldout>("fold_Controls");
            
            lbl_Position = hudUIDocument.rootVisualElement.Q<Label>("lbl_Position");
            lbl_WaypointIndex = hudUIDocument.rootVisualElement.Q<Label>("lbl_WaypointIndex");
            lbl_WaypointPosition = hudUIDocument.rootVisualElement.Q<Label>("lbl_WaypointPosition");
            lbl_WaypointDistance = hudUIDocument.rootVisualElement.Q<Label>("lbl_WaypointDistance");
            lbl_WaypointDirection = hudUIDocument.rootVisualElement.Q<Label>("lbl_WaypointDirection");
            
            lbl_Velocity = hudUIDocument.rootVisualElement.Q<Label>("lbl_Velocity");
            lbl_YSpdTarget = hudUIDocument.rootVisualElement.Q<Label>("lbl_YSpdTarget");
            lbl_YSpdActual = hudUIDocument.rootVisualElement.Q<Label>("lbl_YSpdActual");
            lbl_PitchTarget = hudUIDocument.rootVisualElement.Q<Label>("lbl_PitchTarget");
            lbl_PitchActual = hudUIDocument.rootVisualElement.Q<Label>("lbl_PitchActual");
            lbl_YawTarget = hudUIDocument.rootVisualElement.Q<Label>("lbl_YawTarget");
            lbl_YawActual = hudUIDocument.rootVisualElement.Q<Label>("lbl_YawActual");
            lbl_RollTarget = hudUIDocument.rootVisualElement.Q<Label>("lbl_RollTarget");
            lbl_RollActual = hudUIDocument.rootVisualElement.Q<Label>("lbl_RollActual");
            
            tgl_Stabilization = hudUIDocument.rootVisualElement.Q<Toggle>("tgl_Stabilization");
            sld_Throttle = hudUIDocument.rootVisualElement.Q<Slider>("sld_Throttle");
            sld_Pitch = hudUIDocument.rootVisualElement.Q<Slider>("sld_Pitch");
            sld_Yaw = hudUIDocument.rootVisualElement.Q<Slider>("sld_Yaw");
            sld_Roll = hudUIDocument.rootVisualElement.Q<Slider>("sld_Roll");

            InitializeValuesChangedEventHandlers();
            elementsAreInitialized = true;
        }

        private void InitializeValuesChangedEventHandlers()
        {
            val_position.ValueChanged += PositionChangeHandler;
            val_waypointPos.ValueChanged += WaypointPositionChangeHandler;
            val_waypointDist.ValueChanged += WaypointDistanceChangeHandler;
            val_waypointNumber.ValueChanged += WaypointNumberChangeHandler;
            val_waypointsCount.ValueChanged += WaypointsCountChangeHandler;
            val_waypointDirection.ValueChanged += WaypointDirectionChangeHandler;
    
            val_velocity.ValueChanged += VelocityChangeHandler;
            val_ySpdActual.ValueChanged += YSpdActualChangeHandler;
            val_ySpdTarget.ValueChanged += YSpdTargetChangeHandler;
            val_pitchActual.ValueChanged += PitchTargetChangeHandler;
            val_pitchTarget.ValueChanged += PitchActualChangeHandler;
            val_yawActual.ValueChanged += YawActualChangeHandler;
            val_yawTarget.ValueChanged += YawTargetChangeHandler;
            val_rollActual.ValueChanged += RollTargetChangeHandler;
            val_rollTarget.ValueChanged += RollActualChangeHandler;

            val_throttle.ValueChanged += ThrottleChangeHandler;
            val_pitch.ValueChanged += PitchChangeHandler;
            val_yaw.ValueChanged += YawChangeHandler;
            val_roll.ValueChanged += RollChangeHandler;
            val_stabilization.ValueChanged += StabilizationChangeHandler; 
        }
        
        private void ClearValuesChangedEventHandlers()
        {
            val_position.ValueChanged -= PositionChangeHandler;
            val_waypointPos.ValueChanged -= WaypointPositionChangeHandler;
            val_waypointDist.ValueChanged -= WaypointDistanceChangeHandler;
            val_waypointNumber.ValueChanged -= WaypointNumberChangeHandler;
            val_waypointsCount.ValueChanged -= WaypointsCountChangeHandler;
            val_waypointDirection.ValueChanged -= WaypointDirectionChangeHandler;
    
            val_velocity.ValueChanged -= VelocityChangeHandler;
            val_ySpdActual.ValueChanged -= YSpdActualChangeHandler;
            val_ySpdTarget.ValueChanged -= YSpdTargetChangeHandler;
            val_pitchActual.ValueChanged -= PitchTargetChangeHandler;
            val_pitchTarget.ValueChanged -= PitchActualChangeHandler;
            val_yawActual.ValueChanged -= YawActualChangeHandler;
            val_yawTarget.ValueChanged -= YawTargetChangeHandler;
            val_rollActual.ValueChanged -= RollTargetChangeHandler;
            val_rollTarget.ValueChanged -= RollActualChangeHandler;

            val_throttle.ValueChanged -= ThrottleChangeHandler;
            val_pitch.ValueChanged -= PitchChangeHandler;
            val_yaw.ValueChanged -= YawChangeHandler;
            val_roll.ValueChanged -= RollChangeHandler;
            val_stabilization.ValueChanged -= StabilizationChangeHandler; 
        }
        
        private static void UpdateNumericLabel<T>(T value, Label targetLabel, string format = "F2")
            where T : struct, IFormattable
        {
            targetLabel.text = value.ToString(format, CultureInfo.CurrentCulture);
        }
        
        private static void UpdateSlider(float value, Slider targetSlider)
        {
            targetSlider.value = value;
        }
        
        private void PositionChangeHandler(Vector3 newValue, Vector3 oldValue) 
            => UpdateNumericLabel(newValue, lbl_Position, "F3");
        
        private void WaypointPositionChangeHandler(Vector3 newValue, Vector3 oldValue) 
            => UpdateNumericLabel(newValue, lbl_WaypointPosition, "F3");
        
        private void WaypointDistanceChangeHandler(float newValue, float oldValue) 
            => UpdateNumericLabel(newValue, lbl_WaypointDistance);
        
        private void WaypointNumberChangeHandler(int newValue, int oldValue) 
            => lbl_WaypointIndex.text = $"{newValue}/{val_waypointsCount.Value}";

        private void WaypointsCountChangeHandler(int newValue, int oldValue) 
            => lbl_WaypointIndex.text = $"{val_waypointNumber.Value}/{newValue}";
        
        private void WaypointDirectionChangeHandler(Vector3 newValue, Vector3 oldValue)
        {
            var sb = new List<char>(3);
            if (newValue.x != 0) sb.Add(newValue.x > 0 ? 'R' : 'L');
            if (newValue.y != 0) sb.Add(newValue.y > 0 ? 'U' : 'D');
            if (newValue.z != 0) sb.Add(newValue.z > 0 ? 'F' : 'B');
            lbl_WaypointDirection.text = string.Join("|", sb);
        }
        
        private void VelocityChangeHandler(Vector3 newValue, Vector3 oldValue) 
            => UpdateNumericLabel(newValue, lbl_Velocity, "F3");
        
        private void YSpdTargetChangeHandler(float newValue, float oldValue) 
            => UpdateNumericLabel(newValue, lbl_YSpdTarget);
        
        private void YSpdActualChangeHandler(float newValue, float oldValue) 
            => UpdateNumericLabel(newValue, lbl_YSpdActual);
        
        private void PitchTargetChangeHandler(float newValue, float oldValue) 
            => UpdateNumericLabel(newValue, lbl_PitchTarget);
        
        private void PitchActualChangeHandler(float newValue, float oldValue) 
            => UpdateNumericLabel(newValue, lbl_PitchActual);
        
        private void YawTargetChangeHandler(float newValue, float oldValue) 
            => UpdateNumericLabel(newValue, lbl_YawTarget);
        
        private void YawActualChangeHandler(float newValue, float oldValue) 
            => UpdateNumericLabel(newValue, lbl_YawActual);
        
        private void RollTargetChangeHandler(float newValue, float oldValue) 
            => UpdateNumericLabel(newValue, lbl_RollTarget);
        
        private void RollActualChangeHandler(float newValue, float oldValue) 
            => UpdateNumericLabel(newValue, lbl_RollActual);
        
        private void ThrottleChangeHandler(float newValue, float oldValue) 
            => UpdateSlider(newValue, sld_Throttle);
        
        private void PitchChangeHandler(float newValue, float oldValue) 
            => UpdateSlider(newValue, sld_Pitch);
        
        private void YawChangeHandler(float newValue, float oldValue) 
            => UpdateSlider(newValue, sld_Yaw);
        
        private void RollChangeHandler(float newValue, float oldValue) 
            => UpdateSlider(newValue, sld_Roll);
        
        private void StabilizationChangeHandler(bool newValue, bool oldValue)
            => tgl_Stabilization.value = newValue;
    }
}