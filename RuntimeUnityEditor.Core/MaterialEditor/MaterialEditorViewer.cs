using System.Linq;
using RuntimeUnityEditor.Core.MaterialEditor.Properties;
using RuntimeUnityEditor.Core.UI;
using UnityEngine;
using RuntimeUnityEditor.Core.MaterialEditor.Keywords;
using System.Collections.Generic;
using RuntimeUnityEditor.Core.MaterialEditor.CodeGeneration;
using RuntimeUnityEditor.Core.MaterialEditor.Properties.Types;
using RuntimeUnityEditor.Core.Popup.Dialogs;

namespace RuntimeUnityEditor.Core.MaterialEditor
{
    public class MaterialEditorViewer : Popup.Popup
    {
        public static readonly string MARMOSETUBER_SHADER_NAME = "MarmosetUBER";
        private const float SHADER_EDITOR_WIDTH = 500f;
        private readonly GUILayoutOption _propertyColumnWidth = GUILayout.Width(75f);
        private readonly GUILayoutOption _informationColumnWidth = GUILayout.Width(30f);
        private readonly GUILayoutOption _keywordColumnWidth = GUILayout.Width(400f);
        private Vector2 scrollPosition = Vector2.zero;

        private readonly Material editingMaterial;
        private bool materialKeywordsExpanded = false;
        private bool materialPropertiesExpanded = true;
        protected override bool ShouldEatInput => true;
        protected override bool IsWindowDraggable => true;
        protected override string WindowTitle => $"{editingMaterial.name} - {editingMaterial.shader.name}";
        internal override WindowState RenderOnlyInWindowState => WindowState.VISIBLE;
        internal override WindowState UpdateOnlyInWindowState => WindowState.VISIBLE;

        public MaterialEditorViewer(Material material)
        {
            editingMaterial = material;
            RegisterNewAction("Generate Code", () => MaterialEditorCodeGenerator.GenerateShaderCode(material));
            EnableCloseButton();
        }

        internal override Rect GetStartingRect(Rect screenSize, float centerWidth, float centerX)
        {
            var height = screenSize.height / 2;
            return new Rect(
                PADDING, 
                centerX - (height / 2), 
                SHADER_EDITOR_WIDTH, 
                height
            );
        }

        protected override bool PreCreatedWindow()
        {
            return editingMaterial != null;
        }

        protected override void PostCreatedWindow()
        {
        }

        internal override void Update()
        {
        }
        
        protected override void DrawPopupContents()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(SHADER_EDITOR_WIDTH), GUILayout.ExpandHeight(true));
            {
                if (editingMaterial.shader.name.Equals(MARMOSETUBER_SHADER_NAME))
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    {
                        if (GUILayout.Button("Keywords", GUILayout.ExpandWidth(true)))
                            materialKeywordsExpanded = !materialKeywordsExpanded;

                        if (materialKeywordsExpanded)
                            DrawKeywords();
                    }
                    GUILayout.EndVertical();
                }

                GUILayout.BeginVertical(GUI.skin.box);
                {
                    if (GUILayout.Button("Properties", GUILayout.ExpandWidth(true)))
                        materialPropertiesExpanded = !materialPropertiesExpanded;

                    if (materialPropertiesExpanded)
                        DrawProperties();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
        }
        
        private void DrawKeywords()
        {
            var shaderKeywords = new HashSet<string>(editingMaterial.shaderKeywords);
            DrawKeywordTableHeader();

            foreach (MaterialEditorKeywords keyword in System.Enum.GetValues(typeof(MaterialEditorKeywords)))
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                {
                    DrawKeywordTableRow(shaderKeywords, keyword);
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawKeywordTableHeader()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Keyword", GUI.skin.box, _keywordColumnWidth);
                GUILayout.Label("Toggled", GUI.skin.box, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();
        }

        private void DrawKeywordTableRow(ICollection<string> shaderKeywords, MaterialEditorKeywords keyword)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(keyword.ToString(), _keywordColumnWidth);
                var isEnabledCurrently = shaderKeywords.Contains(keyword.ToString());
                var isEnabledNew = GUILayout.Toggle(isEnabledCurrently, "", GUILayout.ExpandWidth(true));

                if (isEnabledCurrently != isEnabledNew)
                {
                    if (isEnabledNew)
                        editingMaterial.EnableKeyword(keyword.ToString());
                    else
                        editingMaterial.DisableKeyword(keyword.ToString());
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawProperties()
        {
            DrawPropertiesTableHeader();
            
            foreach (var property in MaterialEditorPropertyTypes.TYPES
                .Where(possibleProperty => editingMaterial.HasProperty(possibleProperty.Value.Property)))
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                {
                    DrawPropertiesTableRow(property.Key, property.Value);
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawPropertiesTableHeader()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Name", GUI.skin.box, _propertyColumnWidth);
                GUILayout.Label("Value", GUI.skin.box, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();
        }

        private void DrawPropertiesTableRow(MaterialEditorProperties property, MaterialEditorPropertyType type)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(type.Property, _propertyColumnWidth);
                type.DrawGUI(editingMaterial);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("?", _informationColumnWidth))
                    ShowHelp(property, type);
            }
            GUILayout.EndHorizontal();
        }

        private void ShowHelp(MaterialEditorProperties property, MaterialEditorPropertyType type)
        {
            new Dialog($"Keywords for {type.Property}", () =>
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("For the following property to work, you may need to enable these following keywords:");
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();;
                
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    var keywordCollection = MaterialEditorPropertyKeywords.KEYWORDS[property];
                    GUILayout.Label(keywordCollection.Length == 0 ? "No Keywords" : string.Join(", ", keywordCollection));
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();;
            });
        }
    }
}
