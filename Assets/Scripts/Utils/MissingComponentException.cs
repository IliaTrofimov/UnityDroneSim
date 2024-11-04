namespace Utils
{
    public class MissingComponentException<TImport, TParent> : UnityEngine.MissingComponentException
    {
        public MissingComponentException() 
            : base($"Component {typeof(TParent)} is missing required component  of type {typeof(TImport)}.")
        {
        }
        
        public MissingComponentException(string componentName) 
            : base($"Component {typeof(TParent)} is missing required component {componentName} of type {typeof(TImport)}.")
        {
        }
    }
    
    public class MissingComponentException<TImport> : UnityEngine.MissingComponentException
    {
        public MissingComponentException() 
            : base($"Missing required component  of type {typeof(TImport)}.")
        {
        }
        
        public MissingComponentException(string componentName) 
            : base($"Missing required component {componentName} of type {typeof(TImport)}.")
        {
        }
    }
}