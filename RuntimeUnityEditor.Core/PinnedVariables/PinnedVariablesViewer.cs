using Mono.CSharp;
using RuntimeUnityEditor.Core.Inspector;
using RuntimeUnityEditor.Core.Inspector.Entries;
using RuntimeUnityEditor.Core.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuntimeUnityEditor.Core.PinnedVariables
{
    public class PinnedVariablesViewer : Window
    {
        private const float WIDTH_WHEN_EDITOR_HIDDEN = 350f;
        private const float WIDTH_WHEN_EDITOR_VISIBLE = WIDTH_WHEN_EDITOR_HIDDEN + UNPIN_BUTTON_WIDTH;
        private const float UNPIN_BUTTON_WIDTH = 50f;
        private static readonly Color COMPACT_MODE_BACKGROUND_COLOR = new Color(255, 255, 255, 0.7f);
        private static readonly Color COMPACT_MODE_FOREGROUND_COLOR = new Color(255, 255, 255, 0.7f);

        private PinnedVariablesData _data;
        private readonly GUILayoutOption _nameWidth = GUILayout.Width(170);
        private readonly GUILayoutOption _unPinWidth = GUILayout.Width(UNPIN_BUTTON_WIDTH);
        private Color _originalGUIBackgroundColor;
        private Color _originalGUIForegorundColor;

        protected override bool ShouldEatInput => ShouldBeVisible();
        protected override bool IsWindowDraggable => false;
        protected override string WindowTitle => "Pinned Variables";
        protected override bool UpdateWindowSizeEveryFrame => true;
        internal override WindowState RenderOnlyInWindowState => WindowState.ALL;
        internal override WindowState UpdateOnlyInWindowState => WindowState.ALL;

        public PinnedVariablesViewer(PinnedVariablesData pinnedVariablesData)
        {
            _data = pinnedVariablesData;
        }

        protected override bool PreCreatedWindow()
        {
            _originalGUIBackgroundColor = GUI.backgroundColor;
            _originalGUIForegorundColor = GUI.color;
            if (IsInCompactMode())
            {
                GUI.backgroundColor = COMPACT_MODE_BACKGROUND_COLOR;
                GUI.color = COMPACT_MODE_FOREGROUND_COLOR;
            }

            return ShouldBeVisible();
        }

        protected override void PostCreatedWindow()
        {
            GUI.backgroundColor = _originalGUIBackgroundColor;
            GUI.color = _originalGUIForegorundColor;
        }

        internal override Rect GetStartingRect(Rect screenSize, float centerWidth, float centerX)
        {
            return new Rect(
                PADDING,
                PADDING,
                IsEditorOpen() ? WIDTH_WHEN_EDITOR_VISIBLE : WIDTH_WHEN_EDITOR_HIDDEN,
                1f
            );
        }

        protected override void DrawWindowContents()
        {
            if (_data.GetCount() == 0)
            {
                GUILayout.Label("You don't have any pinned variables, why not pin some?");
            }
            else
            {
                DrawTableHeader();
                foreach (var i in _data)
                {
                    GUILayout.BeginHorizontal();
                    {
                        DrawRow(i.Key, i.Value);
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }

        internal override void Update()
        {
        }

        private bool ShouldBeVisible()
        {
            return IsEditorOpen() || _data.GetCount() >= 1;
        }

        private void DrawTableHeader()
        {
            if (IsInCompactMode() == true)
                return;

            GUILayout.BeginHorizontal();
            {
                //GUILayout.Space(1);
                GUILayout.Label("Name", GUI.skin.box, _nameWidth);
                //GUILayout.Space(1);
                GUILayout.Label("Value", GUI.skin.box, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();
        }

        private void DrawRow(ICacheEntry entry, PinnedVariable pinnedVariable)
        {
            if (IsEditorOpen())
            {
                pinnedVariable.Name = GUILayout.TextField(pinnedVariable.Name, GUI.skin.textField, _nameWidth);
            }
            else
            {
                GUILayout.TextArea(pinnedVariable.Name, GUI.skin.label, _nameWidth);
            }

            GUILayout.TextArea(ToStringConverter.ObjectToString(entry.GetValue()), GUI.skin.label, GUILayout.ExpandWidth(true));
            if (IsEditorOpen() && GUILayout.Button("Unpin", _unPinWidth))
                RuntimeUnityEditorCore.INSTANCE.PinnedVariablesData.Untrack(entry);
        }

        private bool IsEditorOpen()
        {
            return RuntimeUnityEditorCore.INSTANCE.Show;
        }

        private bool IsInCompactMode()
        {
            return RuntimeUnityEditorCore.INSTANCE.SettingsData.PinnedVariablesCompactMode && IsEditorOpen() == false && _data.GetCount() >= 1;
        }
    }
}
