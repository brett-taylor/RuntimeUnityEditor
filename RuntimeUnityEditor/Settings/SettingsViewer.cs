using System.Globalization;
using UnityEngine;

namespace RuntimeUnityEditor.Core.Settings
{
    public class SettingsViewer
    {
        private Settings _settings;
        private bool _expanded = false;

        private readonly GUILayoutOption _collapseExpandButtonOptions = GUILayout.Width(70);
        private readonly GUILayoutOption _setTimeBox = GUILayout.Width(38);

        public SettingsViewer(Settings settings)
        {
            _settings = settings;
        }

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
            DrawClickForGameObjectBehaviour();
            DrawGizmosSettings();
            DrawTimeSettings();
            DrawWireFrame();
        }

        private void DrawClickForGameObjectBehaviour()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                GUILayout.BeginHorizontal();
                {
                    _settings.EnableClickForParentGameObject = GUILayout.Toggle(_settings.EnableClickForParentGameObject, "Left click for parent game object");
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    _settings.EnableClickForChildGameObject = GUILayout.Toggle(_settings.EnableClickForChildGameObject, "Right click for child game object");
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void DrawGizmosSettings()
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                _settings.ShowGizmos = GUILayout.Toggle(_settings.ShowGizmos, "Show gizmos for selection");
                _settings.ShowGizmosOutsideEditor = GUILayout.Toggle(_settings.ShowGizmosOutsideEditor, "Always show");
            }
            GUILayout.EndHorizontal();
        }

        private void DrawTimeSettings()
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUILayout.Label("Time", GUILayout.ExpandWidth(false));

                if (GUILayout.Button(">", GUILayout.ExpandWidth(false)))
                    Time.timeScale = 1;
                if (GUILayout.Button("||", GUILayout.ExpandWidth(false)))
                    Time.timeScale = 0;

                if (float.TryParse(GUILayout.TextField(Time.timeScale.ToString("F2", CultureInfo.InvariantCulture), _setTimeBox), NumberStyles.Any, CultureInfo.InvariantCulture, out var newVal))
                    Time.timeScale = newVal;
            }
            GUILayout.EndHorizontal();
        }

        private void DrawWireFrame()
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                _settings.Wireframe = GUILayout.Toggle(_settings.Wireframe, "Wireframe");
            }
            GUILayout.EndHorizontal();
        }
    }
}
