using System;
using System.IO;
using RuntimeUnityEditor.Core.Gizmos;
using RuntimeUnityEditor.Core.ObjectTree;
using RuntimeUnityEditor.Core.REPL;
using RuntimeUnityEditor.Core.UI;
using RuntimeUnityEditor.Core.Utils;
using UnityEngine;

namespace RuntimeUnityEditor.Core
{
    public class RuntimeUnityEditorCore : MonoBehaviour
    {
        public static RuntimeUnityEditorCore INSTANCE { get; private set; }
        internal static KeyCode SHOW_HOT_KEY { get; set; } = KeyCode.F7;
        internal static ILoggerWrapper LOGGER { get; private set; }

        public Inspector.Inspector Inspector { get; private set; }
        public ObjectTreeViewer TreeViewer { get; private set; }
        public ReplWindow Repl { get; private set; }

        private GizmoDrawer _gizmoDrawer;
        private GameObjectSearcher _gameObjectSearcher = new GameObjectSearcher();
        private CursorLockMode _previousCursorLockState;
        private bool _previousCursorVisible;

        public void Setup(ILoggerWrapper logger, string configPath)
        {
            INSTANCE = this;
            LOGGER = logger;
            TreeViewer = new ObjectTreeViewer(this, _gameObjectSearcher)
            {
                InspectorOpenCallback = items =>
                {
                    Inspector.InspectorClear();
                    foreach (var stackEntry in items)
                        Inspector.InspectorPush(stackEntry);
                }
            };

            if (UnityFeatureHelper.SupportsVectrosity)
            {
                _gizmoDrawer = new GizmoDrawer(this);
                TreeViewer.TreeSelectionChangedCallback = transform => _gizmoDrawer.UpdateState(transform);
            }

            if (UnityFeatureHelper.SupportsCursorIndex &&
                UnityFeatureHelper.SupportsXml)
            {
                try
                {
                    Repl = new ReplWindow(Path.Combine(configPath, "RuntimeUnityEditor.Autostart.cs"));
                    Repl.RunAutostart();
                }
                catch (Exception ex)
                {
                    LOGGER.Log(LogLevel.Warning, "Failed to load REPL - " + ex.Message);
                }
            }

            Inspector = new Inspector.Inspector(targetTransform => TreeViewer.SelectAndShowObject(targetTransform), Repl);
        }

        internal void OnGUI()
        {
            if (Show)
            {
                var originalSkin = GUI.skin;
                GUI.skin = InterfaceMaker.CustomSkin;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                Inspector.DisplayInspector();
                TreeViewer.DisplayViewer();
                Repl?.DisplayWindow();

                // Restore old skin for maximum compatibility
                GUI.skin = originalSkin;
            }
        }

        public bool Show
        {
            get => TreeViewer.Enabled;
            set
            {
                if (Show != value)
                {
                    if (value)
                    {
                        _previousCursorLockState = Cursor.lockState;
                        _previousCursorVisible = Cursor.visible;
                    }
                    else
                    {
                        if (!_previousCursorVisible || _previousCursorLockState != CursorLockMode.None)
                        {
                            Cursor.lockState = _previousCursorLockState;
                            Cursor.visible = _previousCursorVisible;
                        }
                    }
                }

                TreeViewer.Enabled = value;

                if (_gizmoDrawer != null)
                {
                    _gizmoDrawer.Show = value;
                    _gizmoDrawer.UpdateState(TreeViewer.SelectedTransform);
                }

                if (value)
                {
                    SetWindowSizes();

                    RefreshGameObjectSearcher(true);
                }
            }
        }

        internal void Update()
        {
            if (Input.GetKeyDown(SHOW_HOT_KEY))
                Show = !Show;

            if (Show)
            {
                Inspector.InspectorUpdate();
                RefreshGameObjectSearcher(false);

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        internal void LateUpdate()
        {
            if (Show)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void RefreshGameObjectSearcher(bool full)
        {
            bool GizmoFilter(GameObject o) => o.name.StartsWith(GizmoDrawer.GizmoObjectName);
            var gizmosExist = _gizmoDrawer != null && _gizmoDrawer.Lines.Count > 0;
            _gameObjectSearcher.Refresh(full, gizmosExist ? GizmoFilter : (Predicate<GameObject>)null);
        }

        private void SetWindowSizes()
        {
            const int screenOffset = 10;

            var screenRect = new Rect(
                screenOffset,
                screenOffset,
                Screen.width - screenOffset * 2,
                Screen.height - screenOffset * 2);

            var centerWidth = (int)Mathf.Min(850, screenRect.width);
            var centerX = (int)(screenRect.xMin + screenRect.width / 2 - Mathf.RoundToInt((float)centerWidth / 2));

            var inspectorHeight = (int)(screenRect.height / 4) * 3;
            Inspector.UpdateWindowSize(new Rect(
                centerX,
                screenRect.yMin,
                centerWidth,
                inspectorHeight));

            var rightWidth = 350;
            var treeViewHeight = screenRect.height;
            TreeViewer.UpdateWindowSize(new Rect(
                screenRect.xMax - rightWidth,
                screenRect.yMin,
                rightWidth,
                treeViewHeight));

            var replPadding = 8;
            Repl?.UpdateWindowSize(new Rect(
                centerX,
                screenRect.yMin + inspectorHeight + replPadding,
                centerWidth,
                screenRect.height - inspectorHeight - replPadding));
        }
    }
}
