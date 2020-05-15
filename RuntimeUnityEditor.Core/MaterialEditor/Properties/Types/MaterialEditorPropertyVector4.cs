using System.Globalization;
using UnityEngine;

namespace RuntimeUnityEditor.Core.MaterialEditor.Properties.Types
{
    public class MaterialEditorPropertyVector4 : MaterialEditorPropertyType
    {
        private static readonly GUILayoutOption FIELD_WIDTH = GUILayout.Width(38);
        private static readonly GUILayoutOption FIELD_HEIGHT = GUILayout.Height(19);
        private static readonly GUILayoutOption SLIDER_WIDTH = GUILayout.Height(10);
        private static readonly GUILayoutOption SLIDER_HEIGHT = GUILayout.Width(33);

        private readonly int minBound;
        private readonly int maxBound;

        public MaterialEditorPropertyVector4(string property, int minBound, int maxBound) : base(property)
        {
            this.minBound = minBound;
            this.maxBound = maxBound;
        }
        
        public MaterialEditorPropertyVector4(string property) : base(property)
        {
            minBound = 0;
            maxBound = 1;
        }

        protected override void Draw(Material material)
        {
            var currentVector = material.GetVector(Property);
            var newVector = new Vector4(currentVector.x, currentVector.y, currentVector.z, currentVector.w);
            
            newVector.x = GUILayout.HorizontalSlider(currentVector.x, minBound, maxBound, SLIDER_WIDTH, SLIDER_HEIGHT);
            float.TryParse(GUILayout.TextField(newVector.x.ToString("F2", CultureInfo.InvariantCulture), FIELD_WIDTH, FIELD_HEIGHT), NumberStyles.Any, CultureInfo.InvariantCulture, out newVector.x);
            
            newVector.y = GUILayout.HorizontalSlider(currentVector.y, minBound, maxBound, SLIDER_WIDTH, SLIDER_HEIGHT);
            float.TryParse(GUILayout.TextField(newVector.y.ToString("F2", CultureInfo.InvariantCulture), FIELD_WIDTH, FIELD_HEIGHT), NumberStyles.Any, CultureInfo.InvariantCulture, out newVector.y);
            
            newVector.z = GUILayout.HorizontalSlider(currentVector.z, minBound, maxBound, SLIDER_WIDTH, SLIDER_HEIGHT);
            float.TryParse(GUILayout.TextField(newVector.z.ToString("F2", CultureInfo.InvariantCulture), FIELD_WIDTH, FIELD_HEIGHT), NumberStyles.Any, CultureInfo.InvariantCulture, out newVector.z);
            
            newVector.w = GUILayout.HorizontalSlider(currentVector.w, minBound, maxBound, SLIDER_WIDTH, SLIDER_HEIGHT);
            float.TryParse(GUILayout.TextField(newVector.w.ToString("F2", CultureInfo.InvariantCulture), FIELD_WIDTH, FIELD_HEIGHT), NumberStyles.Any, CultureInfo.InvariantCulture, out newVector.w);
            
            if (!currentVector.Equals(newVector))
            {
                material.SetVector(Property, newVector);
                PrintLog(currentVector, newVector);
            }
        }

        public override string GenerateSetPropertyCode(Material material)
        {
            var vector = material.GetVector(Property);
            return $"SetVector(\"{Property}\", new Vector4({vector.x}f, {vector.y}f, {vector.z}f, {vector.w}f))";
        }
    }
}