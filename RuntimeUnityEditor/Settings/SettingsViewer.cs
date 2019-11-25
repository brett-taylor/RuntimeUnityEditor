using UnityEngine;

namespace RuntimeUnityEditor.Core.Settings
{
    public class SettingsViewer
    {
        private Settings _settings;
        private bool _expanded = false;
        private readonly GUILayoutOption _collapseExpandButtonOptions = GUILayout.Width(70);

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
        }

        private void DrawClickForGameObjectBehaviour()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                GUILayout.BeginHorizontal();
                {
                    _settings.EnableClickForParentGameObject = GUILayout.Toggle(_settings.EnableClickForParentGameObject, "Left click for parent GameObject");
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    _settings.EnableClickForChildGameObject = GUILayout.Toggle(_settings.EnableClickForChildGameObject, "Right click for child GameObject");
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
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }
    }
}
