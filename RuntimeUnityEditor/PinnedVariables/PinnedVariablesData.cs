using RuntimeUnityEditor.Core.Inspector.Entries;
using System.Collections.Generic;

namespace RuntimeUnityEditor.Core.PinnedVariables
{
    public class PinnedVariablesData
    {
        private readonly Dictionary<ICacheEntry, PinnedVariable> variables = new Dictionary<ICacheEntry, PinnedVariable>();

        public void Track(string name, ICacheEntry entry)
        {
            variables.Add(entry, new PinnedVariable(name));
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

        public Dictionary<ICacheEntry, PinnedVariable>.Enumerator GetEnumerator()
        {
            return variables.GetEnumerator();
        }
    }
}
