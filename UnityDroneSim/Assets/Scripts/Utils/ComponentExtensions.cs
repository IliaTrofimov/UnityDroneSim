using UnityEngine;


namespace Utils
{
    public static class ComponentExtensions
    {
        public static T TryGetComponent<T>(this Component parent) where T : Component
        {
            if (parent.TryGetComponent(out T child))
                return child;

            return null;
        }

        public static T TryGetComponentHereAndInChildren<T>(this Component parent) where T : Component
        {
            if (parent.TryGetComponent(out T child))
                return child;

            T[] children = parent.GetComponentsInChildren<T>();
            if (children.Length == 0)
                return null;

            if (children.Length == 1)
                return children[0];

            throw new MissingComponentException(
                $"Component {parent.GetType().Name} has more than 1 children of type {typeof(T).Name}"
            );
        }
    }
}