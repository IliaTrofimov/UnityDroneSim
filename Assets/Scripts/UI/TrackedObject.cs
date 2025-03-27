namespace UI
{
    public delegate void ValueChangedEventHandler<T>(ValueChangedEventArgs<T> e);

    
    public class TrackedObject<T>
    {
        private T currentValue;
        
        public event ValueChangedEventHandler<T> ValueChanged;
        
        public TrackedObject() => currentValue = default;

        public TrackedObject(T initialValue) => currentValue = initialValue;
        
        
        public void SetValueWithoutNotify(T newValue) => currentValue = newValue;

        public T Value
        {
            get => currentValue;
            set
            {
                if (!ValuesEqual(currentValue, value))
                {
                    ValueChanged?.Invoke(new ValueChangedEventArgs<T>(currentValue, value));
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