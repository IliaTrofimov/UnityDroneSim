namespace Exceptions
{
    public static class ExceptionHelper
    {
        /// <summary>Throws <see cref="MissingComponentException{T}"/> if parameter obj is null.</summary>
        public static void ThrowIfComponentIsMissing<TImport>(TImport obj, string componentName)
        {
            if (obj == null) throw new MissingComponentException<TImport>(componentName);
        }
        
        /// <summary>Throws <see cref="MissingComponentException{T,T}"/> if parameter obj is null.</summary>
        public static void ThrowIfComponentIsMissing<TImport, TParent>(TParent parent, TImport obj, string componentName)
        {
            if (obj == null) throw new MissingComponentException<TImport, TParent>(componentName);
        } 
        
        /// <summary>Throws <see cref="MissingComponentException{T}"/> if parameter obj is null.</summary>
        public static void ThrowIfComponentIsMissing<TImport>(TImport obj)
        {
            if (obj == null) throw new MissingComponentException<TImport>();
        }
        
        /// <summary>Throws <see cref="MissingComponentException{T,T}"/> if parameter obj is null.</summary>
        public static void ThrowIfComponentIsMissing<TImport, TParent>(TParent parent, TImport obj)
        {
            if (obj == null) throw new MissingComponentException<TImport, TParent>();
        } 
    }
}