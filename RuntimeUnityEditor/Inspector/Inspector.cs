using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using RuntimeUnityEditor.Core.Inspector.Entries;
using RuntimeUnityEditor.Core.PinnedVariables;
using RuntimeUnityEditor.Core.REPL;
using RuntimeUnityEditor.Core.UI;
using RuntimeUnityEditor.Core.Utils;
using UnityEngine;
using Component = UnityEngine.Component;

namespace RuntimeUnityEditor.Core.Inspector
{
    public sealed class Inspector : Window
    {
        private const int InspectorRecordHeight = 25;
        private readonly Action<Transform> _treelistShowCallback;
        private readonly GUILayoutOption[] _inspectorTypeWidth = { GUILayout.Width(170), GUILayout.MaxWidth(170) };
        private readonly GUILayoutOption[] _inspectorNameWidth = { GUILayout.Width(240), GUILayout.MaxWidth(240) };
        private readonly GUILayoutOption _inspectorRecordHeight = GUILayout.Height(InspectorRecordHeight);
        private readonly GUILayoutOption _dnSpyButtonOptions = GUILayout.Width(50);
        private readonly GUILayoutOption _createREPLVariableWidth = GUILayout.Width(80);
        private readonly GUILayoutOption _pinButtonOptions = GUILayout.Width(50);

        private GUIStyle _alignedButtonStyle;

        private Rect _inspectorWindowRect;
        private Vector2 _inspectorStackScrollPos;

        private int _currentVisibleCount;
        private object _currentlyEditingTag;
        private string _currentlyEditingText;
        private bool _userHasHitReturn;

        private string _searchString = "";
        private string SearchString
        {
            get => _searchString;
            // The string can't be null under unity 5.x or we crash
            set => _searchString = value ?? "";
        }
        private bool _focusSearchBox;

        private string _createVariableString = "";
        private string CreateVariableString
        {
            get => _createVariableString;
            // The string can't be null under unity 5.x or we crash
            set => _createVariableString = value ?? "";
        }

        private readonly Dictionary<Type, bool> _canCovertCache = new Dictionary<Type, bool>();
        private readonly List<ICacheEntry> _fieldCache = new List<ICacheEntry>();
        private readonly Stack<InspectorStackEntryBase> _inspectorStack = new Stack<InspectorStackEntryBase>();

        private InspectorStackEntryBase _nextToPush;
        private readonly int _windowId;

        public InspectorStackEntryBase CurrentStackItem => _inspectorStack.Peek();

        internal override WindowState RenderOnlyInWindowState => WindowState.VISIBLE;

        internal override WindowState UpdateOnlyInWindowState => WindowState.VISIBLE;

        protected override bool ShouldEatInput => true;

        protected override bool IsWindowDraggable => true;

        protected override string WindowTitle => "Inspector";

        private ReplWindow _replWindow;

        public Inspector(Action<Transform> treelistShowCallback, ReplWindow replWindow)
        {
            _treelistShowCallback = treelistShowCallback ?? throw new ArgumentNullException(nameof(treelistShowCallback));
            _windowId = GetHashCode();
            _replWindow = replWindow;
        }

