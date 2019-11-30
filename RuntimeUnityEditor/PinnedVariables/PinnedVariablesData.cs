using Mono.CSharp;
using RuntimeUnityEditor.Core.Inspector.Entries;
using System.Collections.Generic;

namespace RuntimeUnityEditor.Core.PinnedVariables
{
    public class PinnedVariablesData
    {
        private readonly HashSet<Tuple<string, ICacheEntry>> variables = new HashSet<Tuple<string, ICacheEntry>>();

        public void Track(string name, ICacheEntry entry)
        {
            variables.Add(new Tuple<string, ICacheEntry>(name, entry));
        }

        public void Untrack(string name, ICacheEntry entry)
        {
            variables.Remove(new Tuple<string, ICacheEntry>(name, entry));
        }

        public bool IsTracked(string name, ICacheEntry entry)
        {
            return variables.Contains(new Tuple<string, ICacheEntry>(name, entry));
        }

        public int GetCount()
        {
            return variables.Count;
        }
    }
}
