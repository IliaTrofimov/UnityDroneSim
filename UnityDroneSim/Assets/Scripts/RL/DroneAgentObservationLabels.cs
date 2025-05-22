using System.Collections.Generic;


namespace RL
{
    public static class DroneAgentObservationLabels
    {
        public static IReadOnlyList<(string label, string format)> GetLabels(bool normalized, bool useAdditional)
        {
            if (useAdditional)
            {
                return normalized ? NormalizedLabelsFull : DenormalizedLabelsFull;
            }
            else
            {
                return normalized ? NormalizedLabels : DenormalizedLabels;
            }
        }
        
        private static readonly IReadOnlyList<(string label, string format)> NormalizedLabelsFull = new[]
        {
            ("IsDestroyed", "{0:f0}"),
            ("IsLanded", "{0:f0}"),
            ("norm Waypoint.dist.", "{0:f3}"),
            ("norm Waypoint.heading.vert", "{0:f3}"),
            ("norm Waypoint.heading.hor", "{0:f3}"),
            ("norm Altitude", "{0:f3}"),
            ("norm Lin.velocity.x", "{0:f3}"),
            ("norm Lin.velocity.y", "{0:f3}"),
            ("norm Lin.velocity.z", "{0:f3}"),
            ("norm Ang.velocity.x", "{0:f3}"),
            ("norm Ang.velocity.y", "{0:f3}"),
            ("norm Ang.velocity.z", "{0:f3}"),
        };
        
        private static readonly IReadOnlyList<(string label, string format)> DenormalizedLabelsFull = new[]
        {
            ("IsDestroyed", "{0:f0}"),
            ("IsLanded", "{0:f0}"),
            ("Waypoint.dist.", "{0:f3} m"),
            ("Waypoint.heading.vert", "{0:f1}°"),
            ("Waypoint.heading.hor", "{0:f1}°"),
            ("Altitude", "{0:f3} m"),
            ("Lin.velocity.x", "{0:f3} m/s"),
            ("Lin.velocity.y", "{0:f3} m/s"),
            ("Lin.velocity.z", "{0:f3} m/s"),
            ("Ang.velocity.x", "{0:f1} rad/s"),
            ("Ang.velocity.y", "{0:f1} rad/s"),
            ("Ang.velocity.z", "{0:f1} rad/s"),
        };
        
        private static readonly IReadOnlyList<(string label, string format)> NormalizedLabels = new[]
        {
            ("norm Waypoint.dist.", "{0:f3}"),
            ("norm Waypoint.heading.vert", "{0:f3}"),
            ("norm Waypoint.heading.hor", "{0:f3}"),
            ("norm Altitude", "{0:f3}"),
            ("norm Lin.velocity.x", "{0:f3}"),
            ("norm Lin.velocity.y", "{0:f3}"),
            ("norm Lin.velocity.z", "{0:f3}"),
        };
        
        private static readonly IReadOnlyList<(string label, string format)> DenormalizedLabels = new[]
        {
            ("Waypoint.dist.", "{0:f3} m"),
            ("Waypoint.heading.vert", "{0:f1}°"),
            ("Waypoint.heading.hor", "{0:f1}°"),
            ("Altitude", "{0:f3} m"),
            ("Lin.velocity.x", "{0:f3} m/s"),
            ("Lin.velocity.y", "{0:f3} m/s"),
            ("Lin.velocity.z", "{0:f3} m/s"),
        };
    }
}