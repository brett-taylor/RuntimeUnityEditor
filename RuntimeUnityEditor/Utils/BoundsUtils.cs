using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuntimeUnityEditor.Core.Utils
{
    public struct Line
    {
        public Vector3 start;
        public Vector3 finish;
    }
    public class RenderableWrapper
    {
        public Color color = Color.yellow;
        public int instanceId;
        public bool occluded = false;
        public virtual List<Line> lines => new List<Line>();
    }
    public class LineWrapper : RenderableWrapper
    {
        public Vector3 start;
        public Vector3 finish;
        public string name;
        public override List<Line> lines =>
            new List<Line> { new Line { start = this.start, finish = this.finish } };
    }
    public class BoxWrapper : RenderableWrapper
    {
        public GameObject obj;
        public Bounds colliderBounds;
        public override List<Line> lines =>
            BoundUtils.VectorToBox(colliderBounds.center,
                                   Vector3.Scale(colliderBounds.size, obj.transform.localScale) / 2);
    }
    
    public static class BoundUtils
    {
        private static bool SharesTwoAxis(Vector3 a, Vector3 b)
        {
            return (a[0] == b[0] && a[1] == b[1]) ||
                   (a[1] == b[1] && a[2] == b[2]) ||
                   (a[0] == b[0] && a[2] == b[2]);
        }
        private static bool SharesOneAxis(Vector3 a, Vector3 b)
        {
            return !(a[0] == b[0] && a[1] == b[1]) &&
                   !(a[1] == b[1] && a[2] == b[2]) &&
                   !(a[0] == b[0] && a[2] == b[2]) &&
                   (a[0] == b[0] || a[1] == b[1] || a[2] == b[2]);
        }
        public static List<Line> VectorToBox(Vector3 origin, Vector3 offset)
        {
            List<Vector3> bounds = new List<Vector3>();
            bounds.Add(new Vector3(origin.x + offset.x, origin.y + offset.y, origin.z + offset.z));
            bounds.Add(new Vector3(origin.x + offset.x, origin.y + offset.y, origin.z - offset.z));
            bounds.Add(new Vector3(origin.x + offset.x, origin.y - offset.y, origin.z + offset.z));
            bounds.Add(new Vector3(origin.x + offset.x, origin.y - offset.y, origin.z - offset.z));
            bounds.Add(new Vector3(origin.x - offset.x, origin.y + offset.y, origin.z + offset.z));
            bounds.Add(new Vector3(origin.x - offset.x, origin.y + offset.y, origin.z - offset.z));
            bounds.Add(new Vector3(origin.x - offset.x, origin.y - offset.y, origin.z + offset.z));
            bounds.Add(new Vector3(origin.x - offset.x, origin.y - offset.y, origin.z - offset.z));

            List<Line> lines = new List<Line>();
            for (int i = 0; i < 8; ++i)
            {
                for (int j = i + 1; j < 8; ++j)
                {
                    if (SharesTwoAxis(bounds[i], bounds[j]))
                    {
                        lines.Add(new Line { start = bounds[i], finish = bounds[j] });
                    }
                }
            }
            return lines;
        }
    }
}