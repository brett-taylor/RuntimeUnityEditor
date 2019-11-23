using RuntimeUnityEditor.Core.Utils;
using UnityEngine;

namespace RuntimeUnityEditor.Core.Gizmos
{
    public class RendererGizmo : IGizmoEntry
    {
        private readonly Renderer _renderer;
        private readonly Renderer[] _childRenderers;
        private readonly LineRenderer _lineRenderer;

        public RendererGizmo(Renderer renderer, Renderer[] childRenderers)
        {
            _renderer = renderer;
            _childRenderers = childRenderers;
            if (renderer == null)
            {
                ErrorMessage.AddMessage($"BOOOO renderer was null");
            }
            else
            {
                _lineRenderer = renderer.gameObject.AddComponent<LineRenderer>();
            }
        }

        public void Destroy()
        {
            Object.Destroy(_lineRenderer);
        }

        public void Draw()
        {
            var bounds = _renderer.bounds;
            if (_childRenderers.Length > 0)
                bounds.Encapsulate(BoundsUtils.CombineBounds(_childRenderers));

            var rect = bounds.BoundsToScreenRect(Camera.main);
        }
    }
}
