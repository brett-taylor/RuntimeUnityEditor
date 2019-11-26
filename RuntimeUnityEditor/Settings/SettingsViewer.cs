using RuntimeUnityEditor.Core.Utils;
using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace RuntimeUnityEditor.Core.Settings
{
    public class SettingsViewer
    {
        private bool _expanded = false;
        private readonly GUILayoutOption _collapseExpandButtonOptions = GUILayout.Width(70);
        private readonly GUILayoutOption _setTimeBoxOptions = GUILayout.Width(38);
        private readonly GUILayoutOption _saveDNSpyPathOptions = GUILayout.Width(70);

        public void DrawSettingsMenu()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                DrawHeader();
                if (_expanded == true)
                {
                    DrawExpanded();
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Settings");

                var buttonText = _expanded == true ? "Collapse" : "Expand";
                if (GUILayout.Button(buttonText, _collapseExpandButtonOptions))
                    _expanded = !_expanded;
            }
            GUILayout.EndHorizontal();
        }

        private void DrawExpanded()
        {
            DrawMiscSettings();
            DrawGizmosSettings();
            DrawClickForGameObjectBehaviour();
            DrawDNSpySettings();
        }

        private void DrawClickForGameObjectBehaviour()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                GUILayout.BeginHorizontal();
                {
                    RuntimeUnityEditorCore.INSTANCE.Settings.EnableClickForParentGameObject = GUILayout.Toggle(RuntimeUnityEditorCore.INSTANCE.Settings.EnableClickForParentGameObject, "Left click for parent game object");
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    RuntimeUnityEditorCore.INSTANCE.Settings.EnableClickForChildGameObject = GUILayout.Toggle(RuntimeUnityEditorCore.INSTANCE.Settings.EnableClickForChildGameObject, "Right click for child game object");
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void DrawGizmosSettings()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                RuntimeUnityEditorCore.INSTANCE.Settings.ShowGizmos = GUILayout.Toggle(RuntimeUnityEditorCore.INSTANCE.Settings.ShowGizmos, "Show gizmos for selection");
                RuntimeUnityEditorCore.INSTANCE.Settings.ShowGizmosOutsideEditor = GUILayout.Toggle(RuntimeUnityEditorCore.INSTANCE.Settings.ShowGizmosOutsideEditor, "Show gizmos outside editor");
            }
            GUILayout.EndVertical();
        }

        private void DrawMiscSettings()
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                if (RuntimeUnityEditorCore.INSTANCE.TreeViewer.SelectedTransform == null)
                    GUI.enabled = false;

                if (GUILayout.Button("Dump", GUILayout.ExpandWidth(false)))
                    SceneDumper.DumpObjects(RuntimeUnityEditorCore.INSTANCE.TreeViewer.SelectedTransform?.gameObject);

                GUI.enabled = true;

                if (GUILayout.Button("Log", GUILayout.ExpandWidth(false)))
                    UnityFeatureHelper.OpenLog();

                GUILayout.FlexibleSpace();

                GUILayout.Label("Time", GUILayout.ExpandWidth(false));

                if (GUILayout.Button(">", GUILayout.ExpandWidth(false)))
                    Time.timeScale = 1;
                if (GUILayout.Button("||", GUILayout.ExpandWidth(false)))
                    Time.timeScale = 0;

                if (float.TryParse(GUILayout.TextField(Time.timeScale.ToString("F2", CultureInfo.InvariantCulture), _setTimeBoxOptions), NumberStyles.Any, CultureInfo.InvariantCulture, out var newVal))
                    Time.timeScale = newVal;

                GUILayout.FlexibleSpace();

                RuntimeUnityEditorCore.INSTANCE.Settings.Wireframe = GUILayout.Toggle(RuntimeUnityEditorCore.INSTANCE.Settings.Wireframe, "Wireframe");
            }
            GUILayout.EndHorizontal();
        }

        private void DrawDNSpySettings()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                GUILayout.Label("DNSpy Path");
                GUILayout.BeginHorizontal();
                {
                    RuntimeUnityEditorCore.INSTANCE.Settings.DNSpyPath = GUILayout.TextField(RuntimeUnityEditorCore.INSTANCE.Settings.DNSpyPath);
                    if (GUILayout.Button("Save", _saveDNSpyPathOptions))
                        DnSpyHelper.DnSpyPath = RuntimeUnityEditorCore.INSTANCE.Settings.DNSpyPath;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
    }
}
