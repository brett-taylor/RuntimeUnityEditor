using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RuntimeUnityEditor.Core.Utils;
using UnityEngine;

namespace RuntimeUnityEditor.Core.Gizmos
{
    public class LineRender : MonoBehaviour
    {
        public void OnPostRender()
        {
            LineStore.nonOccludedMaterial.SetPass(0);
            RenderLines(LineStore.nonOccludedRender);
            LineStore.occludedMaterial.SetPass(0);
            RenderLines(LineStore.occludedRender);
        }
        private static void RenderLines(List<RenderableWrapper> renderList)
        {
            foreach (RenderableWrapper shape in renderList)
            {
                foreach (Line line in shape.lines)
                {
                    GL.Begin(GL.LINES);
                    GL.Color(shape.color);
                    GL.Vertex(line.start);
                    GL.Vertex(line.finish);
                    GL.End();
                }
            }
        }
    }
}
