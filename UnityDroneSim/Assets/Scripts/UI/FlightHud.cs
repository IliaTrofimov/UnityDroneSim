using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Drone;
using Exceptions;
using Inputs;
using InspectorTools;
using Navigation;
using RL;
using RL.Rewards;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;


namespace UI
{
    [DisallowMultipleComponent]
    public class FlightHud : MonoBehaviour
    {
        private DroneControls         _controls;
        private DroneInputsController _inputsController;
        private Rigidbody             _droneRigidbody;
        private DroneStateManager     _droneStateManager;
        private ReadOnlyCollection<float> _agentVectorObservations;
        
        #region UI Elements
        private UIDocument _hudUIDocument;
        private bool _elementsAreInitialized;

        private Label  _lblDroneEnabled;
        private Button _btnSwitchView;

        private Foldout _foldNavigation;
        private Foldout _foldMovement;
        private Foldout _foldControls;
        private Foldout _foldRewards;
        private Foldout _foldObservations;

        private Label _lblPosition;
        private Label _lblWaypointIndex;
        private Label _lblWaypointPosition;
        private Label _lblWaypointDistance;

        private Label _lblVelocity;
        private Label _lblYSpdTarget;
        private Label _lblYSpdActual;
        private Label _lblPitchTarget;
        private Label _lblPitchActual;
        private Label _lblYawTarget;
        private Label _lblYawActual;
        private Label _lblRollTarget;
        private Label _lblRollActual;

        private VisualElement _panelRewards;
        private Label _lblRewardCumulative;
        private Label _lblRewardLast;
        
        private VisualElement _panelObservations;
        private VisualElement _panelObservationsNames;

        private Toggle _tglStabilization;
        private Label  _lblStatus;
        private Slider _sldThrottle;
        private Slider _sldPitch;
        private Slider _sldYaw;
        private Slider _sldRoll;

        #endregion

        #region Tracked properties
        
        private const float MIN_DIRECTION_CHANGE     = 0.1F;
        private const float FLOAT_CHANGE_EPS_PRECISE = 0.0001f;
        private const float FLOAT_CHANGE_EPS         = 0.001f;

        private IEnumerator<RewardProvider> _rewardsEnumerator;
        
        private readonly TrackedVector3            _valPosition          = new(FLOAT_CHANGE_EPS_PRECISE);
        private readonly TrackedVector3            _valWaypointPos       = new(FLOAT_CHANGE_EPS_PRECISE);
        private readonly TrackedObject<int>        _valWaypointNumber    = new();
        private readonly TrackedObject<int>        _valWaypointsCount    = new();
        private readonly TrackedFloat              _valWaypointDist      = new(FLOAT_CHANGE_EPS);
        private readonly TrackedObject<bool>       _valDroneFailure      = new(false);
        private readonly TrackedObject<bool>       _valDroneEnabled      = new(false);

        private readonly TrackedVector3 _valVelocity    = new(FLOAT_CHANGE_EPS_PRECISE);
        private readonly TrackedFloat   _valYSpdTarget  = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   _valYSpdActual  = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   _valPitchTarget = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   _valPitchActual = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   _valYawTarget   = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   _valYawActual   = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   _valRollTarget  = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat   _valRollActual  = new(FLOAT_CHANGE_EPS);

        private readonly TrackedFloat        _valThrottle      = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat        _valPitch         = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat        _valYaw           = new(FLOAT_CHANGE_EPS);
        private readonly TrackedFloat        _valRoll          = new(FLOAT_CHANGE_EPS);
        private readonly TrackedObject<bool> _valStabilization = new();
        
        private readonly TrackedFloat   _valRewardCumulative  = new(1e-4f);
        private readonly TrackedFloat   _valRewardLast  = new(1e-5f);

        
        #endregion

        [Header("UI")] 
        [Tooltip("Start with all elements expanded.")]
        public bool startExpanded  = true;
        
        [Tooltip("Enable/disable movement panel.")]
        public bool enableMovementPanel = true;
        
