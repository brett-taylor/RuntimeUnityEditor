using System.Globalization;
using UnityEngine;

namespace RuntimeUnityEditor.Core.MaterialEditor.Properties.Types
{
    public class MaterialEditorPropertyFloat : MaterialEditorPropertyType
    {
        private static readonly GUILayoutOption FIELD_WIDTH = GUILayout.Width(100);
        private static readonly GUILayoutOption SLIDER_WIDTH = GUILayout.Width(100);
        
        private readonly float minimum;
        private readonly float maximum;
        
        public MaterialEditorPropertyFloat(string property, float minimum, float maximum) : base(property)
        {
            this.minimum = minimum;
            this.maximum = maximum;
        }
        
        public MaterialEditorPropertyFloat(string property) : base(property)
        {
            minimum = 0;
            maximum = 50;
        }

        protected override void Draw(Material material)
        {
            var currentValue = material.GetFloat(Property);
            var newValue = GUILayout.HorizontalSlider(currentValue, minimum, maximum, SLIDER_WIDTH);
            float.TryParse(GUILayout.TextField(newValue.ToString("F2", CultureInfo.InvariantCulture), FIELD_WIDTH),  NumberStyles.Any, CultureInfo.InvariantCulture, out newValue);
            
            if (currentValue != newValue)
            {
                material.SetFloat(Property, newValue);
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