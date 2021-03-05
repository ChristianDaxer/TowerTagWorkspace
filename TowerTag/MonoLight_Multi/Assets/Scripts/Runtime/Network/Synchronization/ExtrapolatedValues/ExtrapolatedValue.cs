namespace Network.Synchronization.ExtrapolatedValues {
    public abstract class ExtrapolatedValue<T> {
        private T _lastReceivedValue;
        private T _speed;
        private int _lastTimestamp;

        protected T LastReceivedValue => _lastReceivedValue;
        protected T Speed => _speed;
        protected int LastTimestamp => _lastTimestamp;

        public void ReceiveNewValue(int timeStamp, T value) {
            _speed = CalculateSpeed(value, _lastReceivedValue, _lastTimestamp, timeStamp);
            _lastReceivedValue = value;
            _lastTimestamp = timeStamp;
        }

        protected abstract T CalculateSpeed(T oldValue, T newValue, int oldTimestamp, int newTimestamp);
        public abstract T GetExtrapolatedValue();
    }
}