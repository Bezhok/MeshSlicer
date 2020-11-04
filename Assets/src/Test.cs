using System.Collections.Generic;
using UnityEngine;

namespace src
{
    public class Test : MonoBehaviour
    {
        public static List<SliceHierarchy> slices;
        public static GameObject obj;
        public static List<MappedPoint> gizmos = new List<MappedPoint>();
        private readonly Color[] colors = new Color[500];

        private void Start()
        {
            for (var i = 0; i < colors.Length; i++)
                colors[i] = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }

        private void Update()
        {
            if (slices != null)
                for (var j = 0; j < slices.Count; j++)
                {
                    if (slices[j] == null) continue;
                    
                    var points = new List<MappedPoint>(slices[j]._points);
                    for (var i = 0; i < points.Count - 1; i++)
                        Debug.DrawLine(
                            obj.transform.TransformPoint(points[i].v3),
                            obj.transform.TransformPoint(points[i + 1].v3),
                            colors[j % colors.Length]);
                }

            for (var i = 0; i < gizmos.Count - 1; i += 1)
                Debug.DrawLine(obj.transform.TransformPoint(gizmos[i].v3), obj.transform.TransformPoint(gizmos[i + 1].v3),
                    Color.red);
            
            if (gizmos.Count > 0)
                Debug.DrawLine(obj.transform.TransformPoint(gizmos[0].v3), obj.transform.TransformPoint(gizmos[gizmos.Count - 1].v3),
                    Color.red);
        }

        private void OnDrawGizmos()
        {
            for (var i = 0; i < gizmos.Count; i++) Gizmos.DrawSphere(obj.transform.TransformPoint(gizmos[i].v3), 0.006f);
        }
    }
}