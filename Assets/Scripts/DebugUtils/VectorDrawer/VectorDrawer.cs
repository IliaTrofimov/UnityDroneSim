using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace DebugUtils.VectorDrawer
{
    /// <summary>
    /// Helper component that can draw points, lines, vectors and their labels.
    /// </summary>
    [DisallowMultipleComponent]
    public class VectorDrawer : MonoBehaviour
    {
        [Range(0.01f, 10f)] public float pointSize = .15f;
        public bool hideSmallVectors = false;
        public bool drawLabels = true;
        
        #if UNITY_EDITOR
        private readonly Dictionary<string, GizmoCommand> gizmosDict = new();
        #endif

        #region Drawing
        
        /// <summary>Draw line.</summary>
        public void DrawLine(string id, Vector3 from, Vector3 to, Color? color = null)
        {
            #if UNITY_EDITOR
            if (!gizmosDict.ContainsKey(id))
                gizmosDict[id] = new GizmoCommand();
            gizmosDict[id].Line(from, to, color);
            #endif
        }

        /// <summary>Draw single point.</summary>
        public void DrawPoint(string id, Vector3 point, Color? color = null)
        {
            #if UNITY_EDITOR
            if (!gizmosDict.ContainsKey(id))
                gizmosDict[id] = new GizmoCommand();
            gizmosDict[id].Point(point, color);
            #endif
        }

        /// <summary>Draw vector from two points.</summary>
        public void DrawVector(string id, Vector3 from, Vector3 to, Color? color = null)
        {
            #if UNITY_EDITOR
            if (!gizmosDict.ContainsKey(id))
                gizmosDict[id] = new GizmoCommand();
            gizmosDict[id].Vector(from, to, color);
            #endif
        }
        
        /// <summary>Draw direction.</summary>
        public void DrawDirection(string id, Vector3 origin, Vector3 direction, Color? color = null)
        {
            #if UNITY_EDITOR
            if (!gizmosDict.ContainsKey(id))
                gizmosDict[id] = new GizmoCommand();
            gizmosDict[id].Direction(origin, direction, color);
            #endif
        }
        
        #endregion

        #region Clearing
        
        /// <summary>Remove one gizmo drawn by this component by its id.</summary>
        public void ClearGizmo(string id)
        {
            #if UNITY_EDITOR
            gizmosDict.Remove(id);
            #endif
        }

        /// <summary>Remove all gizmos drawn by this component.</summary>
        public void ClearGizmos()
        {
            #if UNITY_EDITOR
            gizmosDict.Clear();
            #endif
        }
        
        /// <summary>Remove gizmos drawn by this component by ids.</summary>
        public void ClearGizmos(params string[] ids)
        {
            #if UNITY_EDITOR
            for (var i = 0; i < ids.Length; i++) gizmosDict.Remove(ids[i]);
            #endif
        }

        #endregion
        
        
        #if UNITY_EDITOR
        private void OnDisable() => gizmosDict.Clear();
        
        private void OnDrawGizmos() 
        {
            foreach (var (key, command) in gizmosDict)
            {
                Gizmos.color = command.color;
                
                var cam = UnityEngine.Camera.current;
                var shouldDraw = !hideSmallVectors || (command.start - command.end != Vector3.zero);
                
                switch (command.flags)
                {
                    case GizmoFlags.Point:
                    {
                        var size = HandleUtility.GetHandleSize(command.end) * pointSize;
                        Gizmos.DrawSphere(command.end, size * pointSize);
                        DrawLabel(cam, command.end, command.color, key, isPoint: true);
                        break;
                    }
                    case GizmoFlags.Vector when shouldDraw:
                    {
                        DrawVectorGizmos(command.start, command.end);
                        DrawLabel(cam, command.start, command.color, key);
                        break;
                    }
                    case GizmoFlags.Line when shouldDraw:
                    {
                        Gizmos.DrawLine(command.start, command.end);
                        DrawLabel(cam, command.start, command.color, key);
                        break;
                    }
                }
            }
        }
        
        private void DrawVectorGizmos(Vector3 from, Vector3 to)
        {
            if (from - to == Vector3.zero) return;
            
            var size = HandleUtility.GetHandleSize(to) * pointSize;
            var q = Quaternion.LookRotation((to - from).normalized);
            
            Gizmos.DrawLine(from, to);
            Gizmos.DrawLine(to - q * ((Vector3.forward + Vector3.right).normalized * size), to);
            Gizmos.DrawLine(to - q * ((Vector3.forward - Vector3.right).normalized * size), to);
        }
        
        private void DrawLabel(UnityEngine.Camera cam, Vector3 v, Color color, string label, bool isPoint = false)
        {
            if (!drawLabels) return;
            
            var point = cam.WorldToScreenPoint(v);
            if (point.z > 0 && new Rect(0, 0, cam.pixelWidth, cam.pixelHeight).Contains(point))
            {
                Handles.Label(v, label, new GUIStyle("label")
                {
                    normal = { textColor = color },
                    contentOffset = new Vector2(2, -7),
                    fontStyle = isPoint ? FontStyle.Italic : FontStyle.Bold
                });
            }
        }
        
        #endif
    }
}