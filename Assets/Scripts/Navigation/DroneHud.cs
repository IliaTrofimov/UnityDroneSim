using System;
using Drone;
using Drone.Stability;
using UnityEngine;
using Utils;


namespace Navigation
{
    [ExecuteInEditMode]
    public class DroneHud : MonoBehaviour
    {
        private float collapsedHeight = 34;
        private float screenOffset = 2;
        
        private float movementPanel_WRatio = 0.48f;
        private float movementPanel_HRatio = 0.25f;
        private float movementPanel_MaxW = 450f;
        private float movementPanel_MaxH = 300f;
        
        private float controlsPanel_WRatio = 0.48f;
        private float controlsPanel_HRatio = 0.25f;
        private float controlsPanel_MaxW = 450f;
        private float controlsPanel_MaxH = 300f;
        
        private float navigationPanel_WRatio = 0.5f;
        private float navigationPanel_HRatio = 0.15f;
        private float navigationPanel_MaxW = 600f;
        private float navigationPanel_MaxH = 200f;

        private int lastScreenWidth = 0;
        private int lastScreenHeight = 0;
        
        private bool isMovementVisible, isNavigationVisible, isControlsVisible;
        private bool isFirstPerson;
        private bool stylesInitialized;
        
        private GUIStyle textStyle, boldTextStyle, italicTextStyle, sliderStyle, checkBoxStyle;
        
        
        [Header("Target objects")]
        public QuadcopterComputer drone;
        public WaypointNavigator navigator;
        private DroneInputsController inputsController;

        [Header("Settings")]
        public bool initialExpanded = true;
        public bool enableControlPanel = true;
        public bool enableNavigationPanel = true;
        public bool enableMovementPanel = true;

        [Header("Cameras")]
        public Camera fpvCamera;
        public Camera tpvCamera;
        
        
        private void Awake()
        {
            isMovementVisible = isNavigationVisible = isControlsVisible = initialExpanded;
            if (drone) inputsController = drone.GetComponent<DroneInputsController>();
            
            isFirstPerson = fpvCamera?.enabled ?? false;
        }

        private void OnValidate()
        {
            inputsController = drone ? drone.GetComponent<DroneInputsController>() : null;
        }
        
        private void InitStyles(out int width, out int height)
        {
            width = Screen.width;
            height = Screen.height;
            
            if (stylesInitialized || width == lastScreenWidth || height == lastScreenHeight) return;
            
            lastScreenWidth = width;
            lastScreenHeight = height;
            
            textStyle = new GUIStyle("label")
            {
                alignment = TextAnchor.LowerLeft,
                fontSize = GetFontSize(width, height)
            };
            boldTextStyle = new GUIStyle(textStyle)
            {
                fontStyle = FontStyle.Bold
            };
            italicTextStyle = new GUIStyle(textStyle)
            {
                fontStyle = FontStyle.Italic
            };
            sliderStyle = new GUIStyle(GUI.skin.verticalSlider)
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
            };
            checkBoxStyle = new GUIStyle(GUI.skin.toggle)
            {
                fontSize = textStyle.fontSize
            };

            if (height > 1080)
            {
                movementPanel_MaxW = 600;
                movementPanel_MaxH = 400;
                navigationPanel_MaxW = 800;
                navigationPanel_MaxH = 350;
                controlsPanel_MaxW = 600;
                controlsPanel_MaxH = 400;
            }
            else
            {
                movementPanel_MaxW = 450;
                movementPanel_MaxH = 300;
                navigationPanel_MaxW = 600;
                navigationPanel_MaxH = 200;
                controlsPanel_MaxW = 450;
                controlsPanel_MaxH = 300;
            }
        }

        private static int GetFontSize(int screenWidth, int screenHeight)
        {
            if (screenHeight <= 500) return 12;
            if (screenHeight <= 768) return 18;
            if (screenHeight <= 1080) return 22;
            return 28;
        }