        private static IEnumerable<ICacheEntry> MethodsToCacheEntries(object instance, Type instanceType, MethodInfo[] methodsToCheck)
        {
            var cacheItems = methodsToCheck
                .Where(x => !x.IsConstructor && !x.IsSpecialName && x.GetParameters().Length == 0)
                .Where(f => !f.IsDefined(typeof(CompilerGeneratedAttribute), false))
                .Where(x => x.Name != "MemberwiseClone" && x.Name != "obj_address") // Instant game crash
                .Select(m =>
                {
                    if (m.ContainsGenericParameters)
                        try
                        {
                            return m.MakeGenericMethod(instanceType);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    return m;
                }).Where(x => x != null)
                .Select(m => new MethodCacheEntry(instance, m)).Cast<ICacheEntry>();
            return cacheItems;
        }

        private void CacheAllMembers(object objectToOpen)
        {
            _fieldCache.Clear();

            if (objectToOpen == null) return;

            var type = objectToOpen.GetType();

            try
            {
                if (objectToOpen is Component cmp)
                {
                    _fieldCache.Add(new CallbackCacheEntry("Open in Scene Object Browser", "Navigate to GameObject this Component is attached to", () => _treelistShowCallback(cmp.transform)));
                }
                else if (objectToOpen is GameObject castedObj)
                {
                    _fieldCache.Add(new CallbackCacheEntry("Open in Scene Object Browser", "Navigate to this object in the Scene Object Browser", () => _treelistShowCallback(castedObj.transform)));
                    _fieldCache.Add(new ReadonlyCacheEntry("Child objects", castedObj.transform.Cast<Transform>().ToArray()));
                    _fieldCache.Add(new ReadonlyCacheEntry("Components", castedObj.GetComponents<Component>()));
                }

                // If we somehow enter a string, this allows user to see what the string actually says
                if (type == typeof(string))
                {
                    _fieldCache.Add(new ReadonlyCacheEntry("this", objectToOpen));
                }
                else if (objectToOpen is Transform)
                {
                    // Prevent the list overloads from listing subcomponents
                }
                else if (objectToOpen is IList list)
                {
                    for (var i = 0; i < list.Count; i++)
                        _fieldCache.Add(new ListCacheEntry(list, i));
                }
                else if (objectToOpen is IEnumerable enumerable)
                {
                    _fieldCache.AddRange(enumerable.Cast<object>()
                        .Select((x, y) => x is ICacheEntry ? x : new ReadonlyListCacheEntry(x, y))
                        .Cast<ICacheEntry>());
                }

                // Instance members
                _fieldCache.AddRange(type
                    .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                               BindingFlags.FlattenHierarchy)
                    .Where(f => !f.IsDefined(typeof(CompilerGeneratedAttribute), false))
                    .Select(f => new FieldCacheEntry(objectToOpen, f)).Cast<ICacheEntry>());
                _fieldCache.AddRange(type
                    .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                   BindingFlags.FlattenHierarchy)
                    .Where(f => !f.IsDefined(typeof(CompilerGeneratedAttribute), false))
                    .Select(p => new PropertyCacheEntry(objectToOpen, p)).Cast<ICacheEntry>());
                _fieldCache.AddRange(MethodsToCacheEntries(objectToOpen, type,
                    type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                    BindingFlags.FlattenHierarchy)));