        [Tooltip("Enable/disable controls panel.")]
        public bool enableControlsPanel = true;
        
        [Tooltip("Enable/disable navigation panel.")]
        public bool enableNavigationPanel = true;
        
        [Tooltip("Enable/disable AI agent rewards panel.")]
        public bool enableRewardsPanel = true;

        [Tooltip("Enable/disable AI agent observations panel.")]
        public bool enableObservationsPanel = true;
        
        [Header("Cameras")]
        [Min(0)] 
        [Tooltip("Initial camera.")]
        public int selectedCamera;
        
        [Tooltip("Helper that provides can switch between provided cameras.")]
        public CameraSwitcher cameraSwitcher;

        [Header("Target")] 
        [Tooltip("Target drone that will provide its data.")]
        public DroneComputerBase drone;
        
        [Tooltip("Target drone AI agent that will provide its rewards.")]
        public DroneAgent droneAgent;
        
        [Tooltip("Target waypoint navigator that will provide info about current and next waypoints.")]
        public WaypointNavigator navigator;
        

        private void OnEnable()
        {
            _hudUIDocument = GetComponent<UIDocument>();
            _controls = new DroneControls();
            _controls.Enable();

            ExceptionHelper.ThrowIfComponentIsMissing(this, _hudUIDocument, nameof(_hudUIDocument));
            ExceptionHelper.ThrowIfComponentIsMissing(this, drone, nameof(drone));

            _inputsController = drone.GetComponent<DroneInputsController>();
            _droneRigidbody = drone.Rigidbody;
            _droneStateManager = drone.gameObject.GetComponent<DroneStateManager>();

            cameraSwitcher.TrySetCamera(selectedCamera);
            InitializeElements();
        }

        private void OnDisable()
        {
            ClearValuesChangedEventHandlers();
            _controls.Disable();
        }

        private void Start()
        {
            if (!_elementsAreInitialized) return;

            _foldControls.value = startExpanded;
            _foldMovement.value = startExpanded;
            _foldNavigation.value = startExpanded;
            _foldObservations.value = startExpanded;
            _foldRewards.value = startExpanded;
            _valDroneEnabled.Value = drone?.enabled ?? false;
        }

        private void OnValidate()
        {
            cameraSwitcher.TrySetCamera(selectedCamera);

            if (_elementsAreInitialized)
            {
                _foldControls.visible = enableControlsPanel;
                _foldMovement.visible = enableMovementPanel;
                _foldNavigation.visible = enableNavigationPanel;
                _foldRewards.visible = enableRewardsPanel;
                _foldObservations.visible = enableObservationsPanel;
            }
        }

        private void Update()
        {
            selectedCamera = cameraSwitcher.CurrentCameraIndex;
            
            if (!enabled) return;

            if (_controls.Default.EnableDrone.WasPressedThisFrame() && drone)
            {
                drone.enabled = !drone.enabled;
                _valDroneEnabled.Value = drone.enabled;
            }

            if (_controls.Default.CameraMode.WasPressedThisFrame())
                SwitchView();

            if (!_elementsAreInitialized)
                InitializeElements();

            if (UpdateControlsPanel())
            {
                UpdateMovementPanel();
                UpdateNavigationPanel();
                UpdateRewardsPanel();
                UpdateObservationsPanel();
            }
        }

        private bool UpdateControlsPanel()
        {
            if (!drone)
            {
                _foldNavigation.value = false;
                _foldNavigation.enabledSelf = false;
                _foldControls.value = false;
                _foldControls.enabledSelf = false;
                _foldMovement.value = false;
                _foldMovement.enabledSelf = false;
                _foldNavigation.value = false;
                _foldNavigation.enabledSelf = false;
                return false;
            }
            
            _foldControls.enabledSelf = true;
            if (_controls.Default.ControlsPanel.WasPressedThisFrame())
                _foldControls.value = !_foldControls.value;

            if (_foldControls.value)
                return true;
            
            _valThrottle.Value = _inputsController.throttle;
            _valPitch.Value = _inputsController.pitch;
            _valYaw.Value = _inputsController.yaw;
            _valRoll.Value = _inputsController.roll;
            _valStabilization.Value = _inputsController.IsFullStabilization();

            if (_droneStateManager)
            {
                _valDroneFailure.Value = _droneStateManager.AnyMotorsDestroyed;
                if (_droneStateManager.AnyMotorsDestroyed && _controls.Default.Repair.WasPressedThisFrame())
                {
                    _droneStateManager.RepairAllMotors();
                    drone.ResetStabilizers();
                }
            }

            return true;
        }

