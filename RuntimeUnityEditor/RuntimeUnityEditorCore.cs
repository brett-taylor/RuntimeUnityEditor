using System;
using System.Collections.Generic;
using System.IO;
using RuntimeUnityEditor.Core.Gizmos;
using RuntimeUnityEditor.Core.ObjectTree;
using RuntimeUnityEditor.Core.REPL;
using RuntimeUnityEditor.Core.Settings;
using RuntimeUnityEditor.Core.UI;
using RuntimeUnityEditor.Core.Utils;
using UnityEngine;

namespace RuntimeUnityEditor.Core
{
    public class RuntimeUnityEditorCore : MonoBehaviour
    {
        public static RuntimeUnityEditorCore INSTANCE { get; private set; }
        internal static KeyCode SHOW_HOT_KEY => KeyCode.F7;
        internal static ILoggerWrapper LOGGER { get; private set; }
        internal static float SCREEN_OFFSET = 10f;

        public Inspector.Inspector Inspector { get; private set; }
        public ObjectTreeViewer TreeViewer { get; private set; }
        public ReplWindow Repl { get; private set; }
        public SettingsData SettingsData { get; private set; }
        public SettingsViewer SettingsViewer { get; private set; }

        private GizmoDrawer _gizmoDrawer;
        private GameObjectSearcher _gameObjectSearcher = new GameObjectSearcher();
        private CursorLockMode _previousCursorLockState;
        private bool _previousCursorVisible;
        private readonly List<IWindow> windows = new List<IWindow>();

        public void Setup(ILoggerWrapper logger, string configPath)
        {
            INSTANCE = this;
            LOGGER = logger;

            SettingsData = SettingsManager.LoadOrCreate();
            DnSpyHelper.SetPath(SettingsData.DNSpyPath, false);

            SettingsViewer = new SettingsViewer();
            TreeViewer = new ObjectTreeViewer(this, _gameObjectSearcher)
            {
                InspectorOpenCallback = items =>
                {
                    Inspector.InspectorClear();
                    foreach (var stackEntry in items)
                        Inspector.InspectorPush(stackEntry);
                }
            };

            _gizmoDrawer = new GizmoDrawer(this);
            TreeViewer.TreeSelectionChangedCallback = transform => _gizmoDrawer.UpdateState(transform);
            windows.Add(TreeViewer);

            if (UnityFeatureHelper.SupportsCursorIndex && UnityFeatureHelper.SupportsXml)
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
            var originalSkin = GUI.skin;
            GUI.skin = InterfaceMaker.CustomSkin;
            ShowCursorIfVisible();

            foreach (IWindow window in windows)
            {
                if (IsInCorrectState(window.RenderOnlyInWindowState))
                    window.RenderWindow();
            }

            GUI.skin = originalSkin;
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
                    RefreshGameObjectSearcher(true);
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(SHOW_HOT_KEY))
                Show = !Show;

            ShowCursorIfVisible();

            var screenRect = new Rect(
                SCREEN_OFFSET,
                SCREEN_OFFSET,
                Screen.width - SCREEN_OFFSET * 2,
                Screen.height - SCREEN_OFFSET * 2
            );

            foreach (IWindow window in windows)
            {
                if (IsInCorrectState(window.RenderOnlyInWindowState))
                    window.UpdateWindowSize(screenRect);

                if (IsInCorrectState(window.UpdateOnlyInWindowState))
                    window.Update();
            }
        }

        private void LateUpdate()
        {
            ShowCursorIfVisible();
        }

        private void RefreshGameObjectSearcher(bool full)
        {
            bool GizmoFilter(GameObject o) => o.name.StartsWith(GizmoDrawer.GizmoObjectName);
            var gizmosExist = _gizmoDrawer != null && _gizmoDrawer.Lines.Count > 0;
            _gameObjectSearcher.Refresh(full, gizmosExist ? GizmoFilter : (Predicate<GameObject>)null);
        }

        private bool IsInCorrectState(WindowState windowState)
        {
            if (windowState == WindowState.ALL)
                return true;

            if (windowState == WindowState.HIDDEN && Show == false)
                return true;

            if (windowState == WindowState.VISIBLE && Show == true)
                return true;

            return false;
        }

        private void ShowCursorIfVisible()
        {
            if (Show)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
