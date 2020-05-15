using UnityEngine;

namespace RuntimeUnityEditor.Core.MaterialEditor.Properties.Types
{
    public abstract class MaterialEditorPropertyType
    {
        public string Property { get; }

        protected MaterialEditorPropertyType(string property)
        {
            Property = property;
        }

        public void DrawGUI(Material material)
        {
            Draw(material);
        }
        
        protected abstract void Draw(Material material);

        protected void PrintLog(object oldValue, object newValue)
        {
            RuntimeUnityEditorCore.LOGGER.Log(LogLevel.Debug, $"Shader property {Property} updated from ({oldValue}) to ({newValue})");
        }

        public abstract string GenerateSetPropertyCode(Material material);
    }
}