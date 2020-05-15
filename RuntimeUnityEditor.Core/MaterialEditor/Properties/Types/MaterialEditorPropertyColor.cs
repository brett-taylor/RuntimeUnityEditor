using System.Globalization;
using RuntimeUnityEditor.Core.Utils;
using UnityEngine;

namespace RuntimeUnityEditor.Core.MaterialEditor.Properties.Types
{
    public class MaterialEditorPropertyColor : MaterialEditorPropertyType
    {
        private static Texture2D PREVIEW_COLOR_TEXTURE;
        private static readonly int PREVIEW_COLOR_SIZE = 20;
        private static readonly int COLOR_MIN_BOUND = 0;
        private static readonly int COLOR_MAX_BOUND = 1;
        private static readonly GUILayoutOption FIELD_WIDTH = GUILayout.Width(40);
        private static readonly GUILayoutOption FIELD_HEIGHT = GUILayout.Height(19);
        private static readonly GUILayoutOption SLIDER_WIDTH = GUILayout.Height(10);
        private static readonly GUILayoutOption SLIDER_HEIGHT = GUILayout.Width(33);
        
        public MaterialEditorPropertyColor(string property) : base(property)
        {
        }

        protected override void Draw(Material material)
        {
            CreateTextureIfNeeded();

            var currentColor = material.GetColor(Property);
            var newColor = new Color(currentColor.r, currentColor.g, currentColor.b, currentColor.a);
            
            var guiColor = GUI.color;
            GUI.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1f);
            GUILayout.Label(PREVIEW_COLOR_TEXTURE, GUILayout.Width(PREVIEW_COLOR_SIZE), GUILayout.Height(PREVIEW_COLOR_SIZE));
            
            newColor.r = GUILayout.HorizontalSlider(currentColor.r, COLOR_MIN_BOUND, COLOR_MAX_BOUND, SLIDER_WIDTH, SLIDER_HEIGHT);
            float.TryParse(GUILayout.TextField(newColor.r.ToString("F2", CultureInfo.InvariantCulture), FIELD_WIDTH, FIELD_HEIGHT), NumberStyles.Any, CultureInfo.InvariantCulture, out newColor.r);
            
            newColor.g = GUILayout.HorizontalSlider(currentColor.g, COLOR_MIN_BOUND, COLOR_MAX_BOUND, SLIDER_WIDTH, SLIDER_HEIGHT);
            float.TryParse(GUILayout.TextField(newColor.g.ToString("F2", CultureInfo.InvariantCulture), FIELD_WIDTH, FIELD_HEIGHT), NumberStyles.Any, CultureInfo.InvariantCulture, out newColor.g);
            
            newColor.b = GUILayout.HorizontalSlider(currentColor.b, COLOR_MIN_BOUND, COLOR_MAX_BOUND, SLIDER_WIDTH, SLIDER_HEIGHT);
            float.TryParse(GUILayout.TextField(newColor.b.ToString("F2", CultureInfo.InvariantCulture), FIELD_WIDTH, FIELD_HEIGHT), NumberStyles.Any, CultureInfo.InvariantCulture, out newColor.b);
            
            newColor.a = GUILayout.HorizontalSlider(currentColor.a, COLOR_MIN_BOUND, COLOR_MAX_BOUND, SLIDER_WIDTH, SLIDER_HEIGHT);
            float.TryParse(GUILayout.TextField(newColor.a.ToString("F2", CultureInfo.InvariantCulture), FIELD_WIDTH, FIELD_HEIGHT), NumberStyles.Any, CultureInfo.InvariantCulture, out newColor.a);

            GUI.color = guiColor;
            
            if (!currentColor.IsEqualApprox(newColor, 0.1f))
            {
                material.SetColor(Property, newColor);
                PrintLog(currentColor, newColor);
            }
        }

        private void CreateTextureIfNeeded()
        {
            if (PREVIEW_COLOR_TEXTURE == null)
            {
                PREVIEW_COLOR_TEXTURE = new Texture2D(PREVIEW_COLOR_SIZE, PREVIEW_COLOR_SIZE);
            }
        }

        public override string GenerateSetPropertyCode(Material material)
        {
            var color = material.GetColor(Property);
            return $"SetColor(\"{Property}\", new Color({color.r}f, {color.g}f, {color.b}f, {color.a}f))";
        }
    }
}