using RuntimeUnityEditor.Core.UI;
using System;
using UnityEngine;

namespace RuntimeUnityEditor.Core.PinnedVariables
{
    public class PinnedVariablesViewer : Window
    {
        private PinnedVariablesData _data;

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
                400f, 
                50f
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
                GUILayout.Label($"Count: {_data.GetCount()}");
            }
        }

        internal override void Update()
        {
        }

        internal override bool ShouldBeVisible()
        {
            return RuntimeUnityEditorCore.INSTANCE.Show || _data.GetCount() >= 1;
        }

    }
}