        private void OnGUI()
        {
            if (!enabled) return;
            
            InitStyles(out var width, out var height);

            if (enableControlPanel) ControlsBox(width, height);
            if (enableMovementPanel) MovementBox(width, height);
            if (enableNavigationPanel) NavigationBox(width, height);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                isMovementVisible = !isMovementVisible;
            }
            if (Input.GetKeyDown(KeyCode.N))
            {
                isNavigationVisible = !isNavigationVisible;
            } 
            if (Input.GetKeyDown(KeyCode.C))
            {
                isControlsVisible = !isControlsVisible;
            }
            if (Input.GetKeyDown(KeyCode.V) && fpvCamera && tpvCamera)
            {
                isFirstPerson = !isFirstPerson;
                if (isFirstPerson)
                {
                    tpvCamera.targetDisplay = 3;
                    fpvCamera.targetDisplay = 1;
                    tpvCamera.targetDisplay = 2;
                    fpvCamera.enabled = true;
                    tpvCamera.enabled = false;
                }
                else
                {
                    fpvCamera.targetDisplay = 3;
                    tpvCamera.targetDisplay = 1;
                    fpvCamera.targetDisplay = 2;
                    fpvCamera.enabled = false;
                    tpvCamera.enabled = true;
                }
            }
        }

        private void ControlsBox(int w, int h)
        {
            var boxW = Math.Min(w * controlsPanel_WRatio, controlsPanel_MaxW);
            var boxH = isControlsVisible 
                ? Math.Min(h * controlsPanel_HRatio, controlsPanel_MaxH)
                : collapsedHeight;
            
            GUILayout.BeginArea(new Rect(w - boxW - screenOffset, h - boxH - screenOffset, boxW, boxH), GUI.skin.box);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("CONTROLS", boldTextStyle);
                if (GUILayout.Button(isControlsVisible ? "x" : "+"))
                    isControlsVisible = !isControlsVisible;
            }

            if (!isControlsVisible)
            {
                GUILayout.EndArea();
                return;
            }
            
            if (!inputsController)
            {
                GUILayout.Label($"{nameof(QuadcopterComputer)} with {nameof(DroneInputsController)} are not attached", italicTextStyle);
                GUILayout.EndArea();
                return;
            }
            
            using (new GUILayout.HorizontalScope())
            {
                var hasStab = inputsController.stabilizerMode.HasFlag(DroneStabilizerMode.StabAltitude);
                if (GUILayout.Toggle(hasStab, "Stabilization (press 'space')", checkBoxStyle))
                {
                    inputsController.stabilizerMode = DroneStabilizerMode.StabAltitude | 
                                                      DroneStabilizerMode.StabPitchRoll |
                                                      DroneStabilizerMode.StabYaw;
                }
                else
                {
                    inputsController.stabilizerMode = DroneStabilizerMode.None;
                }
            }
            using (new GUILayout.HorizontalScope())
            {
                var sliderOpt = GUILayout.ExpandWidth(true);
                using (new GUILayout.VerticalScope(sliderOpt))
                {
                    GUILayout.VerticalSlider(inputsController.throttle, 1, -1, sliderStyle, GUI.skin.verticalSliderThumb, sliderOpt);
                    GUILayout.Label($"Throt. {inputsController.throttle:F1}", textStyle);
                }
                using (new GUILayout.VerticalScope(sliderOpt))
                {
                    GUILayout.VerticalSlider(inputsController.pitch, 1, -1, sliderStyle, GUI.skin.verticalSliderThumb);
                    GUILayout.Label($"Pitch {inputsController.pitch:F1}", textStyle);
                } 
                using (new GUILayout.VerticalScope(sliderOpt))
                {
                    GUILayout.VerticalSlider(inputsController.yaw, 1, -1, sliderStyle, GUI.skin.verticalSliderThumb);
                    GUILayout.Label($"Yaw {inputsController.yaw:F1}", textStyle);
                } 
                using (new GUILayout.VerticalScope(sliderOpt))
                {
                    GUILayout.VerticalSlider(inputsController.roll, 1, -1, sliderStyle, GUI.skin.verticalSliderThumb);
                    GUILayout.Label($"Roll {inputsController.roll:F1}", textStyle);
                }
            }
            GUILayout.EndArea();
        }
        
        private void MovementBox(int w, int h)
        {
            var boxW = Math.Min(w * movementPanel_WRatio, movementPanel_MaxW);
            var boxH = isMovementVisible 
                ? Math.Min(h * movementPanel_HRatio, movementPanel_MaxH)
                : collapsedHeight;
            
            GUILayout.BeginArea(new Rect(screenOffset, h - boxH - screenOffset, boxW, boxH), GUI.skin.box);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("MOVEMENT", boldTextStyle);
                if (GUILayout.Button(isMovementVisible ? "x" : "+"))
                    isMovementVisible = !isMovementVisible;
            }
            
            if (!isMovementVisible)
            {
                GUILayout.EndArea();
                return;
            }

            if (!drone || !drone.rigidBody)
            {
                GUILayout.Label($"{nameof(QuadcopterComputer)} is not attached", italicTextStyle);
                GUILayout.EndArea();
                return;
            }
            
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Velocity", textStyle);
                GUILayout.Label($"{drone.rigidBody.linearVelocity:F3}", textStyle);
            }
            
            using (new GUILayout.HorizontalScope())
            {
               using (new GUILayout.VerticalScope())
               {
                   GUILayout.Label("Target / actual Y spd.", textStyle);
                   GUILayout.Label("Target / actual pitch ang.", textStyle);
                   GUILayout.Label("Target / actual yaw spd.", textStyle);
                   GUILayout.Label("Target / actual roll ang.", textStyle);
               }
               using (new GUILayout.VerticalScope())
               {
                   GUILayout.Label($"{drone.controlSettings.maxLiftForce * inputsController.throttle:F2}", textStyle);
                   GUILayout.Label($"{drone.controlSettings.maxPitchAngle * inputsController.pitch:F2}", textStyle);
                   GUILayout.Label($"{drone.controlSettings.maxYawSpeed * inputsController.yaw:F2}", textStyle);
                   GUILayout.Label($"{drone.controlSettings.maxRollAngle * inputsController.roll:F2}", textStyle);
               }
               using (new GUILayout.VerticalScope())
               {
                   var rot = transform.WrapEulerRotation180();
                   GUILayout.Label($"{drone.rigidBody.linearVelocity.y:F3}", textStyle);
                   GUILayout.Label($"{rot.x:F3}", textStyle);
                   GUILayout.Label($"{drone.rigidBody.YawVelocity():F3}", textStyle);
                   GUILayout.Label($"{rot.z:F3}", textStyle);
               }
            }
            GUILayout.EndArea();
        }

        private void NavigationBox(int w, int h)
        {
            var boxW = Math.Min(w * navigationPanel_WRatio, navigationPanel_MaxW);
            var boxH = isNavigationVisible 
                ? Math.Min(h * navigationPanel_HRatio, navigationPanel_MaxH)
                : collapsedHeight;
            
            GUILayout.BeginArea(new Rect(screenOffset, screenOffset, boxW, boxH), GUI.skin.box);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("NAVIGATION", boldTextStyle);
                if (GUILayout.Button(isNavigationVisible ? "x" : "+"))
                    isNavigationVisible = !isNavigationVisible;
            }                  
            
            if (!isNavigationVisible)
            {
                GUILayout.EndArea();
                return;
            }
            
            if (!navigator)
            {
                GUILayout.Label($"{nameof(WaypointNavigator)} is not attached", italicTextStyle);
                GUILayout.EndArea();
                return;
            }
            
            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label("Position", textStyle);
                    GUILayout.Label($"{navigator.transform.position:F3}", textStyle);
                }

                if (navigator.IsFinished)
                {
                    using (new GUILayout.VerticalScope())
                    {
                        GUILayout.Label("Waypoint", textStyle);
                        GUILayout.Label("NONE", italicTextStyle);
                    }
                }
                else
                {
                    using (new GUILayout.VerticalScope())
                    {
                        GUILayout.Label($"Waypoint [{navigator.CurrentWaypointIndex + 1}/{navigator.WaypointsCount}]", textStyle);
                        GUILayout.Label($"{navigator.CurrentWaypoint.position:F3}", textStyle);
                    }
                    using (new GUILayout.VerticalScope())
                    {
                        GUILayout.Label("Distance", textStyle);
                        if (navigator.IsFinished)
                            GUILayout.Label("NONE", italicTextStyle);
                        else
                            GUILayout.Label($"{(navigator.transform.position - navigator.CurrentWaypoint.position).magnitude:F3}", textStyle);
                    }   
                }
            }
            GUILayout.EndArea();
        }

    }
}