        private bool UpdateMovementPanel()
        {
            if (_droneRigidbody)
            {
                _foldMovement.enabledSelf = true;
                if (_controls.Default.MovementPanel.WasPressedThisFrame())
                    _foldMovement.value = !_foldMovement.value;

                if (!_foldMovement.value) 
                    return true;
                
                _valVelocity.Value = _droneRigidbody.linearVelocity;
                _valYSpdTarget.Value = _inputsController.throttle * drone.MaxLiftSpeed;
                _valYSpdActual.Value = _droneRigidbody.linearVelocity.y;
                _valPitchTarget.Value = _inputsController.pitch * drone.MaxPitchAngle;
                _valPitchActual.Value = drone.transform.rotation.eulerAngles.x;
                _valYawTarget.Value = _inputsController.yaw * drone.MaxYawSpeed;
                _valYawActual.Value = _droneRigidbody.YawVelocity();
                _valRollTarget.Value = _inputsController.pitch * drone.MaxRollAngle;
                _valRollActual.Value = drone.transform.rotation.eulerAngles.z;

                return true;
            }

            _foldMovement.value = false;
            _foldMovement.enabledSelf = false;
            return false;
        }

        private bool UpdateNavigationPanel()
        {
            if (navigator && navigator.enabled)
            {
                _foldNavigation.enabledSelf = true;
                if (_controls.Default.NavigationPanel.WasPressedThisFrame())
                    _foldNavigation.value = !_foldNavigation.value;

                if (!_foldNavigation.value) 
                    return true;

                _valPosition.Value = drone.transform.position;
                _valWaypointsCount.Value = navigator.WaypointsCount;

                if (navigator.IsFinished || !navigator.CurrentWaypoint.HasValue)
                {
                    _valWaypointNumber.Value = navigator.WaypointsCount;
                    _valWaypointPos.Value = Vector3.zero;
                    _valWaypointDist.Value = float.NaN;
                }
                else
                {
                    _valWaypointNumber.Value = navigator.CurrentWaypointIndex + 1;
                    _valWaypointPos.Value = navigator.CurrentWaypoint.Value.position;
                    _valWaypointDist.Value =
                        (drone.transform.position - navigator.CurrentWaypoint.Value.position).magnitude;
                }

                return true;
            }

            _foldNavigation.value = false;
            _foldNavigation.enabledSelf = false;
            return false;
        }
        
        private bool UpdateRewardsPanel()
        {
            if (droneAgent)
            {
                _foldRewards.enabledSelf = true;
                if (_controls.Default.RewardsPanel.WasPressedThisFrame())
                    _foldRewards.value = !_foldRewards.value;

                if (!_foldRewards.value)
                    return true;
                
                _valRewardLast.Value = droneAgent.RewardProvider.LastReward;
                _valRewardCumulative.Value = droneAgent.RewardProvider.CumulativeReward;
                
                if (_panelRewards.childCount != droneAgent.RewardProvider.RewardsCount)
                {
                    _panelRewards.Clear();
                    foreach (var reward in droneAgent.RewardProvider.GetRewards())
                    {
                        var root = new VisualElement();
                        root.AddToClassList("reward-container");

                        var lblCumReward = new Label(reward.CumulativeReward.ToString("F2"));
                        lblCumReward.style.color = GetRewardColor(reward.CumulativeReward);

                        var lblLastReward = new Label($"({reward.LastReward:F3})");
                        lblLastReward.style.color = GetRewardColor(reward.LastReward);
                        
                        root.Add(new Label(reward.RewardName));
                        root.Add(lblCumReward);
                        root.Add(lblLastReward);
                        _panelRewards.Add(root);
                    }
                }
                else
                {
                    var index = 0;
                    foreach (var reward in droneAgent.RewardProvider.GetRewards())
                    {
                        var root = _panelRewards[index];
                        if (root[1] is Label lblCumReward)
                        {
                            lblCumReward.text = reward.CumulativeReward.ToString("F2");
                            lblCumReward.style.color = GetRewardColor(reward.CumulativeReward);
                        }
                        if (root[2] is Label lblLastReward)
                        {
                            lblLastReward.text = $"({reward.LastReward:F3})";
                            lblLastReward.style.color = GetRewardColor(reward.LastReward);
                        }
                        index++;
                    }
                }

                return true;
            }

            _foldRewards.value = false;
            _foldRewards.enabledSelf = false;
            return false;
        }

