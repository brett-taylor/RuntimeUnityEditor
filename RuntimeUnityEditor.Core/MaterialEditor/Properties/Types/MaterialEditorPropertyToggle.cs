using UnityEngine;

namespace RuntimeUnityEditor.Core.MaterialEditor.Properties.Types
{
    public class MaterialEditorPropertyToggle : MaterialEditorPropertyType
    {
        public MaterialEditorPropertyToggle(string property) : base(property)
        {
        }
        
        protected override void Draw(Material material)
        {
            var currentValue = material.GetFloat(Property) > 0;
            var newValue = GUILayout.Toggle(currentValue, "Enable");

            if (currentValue != newValue)
            {
                material.SetFloat(Property, newValue ? 1f : 0f);
                PrintLog(currentValue, newValue);
            }
        }

        public override string GenerateSetPropertyCode(Material material)
        {
            var value = material.GetFloat(Property);
            return $"SetFloat(\"{Property}\", {value}f)";
        }
    }
}