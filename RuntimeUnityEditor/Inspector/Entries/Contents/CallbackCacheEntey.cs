using System;

namespace RuntimeUnityEditor.Core.Inspector.Entries
{
    public class CallbackCacheEntry : CacheEntryBase
    {
        private readonly string _message;
        private readonly Action _callback;

        public CallbackCacheEntry(string name, string message, Action callback) : base(name)
        {
            _message = message;
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        public override object GetValueToCache()
        {
            return _message;
        }

        public override bool CanEnterValue()
        {
            return true;
        }

        public override object EnterValue()
        {
            _callback();
            return null;
        }

        protected override bool OnSetValue(object newValue)
        {
            return false;
        }

        public override Type Type()
        {
            return typeof(void);
        }

        public override bool CanSetValue()
        {
            return false;
        }

        public override bool Equals(ICacheEntry other)
        {
            if (other == null)
                return false;

            if (other is CallbackCacheEntry == false)
                return false;

            var otherTyped = (CallbackCacheEntry) other;
            return _callback == otherTyped._callback && _message.Equals(otherTyped._message);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals((CallbackCacheEntry) obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }

    public class CallbackCacheEntry<T> : CacheEntryBase
    {
        private readonly string _message;
        private readonly Func<T> _callback;

        public CallbackCacheEntry(string name, string message, Func<T> callback) : base(name)
        {
            _message = message;
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        public override object GetValueToCache()
        {
            return _message;
        }

        public override bool CanEnterValue()
        {
            return true;
        }

        public override object EnterValue()
        {
            return _callback();
        }

        protected override bool OnSetValue(object newValue)
        {
            return false;
        }

        public override Type Type()
        {
            return typeof(T);
        }

        public override bool CanSetValue()
        {
            return false;
        }

        public override bool Equals(ICacheEntry other)
        {
            if (other == null)
                return false;

            if (other is CallbackCacheEntry<T> == false)
                return false;

            var otherTyped = (CallbackCacheEntry<T>) other;
            return _callback == otherTyped._callback && _message == otherTyped._message;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals((CallbackCacheEntry<T>) obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}