        private static Color GetRewardColor(float value)
        {
            return value switch
            {
                > 1e-5f  => Color.green,
                < -1e-5f => Color.red,
                _    => Color.gray,
            };
        }
        
        private bool UpdateObservationsPanel()
        {
            if (droneAgent)
            {
                _foldObservations.enabledSelf = true;
                if (_controls.Default.ObservationsPanel.WasPressedThisFrame())
                    _foldObservations.value = !_foldObservations.value;

                if (!_foldObservations.value)
                    return true;

                _agentVectorObservations = droneAgent.GetObservations();
                if (_agentVectorObservations.Count == 0) 
                    return true;
                
                var index = 0;
                if (_panelObservations.childCount != _agentVectorObservations.Count)
                {
                    _panelObservations.Clear();
                    _panelObservationsNames.Clear();
                    
                    foreach (var observation in _agentVectorObservations)
                    {
                        var valueLabel = new Label(observation.ToString("F3"));
                        valueLabel.AddToClassList("observation-value");
                        _panelObservations.Add(valueLabel);
                        
                        var nameLabel = new Label(DroneAgent.ScalarObservationNames[index]);
                        nameLabel.AddToClassList("observation-name");
                        _panelObservationsNames.Add(nameLabel);
                        
                        index++;
                    }
                }
                else
                {
                    foreach (var observation in _agentVectorObservations)
                    {
                        if (_panelObservations[index] is Label valueLabel)
                            valueLabel.text = observation.ToString("F3");
                        index++;
                    }
                }

                return true;
            }

            _foldObservations.value = false;
            _foldObservations.enabledSelf = false;
            return false;
        }

        
        private void InitializeElements()
        {
            _foldNavigation  = _hudUIDocument.rootVisualElement.Q<Foldout>("fold_Navigation");
            _foldMovement    = _hudUIDocument.rootVisualElement.Q<Foldout>("fold_Movement");
            _foldControls    = _hudUIDocument.rootVisualElement.Q<Foldout>("fold_Controls");
            _foldRewards     = _hudUIDocument.rootVisualElement.Q<Foldout>("fold_Rewards");
            _foldObservations = _hudUIDocument.rootVisualElement.Q<Foldout>("fold_Observations");

            _lblDroneEnabled = _hudUIDocument.rootVisualElement.Q<Label>("lbl_DroneEnabled");
            _btnSwitchView   = _hudUIDocument.rootVisualElement.Q<Button>("btn_SwitchView");

            _lblPosition          = _hudUIDocument.rootVisualElement.Q<Label>("lbl_Position");
            _lblWaypointIndex     = _hudUIDocument.rootVisualElement.Q<Label>("lbl_WaypointIndex");
            _lblWaypointPosition  = _hudUIDocument.rootVisualElement.Q<Label>("lbl_WaypointPosition");
            _lblWaypointDistance  = _hudUIDocument.rootVisualElement.Q<Label>("lbl_WaypointDistance");

            _panelRewards = _hudUIDocument.rootVisualElement.Q<VisualElement>("panel_RewardsList");
            _lblRewardCumulative = _hudUIDocument.rootVisualElement.Q<Label>("lbl_RewardCumulative");
            _lblRewardLast = _hudUIDocument.rootVisualElement.Q<Label>("lbl_RewardLast");

            _lblVelocity    = _hudUIDocument.rootVisualElement.Q<Label>("lbl_Velocity");
            _lblYSpdTarget  = _hudUIDocument.rootVisualElement.Q<Label>("lbl_YSpdTarget");
            _lblYSpdActual  = _hudUIDocument.rootVisualElement.Q<Label>("lbl_YSpdActual");
            _lblPitchTarget = _hudUIDocument.rootVisualElement.Q<Label>("lbl_PitchTarget");
            _lblPitchActual = _hudUIDocument.rootVisualElement.Q<Label>("lbl_PitchActual");
            _lblYawTarget   = _hudUIDocument.rootVisualElement.Q<Label>("lbl_YawTarget");
            _lblYawActual   = _hudUIDocument.rootVisualElement.Q<Label>("lbl_YawActual");
            _lblRollTarget  = _hudUIDocument.rootVisualElement.Q<Label>("lbl_RollTarget");
            _lblRollActual  = _hudUIDocument.rootVisualElement.Q<Label>("lbl_RollActual");

            _tglStabilization = _hudUIDocument.rootVisualElement.Q<Toggle>("tgl_Stabilization");
            _lblStatus        = _hudUIDocument.rootVisualElement.Q<Label>("lbl_Status");
            _sldThrottle      = _hudUIDocument.rootVisualElement.Q<Slider>("sld_Throttle");
            _sldPitch         = _hudUIDocument.rootVisualElement.Q<Slider>("sld_Pitch");
            _sldYaw           = _hudUIDocument.rootVisualElement.Q<Slider>("sld_Yaw");
            _sldRoll          = _hudUIDocument.rootVisualElement.Q<Slider>("sld_Roll");

            _panelObservations = _hudUIDocument.rootVisualElement.Q<VisualElement>("panel_Observations");
            _panelObservationsNames = _hudUIDocument.rootVisualElement.Q<VisualElement>("panel_ObservationsNames");
            
            InitializeValuesChangedEventHandlers();
            SetTemplatedLabels();

            _elementsAreInitialized = true;
        }

