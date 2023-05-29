namespace Kafka.EventLoop.Utils
{
    internal class LazyFunc<T, TResult> where TResult : class
    {
        private readonly object _lock = new();
        private readonly Func<T, TResult> _provider;
        private volatile TResult? _value;

        public LazyFunc(Func<T, TResult> provider)
        {
            _provider = provider;
        }

        public TResult Invoke(T arg)
        {
            if (_value != null)
                return _value;

            lock (_lock)
            {
                if (_value != null)
                    return _value;

                _value = _provider(arg);
            }

            return _value;
        }
    }
}
