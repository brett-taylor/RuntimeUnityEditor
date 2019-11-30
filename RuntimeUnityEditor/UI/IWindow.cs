using UnityEngine;

namespace RuntimeUnityEditor.Core.UI
{
    internal interface IWindow
    {
        WindowState RenderOnlyInWindowState { get; }
        WindowState UpdateOnlyInWindowState{ get; }

        void UpdateWindowSize(Rect screenSize);
        void RenderWindow();
        void Update();
    }
}