        private void SetTemplatedLabels()
        {
            try
            {
                _foldControls.text = string.Format(_foldControls.text,
                    _controls.Default.ControlsPanel.controls[0].displayName
                );

                _foldNavigation.text = string.Format(_foldNavigation.text,
                    _controls.Default.NavigationPanel.controls[0].displayName
                );

                _foldMovement.text = string.Format(_foldMovement.text,
                    _controls.Default.MovementPanel.controls[0].displayName
                );

                _foldRewards.text = string.Format(_foldRewards.text,
                    _controls.Default.RewardsPanel.controls[0].displayName
                );
                
                _foldObservations.text = string.Format(_foldObservations.text,
                    _controls.Default.ObservationsPanel.controls[0].displayName
                );
                
                _tglStabilization.text = string.Format(_tglStabilization.text,
                    _controls.Default.FullStabilization.controls[0].displayName
                );

                _lblDroneEnabled.text = string.Format(_lblDroneEnabled.text,
                    _controls.Default.EnableDrone.controls[0].displayName
                );
                
                _lblStatus.text = string.Format(_lblStatus.text, _controls.Default.Repair.controls[0].displayName);
                
                _btnSwitchView.text = string.Format(_btnSwitchView.text, 
                    _controls.Default.CameraMode.controls[0].displayName
                );
            }
            catch (Exception ex)
            {
                Debug.LogWarningFormat("Cannot update templated label: {0}", ex);
            }
        }

