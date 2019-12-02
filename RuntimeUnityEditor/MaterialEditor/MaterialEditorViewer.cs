using RuntimeUnityEditor.Core.UI;
using UnityEngine;

namespace RuntimeUnityEditor.Core.MaterialEditor
{
    public class MaterialEditorViewer : Window
    {
        private const float SHADER_EDITOR_WIDTH = 350f;

        private readonly MaterialEditorData _data;

        protected override bool ShouldEatInput => true;
        protected override bool IsWindowDraggable => true;
        protected override string WindowTitle => "Material Editor";
        internal override WindowState RenderOnlyInWindowState => WindowState.ALL;
        internal override WindowState UpdateOnlyInWindowState => WindowState.ALL;

        public MaterialEditorViewer(MaterialEditorData data)
        {
            _data = data;
        }

        internal override Rect GetStartingRect(Rect screenSize, float centerWidth, float centerX)
        {
            float height = screenSize.height / 2;
            return new Rect(
                x: PADDING, 
                y: centerX - (height / 2), 
                width: SHADER_EDITOR_WIDTH, 
                height: height
            );
        }

        protected override bool PreCreatedWindow()
        {
            return true;
        }

        protected override void PostCreatedWindow()
        {
        }

        protected override void DrawWindowContents()
        {
        }

        internal override void Update()
        {
        }
    }
}
