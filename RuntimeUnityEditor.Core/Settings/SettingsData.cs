using System;

namespace RuntimeUnityEditor.Core.Settings
{
    [Serializable]
    public class SettingsData
    {
        public bool EnableClickForParentGameObject = false;
        public bool EnableClickForChildGameObject = false;
        public bool ShowGizmos = false;
        public bool ShowGizmosOutsideEditor = false;
        public bool Wireframe = false;
        public string DNSpyPath = "";
        public bool PinnedVariablesCompactMode = true;
    }
}
