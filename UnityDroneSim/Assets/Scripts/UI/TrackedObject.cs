namespace UI
{
    public delegate void ValueChangedEventHandler<in T>(T newValue, T previousValue);


    /// <summary>
    /// Property that can notify that its value has changed.
    /// </summary>
    public class TrackedObject<T>
    {
        private T _currentValue;

        /// <summary>Value has just changed.</summary>
        public event ValueChangedEventHandler<T> ValueChanged;

        public TrackedObject() { _currentValue = default; }

        public TrackedObject(T initialValue) { _currentValue = initialValue; }


        /// <summary>Update value but not fire notification event.</summary>
        public void SetValueWithoutNotify(T newValue) => _currentValue = newValue;
        
        /// <summary>Update value and always fire notification event.</summary>
        public void SetValueWithNotify(T newValue)
        {
            ValueChanged?.Invoke(newValue, _currentValue);
            _currentValue = newValue;
        }

        /// <summary> Get current or set new value.</summary>
        public T Value
        {
            get => _currentValue;
            set
            {
                if (!ValuesEqual(_currentValue, value))
                    SetValueWithNotify(value);
            }
        }

        protected virtual bool ValuesEqual(T oldValue, T newValue) => oldValue.Equals(newValue);
    }
}