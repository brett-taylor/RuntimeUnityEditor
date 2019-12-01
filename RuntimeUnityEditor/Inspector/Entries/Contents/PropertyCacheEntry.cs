using System;
using System.Reflection;

namespace RuntimeUnityEditor.Core.Inspector.Entries
{
    public class PropertyCacheEntry : CacheEntryBase
    {
        public PropertyCacheEntry(object ins, PropertyInfo p) : base(FieldCacheEntry.GetMemberName(ins, p))
        {
            if (p == null)
                throw new ArgumentNullException(nameof(p));

            _instance = ins;
            PropertyInfo = p;
        }

        public PropertyInfo PropertyInfo { get; }

        private readonly object _instance;

        public override object GetValueToCache()
        {
            if (!PropertyInfo.CanRead)
                return "WRITE ONLY";

            try
            {
                return PropertyInfo.GetValue(_instance, null);
            }
            catch (TargetInvocationException ex)
            {
                return ex.InnerException ?? ex;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        protected override bool OnSetValue(object newValue)
        {
            if (PropertyInfo.CanWrite)
            {
                PropertyInfo.SetValue(_instance, newValue, null);
                return true;
            }
            return false;
        }

        public override Type Type()
        {
            return PropertyInfo.PropertyType;
        }

        public override bool CanSetValue()
        {
            return PropertyInfo.CanWrite;
        }

        public override bool Equals(ICacheEntry other)
        {
            if (other == null)
                return false;

            if (other is PropertyCacheEntry == false)
                return false;

            var otherTyped = (PropertyCacheEntry) other;
            return PropertyInfo == otherTyped.PropertyInfo;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals((PropertyCacheEntry) obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
