namespace RuntimeUnityEditor.Core.Settings
{
    public class Settings
    {
        public bool EnableClickForParentGameObject { get; set; } = false;
        public bool EnableClickForChildGameObject { get; set; } = false;
        public bool ShowGizmos { get; set; } = false;
        public bool ShowGizmosOutsideEditor { get; set; } = false;
        public bool Wireframe { get; set; } = false;
        public string DNSpyPath { get; set; } = "";
    }
}
