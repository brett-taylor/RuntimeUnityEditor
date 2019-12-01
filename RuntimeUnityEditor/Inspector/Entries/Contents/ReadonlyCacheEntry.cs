using System;

namespace RuntimeUnityEditor.Core.Inspector.Entries
{
    public class ReadonlyCacheEntry : CacheEntryBase
    {
        public readonly object Object;
        private readonly Type _type;
        private string _tostringCache;

        public ReadonlyCacheEntry(string name, object obj) : base(name)
        {
            Object = obj;
            _type = obj.GetType();
        }

        public override object GetValueToCache()
        {
            return Object;
        }

        protected override bool OnSetValue(object newValue)
        {
            return false;
        }

        public override Type Type()
        {
            return _type;
        }

        public override bool CanSetValue()
        {
            return false;
        }

        public override string ToString()
        {
            return _tostringCache ?? (_tostringCache = Name() + " | " + Object);
        }

        public override bool Equals(ICacheEntry other)
        {
            if (other == null)
                return false;

            if (other is ReadonlyCacheEntry == false)
                return false;

            var otherTyped = (ReadonlyCacheEntry) other;
            return Object.Equals(otherTyped.Object);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals((ReadonlyCacheEntry) obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}