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
                    
                    var points = new List<MappedPoint>(slices[j].Points);
                    for (var i = 0; i < points.Count - 1; i++)
                    {
                        Debug.DrawLine(
                            obj.transform.TransformPoint(points[i].v3),
                            obj.transform.TransformPoint(points[i + 1].v3),
                            colors[j % colors.Length]);
                        
                        // var c1 = points[i].v2;
                        // c1.Scale(new Vector2(2f, 12f));
                        // var c2 = points[i + 1].v2;
                        // c2.Scale(new Vector2(2f, 12f));
                        // Debug.DrawLine(
                        //     obj.transform.TransformPoint(c1),
                        //     obj.transform.TransformPoint(c2),
                        //     colors[j % colors.Length]);
                    }

                    Debug.DrawLine(
                        obj.transform.TransformPoint(points[0].v3),
                        obj.transform.TransformPoint(points[points.Count - 1].v3),
                        colors[j % colors.Length]);
                }

            // for (var i = 0; i < gizmos.Count - 1; i += 2)
            // {
            //     Debug.DrawLine(obj.transform.TransformPoint(gizmos[i].v3),
            //         obj.transform.TransformPoint(gizmos[i + 1].v3),
            //         Color.red);
            // }

            for (var i = 0; i < gizmos.Count - 1; i += 2)
            {
                // var c1 = gizmos[i].v2;c1.Scale(new Vector2(2f,12f));
                var c1 = gizmos[i].v2;
                c1.Scale(new Vector2(2f, 12f));
                var c2 = gizmos[i + 1].v2;
                c2.Scale(new Vector2(2f, 12f));
                // Gizmos.color = colors[(int) (i / 5)];
                Debug.DrawLine(obj.transform.TransformPoint(c1),
                    obj.transform.TransformPoint(c2), colors[i / 4]);
            }

            // if (gizmos.Count > 0)
            //     Debug.DrawLine(obj.transform.TransformPoint(gizmos[0].v3), obj.transform.TransformPoint(gizmos[gizmos.Count - 1].v3),
            //         Color.red);
        }

        private void OnDrawGizmos()
        {
            // Gizmos.DrawSphere(new Vector3(), 6f);
            // Debug.Log("89");
            for (var i = 0; i < gizmos.Count; i++)
            {
                Gizmos.color = colors[i / 5];
                Gizmos.DrawSphere(obj.transform.TransformPoint(gizmos[i].v3), 0.001f);
            }

            Gizmos.color = Color.red;
            for (var i = 0; i < gizmos.Count; i++)
            {
                var c1 = gizmos[i].v2;
                c1.Scale(new Vector2(2f, 12f));

                Gizmos.color = colors[i / 4];
                Gizmos.DrawSphere(obj.transform.TransformPoint(c1), 0.0005f);
            }
        }
    }
}