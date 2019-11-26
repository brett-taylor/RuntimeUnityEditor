﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RuntimeUnityEditor.Core.Gizmos;
using RuntimeUnityEditor.Core.Inspector;
using RuntimeUnityEditor.Core.Inspector.Entries;
using RuntimeUnityEditor.Core.Settings;
using RuntimeUnityEditor.Core.UI;
using RuntimeUnityEditor.Core.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RuntimeUnityEditor.Core.ObjectTree
{
    public sealed class ObjectTreeViewer
    {
        internal Action<InspectorStackEntryBase[]> InspectorOpenCallback;
        internal Action<Transform> TreeSelectionChangedCallback;

        private readonly HashSet<GameObject> _openedObjects = new HashSet<GameObject>();
        private Vector2 _propertiesScrollPosition;
        private Transform _selectedTransform;
        private Vector2 _treeScrollPosition;
        private Rect _windowRect;
        private float _objectTreeHeight;
        private int _singleObjectTreeItemHeight;
        private bool _scrollTreeToSelected;
        private bool _enabled;
        private readonly LayerMask _clickMask = ~(1 << LayerMask.NameToLayer("layer19"));
        private readonly GameObjectSearcher _gameObjectSearcher;
        private readonly Dictionary<Image, Texture2D> _imagePreviewCache = new Dictionary<Image, Texture2D>();
        private readonly GUILayoutOption _drawVector3FieldWidth = GUILayout.Width(38);
        private readonly GUILayoutOption _drawVector3FieldHeight = GUILayout.Height(19);
        private readonly GUILayoutOption _drawVector3SliderHeight = GUILayout.Height(10);
        private readonly GUILayoutOption _drawVector3SliderWidth = GUILayout.Width(33);
        private readonly int _windowId;
        private bool _actuallyInsideOnGui;

        public ObjectTreeViewer(MonoBehaviour pluginObject, GameObjectSearcher gameObjectSearcher)
        {
            if (pluginObject == null) throw new ArgumentNullException(nameof(pluginObject));
            _gameObjectSearcher = gameObjectSearcher ?? throw new ArgumentNullException(nameof(gameObjectSearcher));
            _windowId = GetHashCode();

            pluginObject.StartCoroutine(SetWireframeCo());
        }

        public void SelectAndShowObject(Transform target)
        {
            SelectedTransform = target;

            if (target == null) return;

            target = target.parent;
            while (target != null)
            {
                _openedObjects.Add(target.gameObject);
                target = target.parent;
            }

            _scrollTreeToSelected = true;
            Enabled = true;
        }

        private IEnumerator SetWireframeCo()
        {
            while (true)
            {
                yield return null;

                _actuallyInsideOnGui = true;

                yield return new WaitForEndOfFrame();

                if (GL.wireframe != RuntimeUnityEditorCore.INSTANCE.SettingsData.Wireframe)
                    GL.wireframe = RuntimeUnityEditorCore.INSTANCE.SettingsData.Wireframe;

                _actuallyInsideOnGui = false;
            }
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (value && !_enabled)
                    ClearCaches();

                _enabled = value;
            }
        }

        public Transform SelectedTransform
        {
            get { return _selectedTransform; }
            set
            {
                _selectedTransform = value;
                TreeSelectionChangedCallback?.Invoke(_selectedTransform);
            }
        }

        public void ClearCaches()
        {
            _imagePreviewCache.Clear();
        }

        private void OnInspectorOpen(params InspectorStackEntryBase[] items)
        {
            InspectorOpenCallback.Invoke(items);
        }

        public void UpdateWindowSize(Rect windowRect)
        {
            _windowRect = windowRect;
            _objectTreeHeight = _windowRect.height / 3;
        }

        private void DisplayObjectTreeHelper(GameObject go, int indent, ref int currentCount)
        {
            currentCount++;

            var needsHeightMeasure = _singleObjectTreeItemHeight == 0;

            var isVisible = currentCount * _singleObjectTreeItemHeight >= _treeScrollPosition.y &&
                            (currentCount - 1) * _singleObjectTreeItemHeight <= _treeScrollPosition.y + _objectTreeHeight;

            if (needsHeightMeasure || isVisible)
            {
                var c = GUI.color;
                if (SelectedTransform == go.transform)
                {
                    GUI.color = Color.cyan;
                    if (_scrollTreeToSelected && Event.current.type == EventType.Repaint)
                    {
                        _scrollTreeToSelected = false;
                        _treeScrollPosition.y = GUILayoutUtility.GetLastRect().y - 50;
                    }
                }
                else if (!go.activeSelf)
                {
                    GUI.color = new Color(1, 1, 1, 0.6f);
                }

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(indent * 20f);

                    GUILayout.BeginHorizontal();
                    {
                        if (go.transform.childCount != 0)
                        {
                            if (GUILayout.Toggle(_openedObjects.Contains(go), "", GUILayout.ExpandWidth(false)))
                                _openedObjects.Add(go);
                            else
                                _openedObjects.Remove(go);
                        }
                        else
                        {
                            GUILayout.Space(20f);
                        }

                        if (GUILayout.Button(go.name, GUI.skin.label, GUILayout.ExpandWidth(true), GUILayout.MinWidth(200)))
                        {
                            if (SelectedTransform == go.transform)
                            {
                                // Toggle on/off
                                if (!_openedObjects.Add(go))
                                    _openedObjects.Remove(go);
                            }
                            else
                            {
                                SelectedTransform = go.transform;
                            }
                        }

                        GUI.color = c;
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndHorizontal();

                if (needsHeightMeasure && Event.current.type == EventType.Repaint)
                    _singleObjectTreeItemHeight = Mathf.CeilToInt(GUILayoutUtility.GetLastRect().height);
            }
            else
            {
                GUILayout.Space(_singleObjectTreeItemHeight);
            }

            if (_openedObjects.Contains(go))
            {
                for (var i = 0; i < go.transform.childCount; ++i)
                    DisplayObjectTreeHelper(go.transform.GetChild(i).gameObject, indent + 1, ref currentCount);
            }
        }

        public void DisplayViewer()
        {
            if (RuntimeUnityEditorCore.INSTANCE.SettingsData.Wireframe && _actuallyInsideOnGui && Event.current.type == EventType.Layout)
                GL.wireframe = false;

            if (Enabled)
            {
                _windowRect = GUILayout.Window(_windowId, _windowRect, WindowFunc, "Scene Object Browser");
                InterfaceMaker.EatInputInRect(_windowRect);
            }
        }

        public void Update()
        {
            bool isLeftClickDown = RuntimeUnityEditorCore.INSTANCE.SettingsData.EnableClickForParentGameObject && Input.GetMouseButtonDown(0);
            bool isRightClickDown = RuntimeUnityEditorCore.INSTANCE.SettingsData.EnableClickForChildGameObject && Input.GetMouseButtonDown(1);

            if (isLeftClickDown || isRightClickDown)
            {
                Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, _clickMask);

                if (isLeftClickDown)
                {
                    SelectAndShowObject(hit.transform);
                }
                else if (isRightClickDown)
                {
                    SelectAndShowObject(hit.collider.transform);
                }
            }
        }

        private void WindowFunc(int id)
        {
            GUILayout.BeginVertical();
            {
                DisplayObjectTree();

                DisplayControls();

                DisplayObjectProperties();

                RuntimeUnityEditorCore.INSTANCE.SettingsViewer.DrawSettingsMenu();
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }

        private void DisplayControls()
        {
            AssetBundleManagerHelper.DrawButtonIfAvailable();
        }

        private void DisplayObjectProperties()
        {
            _propertiesScrollPosition = GUILayout.BeginScrollView(_propertiesScrollPosition, GUI.skin.box);
            {
                if (SelectedTransform == null)
                {
                    GUILayout.Label("No object selected");
                }
                else
                {
                    DrawTransformControls();

                    foreach (var component in SelectedTransform.GetComponents<Component>())
                    {
                        if (component == null)
                            continue;

                        DrawSingleComponent(component);
                    }
                }
            }
            GUILayout.EndScrollView();
        }

        private void DrawTransformControls()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                var fullTransfromPath = SelectedTransform.GetFullTransfromPath();

                GUILayout.TextArea(fullTransfromPath, GUI.skin.label);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label($"Layer {SelectedTransform.gameObject.layer} ({LayerMask.LayerToName(SelectedTransform.gameObject.layer)})");

                    GUILayout.Space(8);

                    GUILayout.Toggle(SelectedTransform.gameObject.isStatic, "isStatic");

                    SelectedTransform.gameObject.SetActive(GUILayout.Toggle(SelectedTransform.gameObject.activeSelf, "Active", GUILayout.ExpandWidth(false)));

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Inspect"))
                        OnInspectorOpen(new InstanceStackEntry(SelectedTransform.gameObject, SelectedTransform.gameObject.name));

                    if (GUILayout.Button("X"))
                        Object.Destroy(SelectedTransform.gameObject);
                }
                GUILayout.EndHorizontal();

                DrawVector3(nameof(Transform.position), vector3 => SelectedTransform.position = vector3, () => SelectedTransform.position, -5, 5);
                DrawVector3(nameof(Transform.localPosition), vector3 => SelectedTransform.localPosition = vector3, () => SelectedTransform.localPosition, -5, 5);
                DrawVector3(nameof(Transform.localScale), vector3 => SelectedTransform.localScale = vector3, () => SelectedTransform.localScale, 0.00001f, 5);
                DrawVector3(nameof(Transform.eulerAngles), vector3 => SelectedTransform.eulerAngles = vector3, () => SelectedTransform.eulerAngles, 0, 360);
                DrawVector3("localEuler", vector3 => SelectedTransform.localEulerAngles = vector3, () => SelectedTransform.localEulerAngles, 0, 360);
            }
            GUILayout.EndVertical();
        }

        private void DrawVector3(string name, Action<Vector3> set, Func<Vector3> get, float minVal, float maxVal)
        {
            var v3 = get();
            var v3New = v3;

            GUI.changed = false;
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(name, GUILayout.ExpandWidth(true), _drawVector3FieldHeight);
                v3New.x = GUILayout.HorizontalSlider(v3.x, minVal, maxVal, _drawVector3SliderWidth, _drawVector3SliderHeight);
                float.TryParse(GUILayout.TextField(v3New.x.ToString("F2", CultureInfo.InvariantCulture), _drawVector3FieldWidth, _drawVector3FieldHeight), NumberStyles.Any, CultureInfo.InvariantCulture, out v3New.x);
                v3New.y = GUILayout.HorizontalSlider(v3.y, minVal, maxVal, _drawVector3SliderWidth, _drawVector3SliderHeight);
                float.TryParse(GUILayout.TextField(v3New.y.ToString("F2", CultureInfo.InvariantCulture), _drawVector3FieldWidth, _drawVector3FieldHeight), NumberStyles.Any, CultureInfo.InvariantCulture, out v3New.y);
                v3New.z = GUILayout.HorizontalSlider(v3.z, minVal, maxVal, _drawVector3SliderWidth, _drawVector3SliderHeight);
                float.TryParse(GUILayout.TextField(v3New.z.ToString("F2", CultureInfo.InvariantCulture), _drawVector3FieldWidth, _drawVector3FieldHeight), NumberStyles.Any, CultureInfo.InvariantCulture, out v3New.z);
            }
            GUILayout.EndHorizontal();

            if (GUI.changed && v3 != v3New) set(v3New);
        }

        private void DrawVector2(string name, Action<Vector2> set, Func<Vector2> get, float minVal, float maxVal)
        {
            var vector2 = get();
            var vector2New = vector2;

            GUI.changed = false;
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(name, GUILayout.ExpandWidth(true), _drawVector3FieldHeight);
                vector2New.x = GUILayout.HorizontalSlider(vector2.x, minVal, maxVal, _drawVector3SliderWidth, _drawVector3SliderHeight);
                float.TryParse(GUILayout.TextField(vector2New.x.ToString("F2", CultureInfo.InvariantCulture), _drawVector3FieldWidth, _drawVector3FieldHeight), NumberStyles.Any, CultureInfo.InvariantCulture, out vector2New.x);
                vector2New.y = GUILayout.HorizontalSlider(vector2.y, minVal, maxVal, _drawVector3SliderWidth, _drawVector3SliderHeight);
                float.TryParse(GUILayout.TextField(vector2New.y.ToString("F2", CultureInfo.InvariantCulture), _drawVector3FieldWidth, _drawVector3FieldHeight), NumberStyles.Any, CultureInfo.InvariantCulture, out vector2New.y);
            }
            GUILayout.EndHorizontal();

            if (GUI.changed && vector2 != vector2New) set(vector2New);
        }

        private void DrawSingleComponent(Component component)
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                if (component is Behaviour bh)
                    bh.enabled = GUILayout.Toggle(bh.enabled, "", GUILayout.ExpandWidth(false));

                if (GUILayout.Button(component.GetType().Name, GUI.skin.label))
                {
                    OnInspectorOpen(new InstanceStackEntry(component.transform, component.transform.name),
                        new InstanceStackEntry(component, component.GetType().FullName));
                }

                switch (component)
                {
                    case Image img:
                        if (img.sprite != null && img.sprite.texture != null)
                        {
                            GUILayout.Label(img.sprite.name);

                            if (!_imagePreviewCache.TryGetValue(img, out var tex))
                            {
                                try
                                {
                                    var newImg = img.sprite.texture.GetPixels(
                                        (int)img.sprite.textureRect.x, (int)img.sprite.textureRect.y,
                                        (int)img.sprite.textureRect.width,
                                        (int)img.sprite.textureRect.height);
                                    tex = new Texture2D((int)img.sprite.textureRect.width,
                                        (int)img.sprite.textureRect.height);
                                    tex.SetPixels(newImg);
                                    //todo tex.Resize(0, 0); get proper width
                                    tex.Apply();
                                }
                                catch (Exception)
                                {
                                    tex = null;
                                }

                                _imagePreviewCache.Add(img, tex);
                            }

                            if (tex != null)
                                GUILayout.Label(tex);
                            else
                                GUILayout.Label("Can't display texture");
                        }
                        //todo img.sprite.texture.EncodeToPNG() button
                        break;
                    case Slider b:
                        {
                            for (var i = 0; i < b.onValueChanged.GetPersistentEventCount(); ++i)
                                GUILayout.Label(ToStringConverter.EventEntryToString(b.onValueChanged, i));
                            break;
                        }
                    case Text text:
                        GUILayout.Label(
                            $"{text.text} {text.font} {text.fontStyle} {text.fontSize} {text.alignment} {text.resizeTextForBestFit} {text.color}");
                        break;
                    case RawImage r:
                        GUILayout.Label(r.mainTexture);
                        break;
                    case Renderer re:
                        GUILayout.Label(re.material != null
                            ? re.material.shader.name
                            : "[No material]");
                        break;
                    case Button b:
                        {
                            var eventObj = b.onClick;
                            for (var i = 0; i < eventObj.GetPersistentEventCount(); ++i)
                                GUILayout.Label(ToStringConverter.EventEntryToString(eventObj, i));

                            var calls = (IList)eventObj.GetPrivateExplicit<UnityEventBase>("m_Calls").GetPrivate("m_RuntimeCalls");
                            foreach (var call in calls)
                                GUILayout.Label(ToStringConverter.ObjectToString(call.GetPrivate("Delegate")));
                            break;
                        }
                    case Toggle b:
                        {
                            var eventObj = b.onValueChanged;
                            for (var i = 0; i < eventObj.GetPersistentEventCount(); ++i)
                                GUILayout.Label(ToStringConverter.EventEntryToString(b.onValueChanged, i));

                            var calls = (IList)b.onValueChanged.GetPrivateExplicit<UnityEventBase>("m_Calls").GetPrivate("m_RuntimeCalls");
                            foreach (var call in calls)
                                GUILayout.Label(ToStringConverter.ObjectToString(call.GetPrivate("Delegate")));
                            break;
                        }
                    case RectTransform rt:
                        GUILayout.BeginVertical();
                        {
                            DrawVector2(nameof(RectTransform.anchorMin), vector2 => rt.anchorMin = vector2, () => rt.anchorMin, 0, 1);
                            DrawVector2(nameof(RectTransform.anchorMax), vector2 => rt.anchorMax = vector2, () => rt.anchorMax, 0, 1);
                            DrawVector2(nameof(RectTransform.offsetMin), vector2 => rt.offsetMin = vector2, () => rt.offsetMin, -1000, 1000);
                            DrawVector2(nameof(RectTransform.offsetMax), vector2 => rt.offsetMax = vector2, () => rt.offsetMax, -1000, 1000);
                            DrawVector2(nameof(RectTransform.sizeDelta), vector2 => rt.sizeDelta = vector2, () => rt.sizeDelta, -1000, 1000);
                            GUILayout.Label("rect " + rt.rect);
                        }
                        GUILayout.EndVertical();
                        break;
                }

                GUILayout.FlexibleSpace();

                if (!(component is Transform))
                {
                    /*if (GUILayout.Button("R"))
                    {
                        var t = component.GetType();
                        var g = component.gameObject;

                        IEnumerator RecreateCo()
                        {
                            Object.Destroy(component);
                            yield return null;
                            g.AddComponent(t);
                        }

                        Object.FindObjectOfType<CheatTools>().StartCoroutine(RecreateCo());
                    }*/

                    if (GUILayout.Button("X"))
                    {
                        Object.Destroy(component);
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private string _searchText = string.Empty;
        private void DisplayObjectTree()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                DisplayTreeSearchBox();

                _treeScrollPosition = GUILayout.BeginScrollView(_treeScrollPosition,
                    GUILayout.Height(_objectTreeHeight), GUILayout.ExpandWidth(true));
                {
                    var currentCount = 0;
                    foreach (var rootGameObject in _gameObjectSearcher.GetSearchedOrAllObjects())
                        DisplayObjectTreeHelper(rootGameObject, 0, ref currentCount);
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
        }

        private void DisplayTreeSearchBox()
        {
            GUILayout.BeginHorizontal();
            {
                GUI.SetNextControlName("searchbox");
                _searchText = GUILayout.TextField(_searchText, GUILayout.ExpandWidth(true));

                if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
                {
                    _searchText = string.Empty;
                    _gameObjectSearcher.Search(_searchText, false);
                    SelectAndShowObject(SelectedTransform);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Search scene"))
                    _gameObjectSearcher.Search(_searchText, false);

                if (Event.current.isKey && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter) && GUI.GetNameOfFocusedControl() == "searchbox")
                {
                    _gameObjectSearcher.Search(_searchText, false);
                    Event.current.Use();
                }

                if (GUILayout.Button("Deep scene"))
                    _gameObjectSearcher.Search(_searchText, true);

                if (GUILayout.Button("Search static"))
                {
                    if (string.IsNullOrEmpty(_searchText))
                    {
                        RuntimeUnityEditorCore.LOGGER.Log(LogLevel.Message | LogLevel.Warning, "Can't search for empty string");
                    }
                    else
                    {
                        var matchedTypes = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(Extensions.GetTypesSafe)
                            .Where(x => x.GetSourceCodeRepresentation().Contains(_searchText, StringComparison.OrdinalIgnoreCase));

                        var stackEntries = matchedTypes.Select(t => new StaticStackEntry(t, t.FullName)).ToList();

                        if (stackEntries.Count == 0)
                            RuntimeUnityEditorCore.LOGGER.Log(LogLevel.Message | LogLevel.Warning, "No static type names contained the search string");
                        else if (stackEntries.Count == 1)
                            RuntimeUnityEditorCore.INSTANCE.Inspector.InspectorPush(stackEntries.Single());
                        else
                            RuntimeUnityEditorCore.INSTANCE.Inspector.InspectorPush(new InstanceStackEntry(stackEntries, "Static type search"));
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}