        private void InitializeValuesChangedEventHandlers()
        {
            _valPosition.ValueChanged += PositionChangeHandler;
            _valWaypointPos.ValueChanged += WaypointPositionChangeHandler;
            _valWaypointDist.ValueChanged += WaypointDistanceChangeHandler;
            _valWaypointNumber.ValueChanged += WaypointNumberChangeHandler;
            _valWaypointsCount.ValueChanged += WaypointsCountChangeHandler;

            _valVelocity.ValueChanged += VelocityChangeHandler;
            _valYSpdActual.ValueChanged += YSpdActualChangeHandler;
            _valYSpdTarget.ValueChanged += YSpdTargetChangeHandler;
            _valPitchActual.ValueChanged += PitchActualChangeHandler;
            _valPitchTarget.ValueChanged += PitchTargetChangeHandler;
            _valYawActual.ValueChanged += YawActualChangeHandler;
            _valYawTarget.ValueChanged += YawTargetChangeHandler;
            _valRollActual.ValueChanged += RollActualChangeHandler;
            _valRollTarget.ValueChanged += RollTargetChangeHandler;

            _valThrottle.ValueChanged += ThrottleChangeHandler;
            _valPitch.ValueChanged += PitchChangeHandler;
            _valYaw.ValueChanged += YawChangeHandler;
            _valRoll.ValueChanged += RollChangeHandler;
            _valStabilization.ValueChanged += StabilizationChangeHandler;
            _valDroneFailure.ValueChanged += DroneFailureChangeHandler;
            _valDroneEnabled.ValueChanged += DroneEnabledChangeHandler;
            
            _valRewardLast.ValueChanged += RewardLastChangeHandler;
            _valRewardCumulative.ValueChanged += RewardCumulativeChangeHandler;
            
            _btnSwitchView.RegisterCallback<PointerDownEvent>(ViewChangedHandler);
            _tglStabilization.RegisterValueChangedCallback(ToggleStabilizationClickHandler);
        }

        private void ClearValuesChangedEventHandlers()
        {
            _valPosition.ValueChanged -= PositionChangeHandler;
            _valWaypointPos.ValueChanged -= WaypointPositionChangeHandler;
            _valWaypointDist.ValueChanged -= WaypointDistanceChangeHandler;
            _valWaypointNumber.ValueChanged -= WaypointNumberChangeHandler;
            _valWaypointsCount.ValueChanged -= WaypointsCountChangeHandler;

            _valVelocity.ValueChanged -= VelocityChangeHandler;
            _valYSpdActual.ValueChanged -= YSpdActualChangeHandler;
            _valYSpdTarget.ValueChanged -= YSpdTargetChangeHandler;
            _valPitchActual.ValueChanged -= PitchTargetChangeHandler;
            _valPitchTarget.ValueChanged -= PitchActualChangeHandler;
            _valYawActual.ValueChanged -= YawActualChangeHandler;
            _valYawTarget.ValueChanged -= YawTargetChangeHandler;
            _valRollActual.ValueChanged -= RollTargetChangeHandler;
            _valRollTarget.ValueChanged -= RollActualChangeHandler;

            _valThrottle.ValueChanged -= ThrottleChangeHandler;
            _valPitch.ValueChanged -= PitchChangeHandler;
            _valYaw.ValueChanged -= YawChangeHandler;
            _valRoll.ValueChanged -= RollChangeHandler;
            _valStabilization.ValueChanged -= StabilizationChangeHandler;
            _valDroneFailure.ValueChanged -= DroneFailureChangeHandler;
            _valDroneEnabled.ValueChanged -= DroneEnabledChangeHandler;
            
            _valRewardLast.ValueChanged -= RewardLastChangeHandler;
            _valRewardCumulative.ValueChanged -= RewardCumulativeChangeHandler;
            
            _btnSwitchView.UnregisterCallback<PointerDownEvent>(ViewChangedHandler);
            _tglStabilization.UnregisterValueChangedCallback(ToggleStabilizationClickHandler);
        }

        private static void UpdateNumericLabel<T>(T value, Label targetLabel, string format = "F2")
            where T : struct, IFormattable
        {
            targetLabel.text = value.ToString(format, CultureInfo.CurrentCulture);
        }

        private static void UpdateSlider(float value, Slider targetSlider) { targetSlider.value = value; }

