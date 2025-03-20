using UnityEngine;


namespace Telemetry
{
    /// <summary>Collection of <see cref="MovementRecord"/> recordered during game.</summary>
    [PreferBinarySerialization]
    [CreateAssetMenu(fileName = "NewObjectMovementLog", menuName = "Telemetry/Object Movement Log")]
    public sealed class ObjectMovementLog : TelemetryLog<MovementRecord>
    {
        public void Add(Vector3 position, Quaternion rotation) 
            => Add(new MovementRecord(position, rotation)); 
        
        public void Add(GameObject gameObject) 
            => Add(new MovementRecord(gameObject)); 
        
        public void Add(Component component) 
            => Add(new MovementRecord(component)); 
        
        public void Add(Transform transform) 
            => Add(new MovementRecord(transform)); 
    }
}