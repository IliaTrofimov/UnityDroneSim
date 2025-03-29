namespace UI
{
    public delegate void ValueChangedEventHandler<in T>(T newValue, T previousValue);

    
    public class TrackedObject<T>
    {
        private T currentValue;
        
        public event ValueChangedEventHandler<T> ValueChanged;
        
        public TrackedObject() => currentValue = default;

        public TrackedObject(T initialValue) => currentValue = initialValue;
        
        
        public void SetValueWithoutNotify(T newValue) => currentValue = newValue;

        public void SetValueWithNotify(T newValue)
        {
            ValueChanged?.Invoke(newValue, currentValue);
            currentValue = newValue;
        }
        
        public T Value
        {
            get => currentValue;
            set
            {
                if (!ValuesEqual(currentValue, value))
                {
                    ValueChanged?.Invoke(value, currentValue);
                    currentValue = value;
                }
            }
        }
        
        protected virtual bool ValuesEqual(T oldValue, T newValue)
        {
            return oldValue.Equals(newValue);
        }
    }
}