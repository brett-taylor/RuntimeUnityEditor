using System;

namespace RuntimeUnityEditor.Core.Popup.Dialogs
{
    public class DialogButton
    {
        public static readonly DialogButton OK = new DialogButton("OK", dialog => dialog.Close());
        
        public string Text { get; }
        public Action<Dialog> Action { get; }

        public DialogButton(string text, Action<Dialog> action)
        {
            Text = text;
            Action = action;
        }
    }
}