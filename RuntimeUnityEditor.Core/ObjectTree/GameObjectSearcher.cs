﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RuntimeUnityEditor.Core.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RuntimeUnityEditor.Core.ObjectTree
{
    /// <summary>
    /// Keeps track of root gameobjects and allows searching objects in the scene
    /// </summary>
    public class GameObjectSearcher
    {
        private List<GameObject> _cachedRootGameObjects;
        private List<GameObject> _searchResults;

        public static IEnumerable<GameObject> FindAllRootGameObjects()
        {
            return Resources.FindObjectsOfTypeAll<Transform>()
                .Where(t => t.parent == null)
                .Select(x => x.gameObject);
        }

        public IEnumerable<GameObject> GetRootObjects()
        {
            if (_cachedRootGameObjects != null)
            {
                _cachedRootGameObjects.RemoveAll(o => o == null);
                return _cachedRootGameObjects;
            }
            return Enumerable.Empty<GameObject>();
        }

        public IEnumerable<GameObject> GetSearchedOrAllObjects()
        {
            if (_searchResults != null)
            {
                _searchResults.RemoveAll(o => o == null);
                return _searchResults;
            }
            return GetRootObjects();
        }

        public void Refresh(bool full, Predicate<GameObject> objectFilter)
        {
            if (_searchResults != null)
                return;

            if (_cachedRootGameObjects == null || full)
            {
                _cachedRootGameObjects = FindAllRootGameObjects().OrderBy(x => x.name, StringComparer.InvariantCultureIgnoreCase).ToList();
                full = true;
            }
            else
            {
                _cachedRootGameObjects.RemoveAll(o => o == null);
            }

            if (UnityFeatureHelper.SupportsScenes && !full)
            {
                var newItems = UnityFeatureHelper.GetSceneGameObjects().Except(_cachedRootGameObjects).ToList();
                if (newItems.Count > 0)
                {
                    _cachedRootGameObjects.AddRange(newItems);
                    _cachedRootGameObjects.Sort((o1, o2) => string.Compare(o1.name, o2.name, StringComparison.InvariantCultureIgnoreCase));
                }
            }

            if (objectFilter != null)
                _cachedRootGameObjects.RemoveAll(objectFilter);
        }

        public void Search(string searchString, bool searchProperties)
        {
            if (string.IsNullOrEmpty(searchString))
                _searchResults = null;
            else
            {
                _searchResults = GetRootObjects()
                    .SelectMany(x => x.GetComponentsInChildren<Transform>(true))
                    .Where(x => x.name.Contains(searchString, StringComparison.InvariantCultureIgnoreCase) || x.GetComponents<Component>().Any(c => SearchInComponent(searchString, c, searchProperties)))
                    .OrderBy(x => x.name, StringComparer.InvariantCultureIgnoreCase)
                    .Select(x => x.gameObject)
                    .ToList();
            }
        }

        private static bool SearchInComponent(string searchString, Object c, bool searchProperties)
        {
            if (c == null) return false;

            var type = c.GetType();
            if (type.Name.Contains(searchString, StringComparison.InvariantCultureIgnoreCase))
                return true;

            if (!searchProperties)
                return false;

            var nameBlacklist = new[] {"parent", "parentInternal", "root", "transform", "gameObject"};
            var typeBlacklist = new[] {typeof(bool)};

            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.CanRead && Array.IndexOf(nameBlacklist, x.Name) < 0 && Array.IndexOf(typeBlacklist, x.PropertyType) < 0))
            {
                try
                {
                    if (prop.GetValue(c, null).ToString().Contains(searchString, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }
                catch
                {
                    // ignored
                }
            }
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => Array.IndexOf(nameBlacklist, x.Name) < 0 && Array.IndexOf(typeBlacklist, x.FieldType) < 0))
            {
                try
                {
                    if (field.GetValue(c).ToString().Contains(searchString, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }
                catch
                {
                    // ignored
                }
            }

            return false;
        }
    }
}
