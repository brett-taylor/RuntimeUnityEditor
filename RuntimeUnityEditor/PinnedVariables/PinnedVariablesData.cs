using RuntimeUnityEditor.Core.Inspector.Entries;
using System.Collections.Generic;

namespace RuntimeUnityEditor.Core.PinnedVariables
{
    public class PinnedVariablesData
    {
        private readonly Dictionary<ICacheEntry, string> variables = new Dictionary<ICacheEntry, string>();

        public void Track(string name, ICacheEntry entry)
        {
            variables.Add(entry, name);
        }

        public void Untrack(ICacheEntry entry)
        {
            variables.Remove(entry);
        }

        public bool IsTracked(ICacheEntry entry)
        {
            return variables.ContainsKey(entry);
        }

        public int GetCount()
        {
            return variables.Count;
        }

        public Dictionary<ICacheEntry, string>.Enumerator GetEnumerator()
        {
            return variables.GetEnumerator();
        }
    }
}
