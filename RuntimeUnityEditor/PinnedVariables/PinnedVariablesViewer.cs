using RuntimeUnityEditor.Core.UI;
using System;
using UnityEngine;

namespace RuntimeUnityEditor.Core.PinnedVariables
{
    public class PinnedVariablesViewer : Window
    {
        protected override bool ShouldEatInput => true;
        protected override bool IsWindowDraggable => true;
        protected override string WindowTitle => "Pinned Variables";
        internal override WindowState RenderOnlyInWindowState => WindowState.ALL;
        internal override WindowState UpdateOnlyInWindowState => WindowState.ALL;

        protected override bool PreCreatedWindow()
        {
            return true;
        }

        protected override void PostCreatedWindow()
        {
        }

        internal override Rect GetStartingRect(Rect screenSize, float centerWidth, float centerX)
        {
            return new Rect(RuntimeUnityEditorCore.SCREEN_OFFSET, RuntimeUnityEditorCore.SCREEN_OFFSET, 300f, 50f);
        }

        protected override void DrawWindowContents()
        {
            GUILayout.Label("You don't have any pinned variables, why not pin some?");   
        }

        internal override void Update()
        {
        }
    }
}
