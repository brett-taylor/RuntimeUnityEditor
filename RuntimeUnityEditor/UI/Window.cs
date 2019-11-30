using UnityEngine;

namespace RuntimeUnityEditor.Core.UI
{
    public abstract class Window
    {
        protected Rect _windowRect;
        protected readonly int _windowID;
        private bool _hasRectBeenSet = false;

        internal abstract WindowState RenderOnlyInWindowState { get; }
        internal abstract WindowState UpdateOnlyInWindowState{ get; }
        protected abstract bool ShouldEatInput { get; }
        protected abstract bool IsWindowDraggable { get; }
        protected abstract string WindowTitle { get; }

        internal abstract void Update();
        protected abstract bool PreCreatedWindow();
        protected abstract void PostCreatedWindow();
        protected abstract void DrawWindowContents();
        internal abstract Rect GetStartingRect(Rect screenSize, float centerWidth, float centerX);

        public Window()
        {
            _windowID = GetHashCode();
        }

        internal void RenderWindow()
        {
            if (PreCreatedWindow())
            {
                _windowRect = GUILayout.Window(_windowID, _windowRect, WindowFunc, WindowTitle);

                if (ShouldEatInput)
                {
                    InterfaceMaker.EatInputInRect(_windowRect);
                }
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
                var centerWidth = (int) Mathf.Min(850, screenSize.width);
                var centerX = (int) (screenSize.xMin + screenSize.width / 2 - Mathf.RoundToInt((float) centerWidth / 2));

                _windowRect = GetStartingRect(screenSize, centerWidth, centerX);
                _hasRectBeenSet = true;
            }
        }

        internal virtual bool ShouldBeVisible()
        {
            return true;
        }

        internal virtual bool ShouldUpdate()
        {
            return true;
        }
    }
}
