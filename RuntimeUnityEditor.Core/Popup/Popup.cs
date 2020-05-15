using System;
using System.Collections.Generic;
using RuntimeUnityEditor.Core.UI;
using UnityEngine;

namespace RuntimeUnityEditor.Core.Popup
{
    public abstract class Popup : Window
    {
        private readonly GUILayoutOption actionButtonWidth = GUILayout.Width(100f);
        private readonly Dictionary<string, Action> actions = new Dictionary<string, Action>();

        protected Popup()
        {
            RuntimeUnityEditorCore.INSTANCE.RegisterWindow(this);
        }

        public void Close()
        {
            RuntimeUnityEditorCore.INSTANCE.UnregisterWindow(this);
        }

        protected abstract void DrawPopupContents();
        protected sealed override void DrawWindowContents()
        {
            DrawPopupContents();
            GUILayout.FlexibleSpace();
            DrawActionBar();
        }

        private void DrawActionBar()
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            {
                GUILayout.FlexibleSpace();

                foreach(var kvp in actions)
                {
                    if (GUILayout.Button(kvp.Key, actionButtonWidth))
                        kvp.Value.Invoke();
                }
            }
            GUILayout.EndHorizontal();
        }

        protected void RegisterNewAction(string actionName, Action action)
        {
            actions.Add(actionName, action);
        }

        protected void EnableCloseButton()
        {
            RegisterNewAction("Close", () => Close());
        }
    }
}