using System;
using RuntimeUnityEditor.Core.UI;
using UnityEngine;

namespace RuntimeUnityEditor.Core.Popup.Dialogs
{
    public class Dialog : Popup
    {
        private static readonly float DIALOG_WIDTH = 400f;
        private static readonly float DIALOG_DEFAULT_HEIGHT = 150f;
        
        internal override WindowState RenderOnlyInWindowState => WindowState.VISIBLE;
        internal override WindowState UpdateOnlyInWindowState => WindowState.VISIBLE;
        protected override bool ShouldEatInput => true;
        protected override bool IsWindowDraggable => true;
        protected override string WindowTitle { get; }
        private readonly Action drawGUI;

        protected override bool PreCreatedWindow() => true;
        protected override void PostCreatedWindow() { }
        internal override void Update() { }

        public Dialog(string windowTitle, Action drawGUI)
        {
            WindowTitle = windowTitle;
            this.drawGUI = drawGUI;
            
            RegisterNewAction(DialogButton.OK.Text, () => DialogButton.OK.Action.Invoke(this));
        }
        
        internal override Rect GetStartingRect(Rect screenSize, float centerWidth, float centerX)
        {
            return new Rect(
                (screenSize.width / 2) - (DIALOG_WIDTH / 2), 
                (screenSize.height / 2) - (DIALOG_DEFAULT_HEIGHT / 2), 
                DIALOG_WIDTH, 
                DIALOG_DEFAULT_HEIGHT
            );
        }

        protected override void DrawPopupContents()
        {
            drawGUI.Invoke();
        }
    }
}