                CacheStaticMembersHelper(type);
            }
            catch (Exception ex)
            {
                RuntimeUnityEditorCore.LOGGER.Log(LogLevel.Warning, "[Inspector] CacheFields crash: " + ex);
            }
        }

        private void CacheStaticMembers(Type type)
        {
            _fieldCache.Clear();

            if (type == null) return;

            try
            {
                CacheStaticMembersHelper(type);
            }
            catch (Exception ex)
            {
                RuntimeUnityEditorCore.LOGGER.Log(LogLevel.Warning, "[Inspector] CacheFields crash: " + ex);
            }
        }

        private void CacheStaticMembersHelper(Type type)
        {
            _fieldCache.AddRange(type
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                           BindingFlags.FlattenHierarchy)
                .Where(f => !f.IsDefined(typeof(CompilerGeneratedAttribute), false))
                .Select(f => new FieldCacheEntry(null, f)).Cast<ICacheEntry>());
            _fieldCache.AddRange(type
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                               BindingFlags.FlattenHierarchy)
                .Where(f => !f.IsDefined(typeof(CompilerGeneratedAttribute), false))
                .Select(p => new PropertyCacheEntry(null, p)).Cast<ICacheEntry>());
            _fieldCache.AddRange(MethodsToCacheEntries(null, type,
                type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                                BindingFlags.FlattenHierarchy)));
        }

        private bool CanCovert(string value, Type type)
        {
            if (_canCovertCache.ContainsKey(type))
                return _canCovertCache[type];

            try
            {
                var _ = Convert.ChangeType(value, type);
                _canCovertCache[type] = true;
                return true;
            }
            catch
            {
                _canCovertCache[type] = false;
                return false;
            }
        }

        private void DrawEditableValue(ICacheEntry field, object value, params GUILayoutOption[] layoutParams)
        {
            var isBeingEdited = _currentlyEditingTag == field;
            var text = isBeingEdited ? _currentlyEditingText : ToStringConverter.ObjectToString(value);
            var result = GUILayout.TextField(text, layoutParams);

            if (!Equals(text, result) || isBeingEdited)
                if (_userHasHitReturn)
                {
                    _currentlyEditingTag = null;
                    _userHasHitReturn = false;
                    try
                    {
                        var converted = Convert.ChangeType(result, field.Type());
                        if (!Equals(converted, value))
                            field.SetValue(converted);
                    }
                    catch (Exception ex)
                    {
                        RuntimeUnityEditorCore.LOGGER.Log(LogLevel.Error, "[Inspector] Failed to set value - " + ex.Message);
                    }
                }
                else
                {
                    _currentlyEditingText = result;
                    _currentlyEditingTag = field;
                }
        }

        private void DrawValue(object value, params GUILayoutOption[] layoutParams)
        {
            GUILayout.TextArea(ToStringConverter.ObjectToString(value), GUI.skin.label, layoutParams);
        }

        private void DrawVariableName(ICacheEntry field)
        {
            GUILayout.TextArea(field.Name(), GUI.skin.label, _inspectorNameWidth);
        }

        private void DrawVariableNameEnterButton(ICacheEntry field)
        {
            if (GUILayout.Button(field.Name(), _alignedButtonStyle, _inspectorNameWidth))
            {
                var val = field.EnterValue();
                if (val != null)
                {
                    if (val is InspectorStackEntryBase sb)
                        _nextToPush = sb;
                    else
                        _nextToPush = new InstanceStackEntry(val, field.Name());
                }
            }
        }

        public void InspectorClear()
        {
            _inspectorStack.Clear();
            CacheAllMembers(null);
        }

        private void InspectorPop()
        {
            _focusSearchBox = true;
            SearchString = null;

            _inspectorStack.Pop();
            LoadStackEntry(_inspectorStack.Peek());
        }

        public void InspectorPush(InspectorStackEntryBase stackEntry)
        {
            _focusSearchBox = true;
            SearchString = null;

            _inspectorStack.Push(stackEntry);
            LoadStackEntry(stackEntry);

            GenerateVariableName(stackEntry);
        }

        private void GenerateVariableName(InspectorStackEntryBase stackEntry)
        {
            if (stackEntry.Name.Length >= 2)
            {
                CreateVariableString = char.ToLowerInvariant(stackEntry.Name[0]) + stackEntry.Name.Substring(1);
                CreateVariableString = CreateVariableString.Replace(" ", "");
                CreateVariableString = CreateVariableString.Replace(":", "");
            }
            else
            {
                CreateVariableString = null;
            }
        }

        public object GetInspectedObject()
        {
            if (_inspectorStack.Count > 0 && _inspectorStack.Peek() is InstanceStackEntry se)
                return se.Instance;
            return null;
        }

        private void LoadStackEntry(InspectorStackEntryBase stackEntry)
        {
            switch (stackEntry)
            {
                case InstanceStackEntry instanceStackEntry:
                    CacheAllMembers(instanceStackEntry.Instance);
                    break;
                case StaticStackEntry staticStackEntry:
                    CacheStaticMembers(staticStackEntry.StaticType);
                    break;
                default:
                    throw new InvalidEnumArgumentException("Invalid stack entry type: " + stackEntry.GetType().FullName);
            }
        }

        private const string SearchBoxName = "InspectorFilterBox";

        private void DrawContentScrollView()
        {
            if (_inspectorStack.Count == 0) return;

            var currentItem = CurrentStackItem;
            currentItem.ScrollPosition = GUILayout.BeginScrollView(currentItem.ScrollPosition);
            {
                GUILayout.BeginVertical();
                {
                    var visibleFields = string.IsNullOrEmpty(SearchString) ?
                        _fieldCache :
                        _fieldCache.Where(x => x.Name().Contains(SearchString, StringComparison.OrdinalIgnoreCase) || x.TypeName().Contains(SearchString, StringComparison.OrdinalIgnoreCase)).ToList();

                    var firstIndex = (int)(currentItem.ScrollPosition.y / InspectorRecordHeight);

                    GUILayout.Space(firstIndex * InspectorRecordHeight);

                    _currentVisibleCount = (int)(_inspectorWindowRect.height / InspectorRecordHeight) - 4;
                    for (var index = firstIndex; index < Mathf.Min(visibleFields.Count, firstIndex + _currentVisibleCount); index++)
                    {
                        var entry = visibleFields[index];
                        try
                        {
                            DrawSingleContentEntry(entry);
                        }
                        catch (ArgumentException)
                        {
                            // Needed to avoid GUILayout: Mismatched LayoutGroup.Repaint crashes on large lists
                        }
                    }
                    try
                    {
                        GUILayout.Space(Mathf.Max(_inspectorWindowRect.height / 2, (visibleFields.Count - firstIndex - _currentVisibleCount) * InspectorRecordHeight));
                    }
                    catch
                    {
                        // Needed to avoid GUILayout: Mismatched LayoutGroup.Repaint crashes on large lists
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
        }

        private void DrawSingleContentEntry(ICacheEntry entry)
        {
            GUILayout.BeginHorizontal((_inspectorRecordHeight));
            {
                GUILayout.Label($"Type: {entry.GetType()}");
                GUILayout.Label(entry.TypeName(), _inspectorTypeWidth);

                var value = entry.GetValue();

                if (entry.CanEnterValue() || value is Exception)
                    DrawVariableNameEnterButton(entry);
                else
                    DrawVariableName(entry);

                if (entry.CanSetValue() &&
                    CanCovert(ToStringConverter.ObjectToString(value), entry.Type()))
                    DrawEditableValue(entry, value, GUILayout.ExpandWidth(true));
                else
                    DrawValue(value, GUILayout.ExpandWidth(true));

                if (DnSpyHelper.IsAvailable && GUILayout.Button("DNSpy", _dnSpyButtonOptions))
                    DnSpyHelper.OpenInDnSpy(entry);

                if (RuntimeUnityEditorCore.INSTANCE.PinnedVariablesData.IsTracked(entry))
                {
                    if (GUILayout.Button("Unpin", _pinButtonOptions))
                        RuntimeUnityEditorCore.INSTANCE.PinnedVariablesData.Untrack(entry);
                }
                else
                {
                    if (GUILayout.Button("Pin", _pinButtonOptions))
                        RuntimeUnityEditorCore.INSTANCE.PinnedVariablesData.Track(entry.Name(), entry);
                }
            }
            GUILayout.EndHorizontal();
        }

        internal override void Update()
        {
            if (_nextToPush != null)
            {
                InspectorPush(_nextToPush);

                _nextToPush = null;
            }
        }

        private void CreateREPLVariable()
        {
            if (CreateVariableString.Length > 0 && CurrentStackItem is InstanceStackEntry)
            {
                _replWindow.InputField = $"var {CreateVariableString} = ({((InstanceStackEntry)CurrentStackItem).Instance.GetType()}) ((InstanceStackEntry) RuntimeUnityEditorCore.INSTANCE.Inspector.CurrentStackItem).Instance;";
                _replWindow.AcceptInput();
            }
        }

        protected override bool PreCreatedWindow()
        {
            if (_alignedButtonStyle == null)
            {
                _alignedButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = true
                };
            }

            if (Event.current.isKey && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)) _userHasHitReturn = true;

            while (_inspectorStack.Count > 0 && !_inspectorStack.Peek().EntryIsValid())
            {
                var se = _inspectorStack.Pop();
                RuntimeUnityEditorCore.LOGGER.Log(LogLevel.Message, $"[Inspector] Removed invalid/removed stack object: \"{se.Name}\"");
            }

            return _inspectorStack.Count != 0;
        }

        protected override void PostCreatedWindow()
        {
        }

        protected override void DrawWindowContents()
        {
            try
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true));
                        {
                            GUILayout.Label("Filter:", GUILayout.ExpandWidth(false));

                            GUI.SetNextControlName(SearchBoxName);
                            SearchString = GUILayout.TextField(SearchString, GUILayout.ExpandWidth(true));

                            if (_focusSearchBox)
                            {
                                GUI.FocusWindow(_windowID);
                                GUI.FocusControl(SearchBoxName);
                                _focusSearchBox = false;
                            }

                            GUILayout.Label("Find:", GUILayout.ExpandWidth(false));
                            foreach (var obj in new[]
                            {
                                new KeyValuePair<object, string>(
                                    EditorUtilities.GetInstanceClassScanner().OrderBy(x => x.Name()), "Instances"),
                                new KeyValuePair<object, string>(EditorUtilities.GetComponentScanner().OrderBy(x => x.Name()),
                                    "Components"),
                                new KeyValuePair<object, string>(
                                    EditorUtilities.GetMonoBehaviourScanner().OrderBy(x => x.Name()), "MonoBehaviours"),
                                new KeyValuePair<object, string>(EditorUtilities.GetTransformScanner().OrderBy(x => x.Name()),
                                    "Transforms")
                                //                            new KeyValuePair<object, string>(GetTypeScanner(_inspectorStack.Peek().GetType()).OrderBy(x=>x.Name()), _inspectorStack.Peek().GetType().ToString()+"s"),
                            })
                            {
                                if (obj.Key == null) continue;
                                if (GUILayout.Button(obj.Value, GUILayout.ExpandWidth(false)))
                                {
                                    InspectorClear();
                                    InspectorPush(new InstanceStackEntry(obj.Key, obj.Value));
                                }
                            }
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(6);

                        GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Width(160));
                        {
                            if (GUILayout.Button("Help"))
                                InspectorPush(InspectorHelpObject.Create());
                            if (GUILayout.Button("Close"))
                                InspectorClear();
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndHorizontal();

                    _inspectorStackScrollPos = GUILayout.BeginScrollView(_inspectorStackScrollPos, true, false,
                        GUI.skin.horizontalScrollbar, GUIStyle.none, GUIStyle.none, GUILayout.Height(46));
                    {
                        GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(false),
                            GUILayout.ExpandHeight(false));
                        foreach (var item in _inspectorStack.Reverse().ToArray())
                            if (GUILayout.Button(item.Name, GUILayout.ExpandWidth(false)))
                            {
                                while (_inspectorStack.Peek() != item)
                                    InspectorPop();

                                return;
                            }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndScrollView();

                    GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true));
                    {
                        GUILayout.Label("Create REPL Variable:", GUILayout.ExpandWidth(false));
                        CreateVariableString = GUILayout.TextField(CreateVariableString, GUILayout.ExpandWidth(true));
                        if (GUILayout.Button("Create", _createREPLVariableWidth))
                            CreateREPLVariable();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginVertical(GUI.skin.box);
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(1);
                            GUILayout.Label("Value/return type", GUI.skin.box, _inspectorTypeWidth);
                            GUILayout.Space(2);
                            GUILayout.Label("Member name", GUI.skin.box, _inspectorNameWidth);
                            GUILayout.Space(1);
                            GUILayout.Label("Value", GUI.skin.box, GUILayout.ExpandWidth(true));
                        }
                        GUILayout.EndHorizontal();

                        DrawContentScrollView();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
            catch (Exception ex)
            {
                RuntimeUnityEditorCore.LOGGER.Log(LogLevel.Error, "[Inspector] GUI crash: " + ex);
                InspectorClear();
            }
        }

        internal override Rect GetStartingRect(Rect screenSize, float centerWidth, float centerX)
        {
            var inspectorHeight = (int) (screenSize.height / 4) * 3;

            var windowRect = new Rect(
                centerX,
                screenSize.yMin,
                centerWidth,
                inspectorHeight
            );

            _inspectorWindowRect = windowRect;
            return windowRect;
        }
    }
}