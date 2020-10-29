using System.Collections.Generic;
using UnityEngine;

namespace src
{
    public class Test : MonoBehaviour
    {
        private List<Vector2> _points;
       public  static List<List<Triangle>> trisl = new List<List<Triangle>>();

       private List<Triangle> lll;
        Color[] cc = new Color[500];
        private void Start()
        {
            for (int i = 0; i < cc.Length; i++)
            {
                cc[i] = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            }
            // test data
            _points = new List<Vector2>();

            // _points.Add(new Vector2(0, 0));
            // _points.Add(new Vector2(2, 2));
            // _points.Add(new Vector2(3, 0));
            // _points.Add(new Vector2(4, 3));
            // _points.Add(new Vector2(-2, 5));
            // _points.Add(new Vector2(-1, 4));
            // _points.Add(new Vector2(1, 3));
            // _points.Add(new Vector2(-1, 1));

            // _points.Add(new Vector2(0, 4));
            // _points.Add(new Vector2(2, 2));
            // _points.Add(new Vector2(3, 0));
            // _points.Add(new Vector2(4, 1));
            // _points.Add(new Vector2(7, 1));
            // _points.Add(new Vector2(8, 0));
            // _points.Add(new Vector2(9, 2));
            // _points.Add(new Vector2(11, 5));
            // _points.Add(new Vector2(10, 5));
            // _points.Add(new Vector2(8, 2));
            // _points.Add(new Vector2(3, 2));
            // _points.Add(new Vector2(1, 5));
            
            // _points.Add(new Vector2(0, 4));
;
            _points.Add(new Vector2(3, 0));
            _points.Add(new Vector2(4, 1));
            // _points.Add(new Vector2(7, 1));
            _points.Add(new Vector2(8, 0));

            // _points.Add(new Vector2(11, 5));
            _points.Add(new Vector2(10, 5));
            _points.Add(new Vector2(8, 2));
            _points.Add(new Vector2(3, 2));
            _points.Add(new Vector2(1, 5));
            
            List<MappedPoint> ll2 = new List<MappedPoint>();
            for (int i = 0; i < _points.Count; i++)
            {
                ll2.Add(new MappedPoint(new Vector3(i,i,i), Vector3.back, Vector3.down));
                ll2[i].v2 = _points[i];
                // ll2[i].v3 = new Vector3(i,i,i);
            } 
            var x = new Triangulator(ll2);

            lll = x.Triangulate();
            var xx = 0;
            
            // for (var i = 0; i < lll.Count ; i++)
            // {
            //     Debug.Log(lll[i].A.v2);
            //     Debug.Log(lll[i].B.v2);
            //     Debug.Log(lll[i].C.v2);
            //     Debug.Log("----");
            //
            // }
        }

        private void Update()
        {

            if (_points11 != null)
            {
                for (int j = 0; j < _points11.Count; j++)
                {
                    if (_points11[j] == null) continue;
                    var _points1 = new List<MappedPoint>(_points11[j]._points);
                    var v1 = new Vector3(0.0f, 0);
                    // var c = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                    // for (var i = 0; i < _points1.Count - 1; i++) Debug.DrawLine(g.transform.TransformPoint(_points1[i].v3) + v1, g.transform.TransformPoint(_points1[i + 1].v3) + v1,
                    //     cc[j%cc.Length]);

                    for (var i = 0; i < _points1.Count - 1; i++) Debug.DrawLine(
                        g.transform.TransformPoint(_points1[i].v3) ,
                        g.transform.TransformPoint(_points1[i + 1].v3),
                        Color.red);
                    
                    // Debug.DrawLine(g.transform.TransformPoint(_points1[0]) + v1, 
                    //     g.transform.TransformPoint(_points1[_points1.Count - 1]) + v1);
                }

            }
            int k = 0;

            foreach (var tris in trisl)
            {
                // break;
                // Debug.Log("----");
                // Debug.Log(tris.Count);
                k++;
                foreach (var tri in tris)
                {

                    // Debug.Log(g.transform.TransformPoint(tri.A.v3));
                    // Debug.Log(g.transform.TransformPoint(tri.B.v3));
                    // Debug.Log(g.transform.TransformPoint(tri.C.v3));
                    Debug.DrawLine(g.transform.TransformPoint(tri.A.v3), g.transform.TransformPoint(tri.B.v3), cc[k%cc.Length]);
                    Debug.DrawLine(g.transform.TransformPoint(tri.A.v3), g.transform.TransformPoint(tri.C.v3), cc[k%cc.Length]);
                    Debug.DrawLine(g.transform.TransformPoint(tri.B.v3), g.transform.TransformPoint(tri.C.v3), cc[k%cc.Length]);
                }
            }
            // var trii = tris[3];
            // Debug.DrawLine(trii.A, trii.B, Color.blue);
            // Debug.DrawLine(trii.A, trii.C, Color.blue);
            // Debug.DrawLine(trii.B, trii.C, Color.blue);



            
            var v = new Vector2(0.0f, 0);
            for (var i = 0; i < _points.Count - 1; i++) Debug.DrawLine(_points[i] + v, _points[i + 1] + v);
            
            Debug.DrawLine(_points[0] + v, _points[_points.Count - 1] + v);
            for (var i = 0; i < lll.Count ; i++)
            {
                Debug.DrawLine(lll[i].A.v2, lll[i].B.v2, Color.blue);
                Debug.DrawLine(lll[i].A.v2, lll[i].C.v2, Color.blue);
                Debug.DrawLine(lll[i].B.v2, lll[i].C.v2, Color.blue);
            }
        }
        

        
        public static List<SliceHierarchy> _points11;
        public static GameObject g;
        public static GameObject g2;
    }
}