﻿using Mono.CSharp;
using RuntimeUnityEditor.Core.Inspector;
using RuntimeUnityEditor.Core.Inspector.Entries;
using RuntimeUnityEditor.Core.UI;
using System;
using UnityEngine;

namespace RuntimeUnityEditor.Core.PinnedVariables
{
    public class PinnedVariablesViewer : Window
    {
        private const float HEIGHT_WHEN_NOTIHNG_PINNED = 40f;
        private const float BASE_HEIGHT_WITH_PINNED = 60f;
        private const float WIDTH_WHEN_EDITOR_HIDDEN = 350f;
        private const float WIDTH_WHEN_EDITOR_VISIBLE = WIDTH_WHEN_EDITOR_HIDDEN + UNPIN_BUTTON_WIDTH;
        private const float UNPIN_BUTTON_WIDTH = 50f;

        private PinnedVariablesData _data;
        private readonly GUILayoutOption _nameWidth = GUILayout.Width(170);
        private readonly GUILayoutOption _unPinWidth = GUILayout.Width(UNPIN_BUTTON_WIDTH);

        protected override bool ShouldEatInput => ShouldBeVisible();
        protected override bool IsWindowDraggable => true;
        protected override string WindowTitle => "Pinned Variables";
        protected override bool UpdateWindowSizeEveryFrame => true;
        internal override WindowState RenderOnlyInWindowState => WindowState.CONDITIONAL;
        internal override WindowState UpdateOnlyInWindowState => WindowState.ALL;

        public PinnedVariablesViewer(PinnedVariablesData pinnedVariablesData)
        {
            _data = pinnedVariablesData;
        }

        protected override bool PreCreatedWindow()
        {
            return true;
        }

        protected override void PostCreatedWindow()
        {
        }

        internal override Rect GetStartingRect(Rect screenSize, float centerWidth, float centerX)
        {
            return new Rect(
                RuntimeUnityEditorCore.SCREEN_OFFSET,
                RuntimeUnityEditorCore.SCREEN_OFFSET,
                RuntimeUnityEditorCore.INSTANCE.Show ? WIDTH_WHEN_EDITOR_VISIBLE : WIDTH_WHEN_EDITOR_HIDDEN,
                _data.GetCount() == 0 ? HEIGHT_WHEN_NOTIHNG_PINNED : BASE_HEIGHT_WITH_PINNED
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
                foreach (Tuple<string, ICacheEntry> tuple in _data)
                {
                    GUILayout.BeginHorizontal();
                    {
                        DrawRow(tuple);
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }

        internal override void Update()
        {
        }

        internal override bool ShouldBeVisible()
        {
            return RuntimeUnityEditorCore.INSTANCE.Show || _data.GetCount() >= 1;
        }

        private void DrawTableHeader()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(1);
                GUILayout.Label("Name", GUI.skin.box, _nameWidth);
                GUILayout.Space(1);
                GUILayout.Label("Value", GUI.skin.box, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();
        }

        private void DrawRow(Tuple<string, ICacheEntry> tuple)
        {
            GUILayout.TextArea(tuple.Item1, GUI.skin.label, _nameWidth);
            GUILayout.TextArea(ToStringConverter.ObjectToString(tuple.Item2.GetValue()), GUI.skin.label, GUILayout.ExpandWidth(true));
            if (RuntimeUnityEditorCore.INSTANCE.Show && GUILayout.Button("Unpin", _unPinWidth))
                RuntimeUnityEditorCore.INSTANCE.PinnedVariablesData.Untrack(tuple);
        }
    }
}
