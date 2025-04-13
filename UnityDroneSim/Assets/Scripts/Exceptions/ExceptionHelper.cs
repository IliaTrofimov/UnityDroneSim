using UnityEngine;


namespace Exceptions
{
    public static class ExceptionHelper
    {
        /// <summary>Throws <see cref="MissingComponentException"/> if parameter obj is null.</summary>
        public static void ThrowIfComponentIsMissing<TImport>(TImport obj, string componentName)
        {
            if (obj == null)
                throw new MissingComponentException(
                    $"Missing component '{componentName}' of type {typeof(TImport).Name}."
                );
        }

        /// <summary>Throws <see cref="MissingComponentException"/> if parameter obj is null.</summary>
        public static void ThrowIfComponentIsMissing<TImport, TParent>(
            TParent parent,
            TImport obj,
            string componentName)
        {
            if (obj == null)
                throw new MissingComponentException(
                    $"Parent object {typeof(TParent).Name} is missing component '{componentName}' of type {typeof(TImport).Name}."
                );
        }

        /// <summary>Throws <see cref="MissingComponentException"/> if parameter obj is null.</summary>
        public static void ThrowIfComponentIsMissing<TImport>(TImport obj)
        {
            if (obj == null)
                throw new MissingComponentException($"Missing component of type {typeof(TImport).Name}.");
        }

        /// <summary>Throws <see cref="MissingComponentException"/> if parameter obj is null.</summary>
        public static void ThrowIfComponentIsMissing<TImport, TParent>(TParent parent, TImport obj)
        {
            if (obj == null)
                throw new MissingComponentException(
                    $"Parent object {typeof(TParent).Name} is missing component of type {typeof(TImport).Name}."
                );
        }
    }
}