namespace UI
{
    public struct ValueChangedEventArgs<T>
    {
        public T NewValue { get; }
        public T OldValue { get; }

        public ValueChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}