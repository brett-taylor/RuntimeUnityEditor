using UnityEngine;

namespace RuntimeUnityEditor.Core.UI
{
    internal interface IWindow
    {
        bool VisibleOnlyWhenEditorActive { get; set; }
        void UpdateWindowSize(Rect screenSize);
        void DisplayWindow();
    }
}
