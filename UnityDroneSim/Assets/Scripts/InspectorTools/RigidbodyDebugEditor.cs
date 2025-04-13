using UnityEditor;
using UnityEngine;
using UtilsDebug;


namespace InspectorTools
{
    /// <summary>
    /// Custom editor for <see cref="RigidbodyDebug"/> component.
    /// Adds readonly fields for displaying velocity, acceleration and center of mass values.
    /// </summary>
    [CustomEditor(typeof(RigidbodyDebug))]
    public class RigidbodyDebugEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var script = (RigidbodyDebug)target;
            script.GetBodyParameters(out var linVel,
                out var angVel,
                out var linAcc,
                out var centerOfMass,
                out var cmError
            );

            DrawDefaultInspector();

            var enabled = GUI.enabled;
            GUI.enabled = false;

            GUILayout.Space(10);


            var styleRight = new GUIStyle("label") { alignment = TextAnchor.MiddleRight };
            using (var _ = new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Linear velocity ({linVel.magnitude:F2} m/s):");
                GUILayout.Label($"{linVel:F3}", styleRight, GUILayout.ExpandWidth(true));
            }

            using (var _ = new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Angular velocity ({angVel.magnitude:F2} rad/s):");
                GUILayout.Label($"{angVel:F3}", styleRight, GUILayout.ExpandWidth(true));
            }

            using (var _ = new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Linear acceleration ({linAcc.magnitude:F2} m/s^2):");
                GUILayout.Label($"{linAcc:F3}", styleRight, GUILayout.ExpandWidth(true));
            }

            using (var _ = new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Center of mass:");
                GUILayout.Label($"{centerOfMass:F3}", styleRight, GUILayout.ExpandWidth(true));
            }

            using (var _ = new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Center of mass error:");
                GUILayout.Label(cmError.magnitude > 0 ? $"{cmError:F3}" : "0",
                    styleRight,
                    GUILayout.ExpandWidth(true)
                );
            }

            GUI.enabled = enabled;


            GUILayout.Space(10);
            using (var _ = new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Stop all"))
                    script.StopMovement(true, true);

                if (GUILayout.Button("Stop movement"))
                    script.StopMovement(true, false);

                if (GUILayout.Button("Stop rotation"))
                    script.StopMovement(false, true);

                if (GUILayout.Button("Recover"))
                    script.Recover();
            }
        }
    }
}