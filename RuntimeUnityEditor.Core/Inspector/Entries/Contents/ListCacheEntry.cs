﻿using System;
using System.Collections;

namespace RuntimeUnityEditor.Core.Inspector.Entries
{
    public class ListCacheEntry : CacheEntryBase
    {
        private Type _type;
        private readonly IList _list;
        private readonly int _index;

        public ListCacheEntry(IList container, int index) : base(ReadonlyListCacheEntry.GetListItemName(index))
        {
            _index = index;
            _list = container;
        }

        public override object GetValueToCache()
        {
            return _list.Count > _index ? _list[_index] : "ERROR: The list was changed while browsing!";
        }

        protected override bool OnSetValue(object newValue)
        {
            if (CanSetValue())
            {
                _list[_index] = newValue;
                _type = null;
                return true;
            }

            return false;
        }

        public override Type Type()
        {
            return _type ?? (_type = GetValue()?.GetType());
        }

        public override bool CanSetValue()
        {
            return !_list.IsReadOnly;
        }

        public override bool Equals(ICacheEntry other)
        {
            if (other == null)
                return false;

            if (other is ListCacheEntry == false)
                return false;

            var otherTyped = (ListCacheEntry) other;
            return _list == otherTyped._list;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals((ListCacheEntry) obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
