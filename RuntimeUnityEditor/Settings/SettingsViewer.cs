using RuntimeUnityEditor.Core.Utils;
using System.Globalization;
using UnityEngine;

namespace RuntimeUnityEditor.Core.Settings
{
    public class SettingsViewer
    {
        private bool _expanded = false;
        private readonly GUILayoutOption _collapseExpandButtonOptions = GUILayout.Width(70);
        private readonly GUILayoutOption _saveSettingsButtonOptions = GUILayout.Width(120);
        private readonly GUILayoutOption _setTimeBoxOptions = GUILayout.Width(38);
        private readonly GUILayoutOption _saveDNSpyPathOptions = GUILayout.Width(35);

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

                GUILayout.FlexibleSpace();

                if (_expanded)
                {
                    if (GUILayout.Button("Save Settings", _saveSettingsButtonOptions))
                        SettingsManager.Save(RuntimeUnityEditorCore.INSTANCE.SettingsData);
                }

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
            DrawPinnedVariablesSettings();
        }

        private void DrawClickForGameObjectBehaviour()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                GUILayout.BeginHorizontal();
                {
                    RuntimeUnityEditorCore.INSTANCE.SettingsData.EnableClickForParentGameObject = GUILayout.Toggle(RuntimeUnityEditorCore.INSTANCE.SettingsData.EnableClickForParentGameObject, "Left click for parent game object");
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    RuntimeUnityEditorCore.INSTANCE.SettingsData.EnableClickForChildGameObject = GUILayout.Toggle(RuntimeUnityEditorCore.INSTANCE.SettingsData.EnableClickForChildGameObject, "Right click for child game object");
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void DrawGizmosSettings()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                RuntimeUnityEditorCore.INSTANCE.SettingsData.ShowGizmos = GUILayout.Toggle(RuntimeUnityEditorCore.INSTANCE.SettingsData.ShowGizmos, "Show gizmos for selection");
                RuntimeUnityEditorCore.INSTANCE.SettingsData.ShowGizmosOutsideEditor = GUILayout.Toggle(RuntimeUnityEditorCore.INSTANCE.SettingsData.ShowGizmosOutsideEditor, "Show gizmos outside editor");
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

                RuntimeUnityEditorCore.INSTANCE.SettingsData.Wireframe = GUILayout.Toggle(RuntimeUnityEditorCore.INSTANCE.SettingsData.Wireframe, "Wireframe");
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
                    RuntimeUnityEditorCore.INSTANCE.SettingsData.DNSpyPath = GUILayout.TextField(RuntimeUnityEditorCore.INSTANCE.SettingsData.DNSpyPath);
                    if (GUILayout.Button("Set", _saveDNSpyPathOptions))
                        DnSpyHelper.SetPath(RuntimeUnityEditorCore.INSTANCE.SettingsData.DNSpyPath);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void DrawPinnedVariablesSettings()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                RuntimeUnityEditorCore.INSTANCE.SettingsData.PinnedVariablesCompactMode = GUILayout.Toggle(RuntimeUnityEditorCore.INSTANCE.SettingsData.PinnedVariablesCompactMode, "Enable Pinned Variables' Compact Mode");
            }
            GUILayout.EndVertical();
        }
    }
}
