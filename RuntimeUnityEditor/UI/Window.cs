using UnityEngine;

namespace RuntimeUnityEditor.Core.UI
{
    public abstract class Window
    {
        protected Rect _windowRect;
        private readonly int _windowID;
        private bool _hasRectBeenSet = false;

        internal abstract WindowState RenderOnlyInWindowState { get; }
        internal abstract WindowState UpdateOnlyInWindowState{ get; }
        protected abstract bool ShouldEatInput { get; }
        protected abstract bool IsWindowDraggable { get; }
        protected abstract string WindowTitle { get; }

        internal abstract void Update();
        protected abstract void PostCreatedWindow();
        protected abstract void DrawWindowContents();
        protected abstract Rect GetStartingRect(Rect screenSize);

        public Window()
        {
            _windowID = GetHashCode();
        }

        internal void RenderWindow()
        {
            _windowRect = GUILayout.Window(_windowID, _windowRect, WindowFunc, WindowTitle);

            if (ShouldEatInput)
            {
                InterfaceMaker.EatInputInRect(_windowRect);
            }

            PostCreatedWindow();
        }

        internal void WindowFunc(int id)
        {
            GUILayout.BeginVertical();
            {
                DrawWindowContents();
            }
            GUILayout.EndHorizontal();

            if (IsWindowDraggable)
                GUI.DragWindow();
        }

        internal void UpdateWindowSize(Rect screenSize)
        {
            if (_hasRectBeenSet == false)
            {
                _windowRect = GetStartingRect(screenSize);
                _hasRectBeenSet = true;
            }
        }
    }
}