        private void PositionChangeHandler(Vector3 newValue, Vector3 oldValue) =>
            UpdateNumericLabel(newValue, _lblPosition, "F3");

        private void WaypointPositionChangeHandler(Vector3 newValue, Vector3 oldValue) =>
            UpdateNumericLabel(newValue, _lblWaypointPosition, "F3");

        private void WaypointDistanceChangeHandler(float newValue, float oldValue) =>
            UpdateNumericLabel(newValue, _lblWaypointDistance);

        private void WaypointNumberChangeHandler(int newValue, int oldValue) =>
            SetWaypointIndex(newValue, _valWaypointsCount.Value);

        private void WaypointsCountChangeHandler(int newValue, int oldValue) =>
            SetWaypointIndex(_valWaypointNumber.Value, newValue);

        private void SetWaypointIndex(int current, int max)
        {
            if (max <= 0) _lblWaypointIndex.text = "NONE";
            else if (max == current) _lblWaypointIndex.text = $"{current}/{max} FINISHED";
            else _lblWaypointIndex.text = $"{current}/{max}";
        }

        private void VelocityChangeHandler(Vector3 newValue, Vector3 oldValue) =>
            _lblVelocity.text = $"{newValue.magnitude:F3} m/s {newValue:F3}";

        private void YSpdTargetChangeHandler(float newValue, float oldValue) =>
            UpdateNumericLabel(newValue, _lblYSpdTarget);

        private void YSpdActualChangeHandler(float newValue, float oldValue) =>
            UpdateNumericLabel(newValue, _lblYSpdActual);

        private void PitchTargetChangeHandler(float newValue, float oldValue) =>
            UpdateNumericLabel(newValue, _lblPitchTarget);

        private void PitchActualChangeHandler(float newValue, float oldValue) =>
            UpdateNumericLabel(newValue, _lblPitchActual);

        private void YawTargetChangeHandler(float newValue, float oldValue) =>
            UpdateNumericLabel(newValue, _lblYawTarget);

        private void YawActualChangeHandler(float newValue, float oldValue) =>
            UpdateNumericLabel(newValue, _lblYawActual);

        private void RollTargetChangeHandler(float newValue, float oldValue) =>
            UpdateNumericLabel(newValue, _lblRollTarget);

        private void RollActualChangeHandler(float newValue, float oldValue) =>
            UpdateNumericLabel(newValue, _lblRollActual);

        private void RewardLastChangeHandler(float newValue, float oldValue)
        {
            _lblRewardLast.text = $"({newValue:F3})";
            _lblRewardLast.style.color = GetRewardColor(newValue);
        }
        
        private void RewardCumulativeChangeHandler(float newValue, float oldValue)
        {
            _lblRewardCumulative.text = $"{newValue:F2}";
            _lblRewardCumulative.style.color = GetRewardColor(newValue);
        }

        private void ThrottleChangeHandler(float newValue, float oldValue) => UpdateSlider(newValue, _sldThrottle);

        private void PitchChangeHandler(float newValue, float oldValue) => UpdateSlider(newValue, _sldPitch);

        private void YawChangeHandler(float newValue, float oldValue) => UpdateSlider(newValue, _sldYaw);

        private void RollChangeHandler(float newValue, float oldValue) => UpdateSlider(newValue, _sldRoll);

        private void StabilizationChangeHandler(bool newValue, bool oldValue) => _tglStabilization.value = newValue;

        private void DroneFailureChangeHandler(bool newValue, bool previousValue)
        {
            _lblStatus.style.display = newValue ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void DroneEnabledChangeHandler(bool newValue, bool previousValue)
        {
            _lblDroneEnabled.style.display = newValue ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void ViewChangedHandler(PointerDownEvent evt)
        {
            SwitchView();
        }

        private void ToggleStabilizationClickHandler(ChangeEvent<bool> evt)
        {
            StabilizationChangeHandler(evt.newValue, evt.previousValue);
        }
        
        private void SwitchView()
        {
            cameraSwitcher.SwitchNext();
        }
    }
}