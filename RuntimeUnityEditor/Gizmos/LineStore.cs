using RuntimeUnityEditor.Core.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace RuntimeUnityEditor.Core.Gizmos
{
    public static class LineStore
    {
        public static Material occludedMaterial;
        public static Material nonOccludedMaterial;
        public static List<RenderableWrapper> occludedRender;
        public static List<RenderableWrapper> nonOccludedRender;
        private static void CreateLineMaterial()
        {
            if (!occludedMaterial)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                occludedMaterial = new Material(shader);
                occludedMaterial.hideFlags = HideFlags.HideAndDontSave;
                occludedMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                occludedMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                occludedMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                occludedMaterial.SetInt("_ZWrite", 1);
            }
            if (!nonOccludedMaterial)
            {
                nonOccludedMaterial = new Material(occludedMaterial);
                nonOccludedMaterial.SetInt("_ZTest", -1);
            }
        }

        static LineStore()
        {
            CreateLineMaterial();
            occludedRender = new List<RenderableWrapper>();
            nonOccludedRender = new List<RenderableWrapper>();
        }

        public static void AddBox(GameObject objWithBoxCollider)
        {
            foreach (var collider in objWithBoxCollider.GetComponents<BoxCollider>()) {
                var box = new BoxWrapper();
                box.color = Color.green;
                box.obj = objWithBoxCollider;
                box.instanceId = box.obj.GetInstanceID();
                box.collider = collider;
                AddRenderable(box);
            }
        }

        public static void AddRenderable(RenderableWrapper wrapper)
        {
            if (wrapper.occluded)
            {
                occludedRender.Add(wrapper);
            }
            else
            {
                nonOccludedRender.Add(wrapper);
            }
        }

        public static void RemoveRenderable(int instanceId)
        {
            occludedRender.RemoveAll((x) => x.instanceId == instanceId);
            nonOccludedRender.RemoveAll((x) => x.instanceId == instanceId);
        }

        public static void RemoveRenderable(GameObject obj)
        {
            RemoveRenderable(obj.GetInstanceID());
        }
    }
}
