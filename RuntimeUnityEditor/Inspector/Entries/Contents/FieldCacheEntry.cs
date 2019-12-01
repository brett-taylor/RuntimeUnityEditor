using System;
using System.Reflection;

namespace RuntimeUnityEditor.Core.Inspector.Entries
{
    public class FieldCacheEntry : CacheEntryBase
    {
        public FieldCacheEntry(object ins, FieldInfo f) : base(GetMemberName(ins, f))
        {
            if (f == null)
                throw new ArgumentNullException(nameof(f));

            _instance = ins;
            FieldInfo = f;
        }

        internal static string GetMemberName(object ins, MemberInfo f)
        {
            if (ins != null) return f?.Name;
            return "S/" + f?.Name;
        }

        public FieldInfo FieldInfo { get; }
        private readonly object _instance;

        public override object GetValueToCache()
        {
            return FieldInfo.GetValue(_instance);
        }

        protected override bool OnSetValue(object newValue)
        {
            if (!FieldInfo.IsInitOnly)
            {
                FieldInfo.SetValue(_instance, newValue);
                return true;
            }
            return false;
        }

        public override Type Type()
        {
            return FieldInfo.FieldType;
        }

        public override bool CanSetValue()
        {
            return (FieldInfo.Attributes & FieldAttributes.Literal) == 0;
        }

        public override bool Equals(ICacheEntry other)
        {
            if (other == null)
                return false;

            if (other is FieldCacheEntry == false)
                return false;

            var otherTyped = (FieldCacheEntry) other;
            return FieldInfo == otherTyped.FieldInfo;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals((FieldCacheEntry) obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
