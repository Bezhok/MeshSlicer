using System.Collections.Generic;
using UnityEngine;

namespace src
{
    public class Test : MonoBehaviour
    {
        private List<Vector2> _points;
        private List<Triangle> tris;

        private void Start()
        {
            // test data
            _points = new List<Vector2>();

            _points.Add(new Vector2(0, 0));
            _points.Add(new Vector2(2, 2));
            _points.Add(new Vector2(3, 0));
            _points.Add(new Vector2(4, 3));
            _points.Add(new Vector2(-2, 5));
            _points.Add(new Vector2(-1, 4));
            _points.Add(new Vector2(1, 3));
            _points.Add(new Vector2(-1, 1));

            var x = new Triangulator(_points);

            tris = x.Triangulate();
            var xx = 0;
        }

        private void Update()
        {
            var v = new Vector2(0.1f, 0);
            for (var i = 0; i < _points.Count - 1; i++) Debug.DrawLine(_points[i] + v, _points[i + 1] + v);

            Debug.DrawLine(_points[0] + v, _points[_points.Count - 1] + v);

            foreach (var tri in tris)
            {
                Debug.DrawLine(tri.A, tri.B, Color.red);
                Debug.DrawLine(tri.A, tri.C, Color.red);
                Debug.DrawLine(tri.B, tri.C, Color.red);
            }

            // var trii = tris[3];
            // Debug.DrawLine(trii.A, trii.B, Color.blue);
            // Debug.DrawLine(trii.A, trii.C, Color.blue);
            // Debug.DrawLine(trii.B, trii.C, Color.blue);
        }
    }
}