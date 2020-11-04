using System.Collections.Generic;
using System.Linq;
using Plugins;
using UnityEngine;

namespace src
{
    public class Intersector
    {
        private readonly List<Vector3> _normals;
        private readonly Vector3 _planeNormal;
        private readonly Vector3 _planePoint;
        private readonly GameObject _srcObject;
        private readonly List<Vector4> _tangents;
        private readonly List<int> _triangles;
        private readonly List<int> _triangles2;
        private readonly List<Vector2> _uvs;
        private readonly List<Vector3> _verts;
        private readonly WholeSlicePlane _wholeSlicePlane;

        private void CreatePlaneSliceTriangle(Triangle triangle, Vector3 normal, int t0, int t1, int t2)
        {
            _verts.Add(triangle.A.v3);
            _verts.Add(triangle.B.v3);
            _verts.Add(triangle.C.v3);
            
            _triangles.Add(t0);
            _triangles.Add(t1);
            _triangles.Add(t2);
                    
            _normals.Add(normal);
            _normals.Add(normal);
            _normals.Add(normal);

            if (_tangents.Any())
            {
                _tangents.Add(new Vector4());
                _tangents.Add(new Vector4());
                _tangents.Add(new Vector4());
            }

            if (_uvs.Any())
            {
                _uvs.Add(new Vector2());
                _uvs.Add(new Vector2());
                _uvs.Add(new Vector2());
            }
        }
        public Mesh CreateMesh()
        {
            var slicePlane = _wholeSlicePlane.CreateSlicePlane();
            var localNormal = _srcObject.transform.InverseTransformDirection(_planeNormal);
            
            //TODO
            for (int i = 0; i < slicePlane.Count; i++)
            {
                for (int j = 0; j < slicePlane[i].Count; j++)
                {
                    int n = _verts.Count;
                    CreatePlaneSliceTriangle(slicePlane[i][j], -localNormal, n+0, n+1, n+2);
                    CreatePlaneSliceTriangle(slicePlane[i][j], -localNormal, n+2, n+1, n+0);
                }
            }
            
            var newMesh = new Mesh();
            newMesh.vertices = _verts.ToArray();
            newMesh.normals = _normals.ToArray();
            newMesh.uv = _uvs.ToArray();
            newMesh.triangles = _triangles.ToArray();
            newMesh.tangents = _tangents.ToArray();
            
            return newMesh;
        }
        public Intersector(Vector3 planePoint,
            Vector3 planeNormal, GameObject srcObject, Mesh mesh)
        {
            _planeNormal = planeNormal;
            _planePoint = planePoint;
            _srcObject = srcObject;
            _wholeSlicePlane = new WholeSlicePlane(srcObject.transform.InverseTransformPoint(planeNormal));
            
            _verts = new List<Vector3>();
            _normals = new List<Vector3>();
            _uvs = new List<Vector2>();
            _triangles = new List<int>();
            _tangents = new List<Vector4>();
        }

        public Intersector(Vector3 planePoint,
            Vector3 planeNormal, GameObject srcObject, Mesh mesh, List<int> triangles)
        {
            _planeNormal = planeNormal;
            _planePoint = planePoint;
            _srcObject = srcObject;
            _wholeSlicePlane = new WholeSlicePlane(srcObject.transform.InverseTransformPoint(planeNormal));
            
            _verts = new List<Vector3>(mesh.vertices);
            _normals = new List<Vector3>(mesh.normals);
            _uvs = new List<Vector2>(mesh.uv);
            _tangents = new List<Vector4>(mesh.tangents);
            _triangles = triangles;
            _triangles2 = new List<int>(mesh.triangles);
        }

        private Vector3 IntersectionPlaneLinePoint(Vector3 pointF, Vector3 pointS, Vector3 planePoint,
            Vector3 planeNormal)
        {
            var t = Vector3.Dot(planeNormal, planePoint - pointF) /
                    Vector3.Dot(planeNormal, pointS - pointF);
            var intersectionPoint = pointF + t * (pointS - pointF);
            return intersectionPoint;
        }

        //                   leftPoint
        //                   |\
        //                   | \
        // intersectionPoint2|__\ intersectionPoint1
        //                   |   \
        //        rightPoint2|____\ rightPoint1
        //
        public void CreateTriangleOnePointOnLeft(Vector3 leftPoint, Vector3 rightPoint1, Vector3 rightPoint2, int i,
            int n0, int n1, int n2)
        {
            var n = _verts.Count;
            var intersectionPoint1 = IntersectionPlaneLinePoint(leftPoint, rightPoint1, _planePoint, _planeNormal);
            var intersectionPoint2 = IntersectionPlaneLinePoint(leftPoint, rightPoint2, _planePoint, _planeNormal);

            var localLeftPoint1 = _srcObject.transform.InverseTransformPoint(leftPoint);
            var localIntersectionPoint1 = _srcObject.transform.InverseTransformPoint(intersectionPoint1);
            var localIntersectionPoint2 = _srcObject.transform.InverseTransformPoint(intersectionPoint2);

            _wholeSlicePlane.AddSlicePlanePoints(localIntersectionPoint1, localIntersectionPoint2);

            int t0 = _triangles2[i + n0];
            int t1 = _triangles2[i + n1];
            int t2 = _triangles2[i + n2];
            
            var localRightPoint1 = _srcObject.transform.InverseTransformPoint(rightPoint1);
            var localRightPoint2 = _srcObject.transform.InverseTransformPoint(rightPoint2);

            if (_tangents.Any())
                AddTangents(_tangents[t0], _tangents[t1], _tangents[t2]);

            if (_uvs.Any())
            {
                var b1 = new Barycentric(localLeftPoint1, localRightPoint1, localRightPoint2, localIntersectionPoint1);
                var localIntersectionPoint1Uv = b1.Interpolate(_uvs[t0],
                    _uvs[t1],
                    _uvs[t2]);

                var b2 = new Barycentric(localLeftPoint1, localRightPoint1, localRightPoint2, localIntersectionPoint2);
                var localIntersectionPoint2Uv = b2.Interpolate(_uvs[t0],
                    _uvs[t1],
                    _uvs[t2]);

                AddUVs(_uvs[t0], localIntersectionPoint1Uv, localIntersectionPoint2Uv);
            }

            var b3 = new Barycentric(localLeftPoint1, localRightPoint1, localRightPoint2, localIntersectionPoint1);
            var localIntersectionPoint1Normal = b3.Interpolate(_normals[t0],
                _normals[t1],
                _normals[t2]);

            var b4 = new Barycentric(localLeftPoint1, localRightPoint1, localRightPoint2, localIntersectionPoint2);
            var localIntersectionPoint2Normal = b4.Interpolate(_normals[t0],
                _normals[t1],
                _normals[t2]);

            AddNormals(_normals[t0], localIntersectionPoint1Normal,
                localIntersectionPoint2Normal);
            AddVerts(localLeftPoint1, localIntersectionPoint1, localIntersectionPoint2);


            if (n0 == 0 || n0 == 2)
                AddTriangle(n, 0, 1, 2);
            else //if (n0 == 1)
                AddTriangle(n, 0, 2, 1);
        }

        //                   rightPoint1
        //                   |\
        //                   | \
        // intersectionPoint2|__\ intersectionPoint1
        //                   |   \
        //        leftPoint2 |____\ leftPoint1
        //
        public void CreateTrianglesTwoPointsOnLeft(Vector3 leftPoint1, Vector3 leftPoint2, Vector3 rightPoint1, int i,
            int n0, int n1, int n2)
        {
            var n = _verts.Count;
            var intersectionPoint1 = IntersectionPlaneLinePoint(leftPoint1, rightPoint1, _planePoint, _planeNormal);
            var intersectionPoint2 = IntersectionPlaneLinePoint(leftPoint2, rightPoint1, _planePoint, _planeNormal);

            var localLeftPoint1 = _srcObject.transform.InverseTransformPoint(leftPoint1);
            var localLeftPoint2 = _srcObject.transform.InverseTransformPoint(leftPoint2);

            var localIntersectionPoint1 = _srcObject.transform.InverseTransformPoint(intersectionPoint1);
            var localIntersectionPoint2 = _srcObject.transform.InverseTransformPoint(intersectionPoint2);

            _wholeSlicePlane.AddSlicePlanePoints(localIntersectionPoint1, localIntersectionPoint2);
            
            int t0 = _triangles2[i + n0];
            int t1 = _triangles2[i + n1];
            int t2 = _triangles2[i + n2];
            var localRightPoint1 = _srcObject.transform.InverseTransformPoint(rightPoint1);

            if (_tangents.Any())
                AddTangents(_tangents[t0], _tangents[t1], _tangents[t2], _tangents[t2]);

            if (_uvs.Any())
            {
                var b1 = new Barycentric(localLeftPoint1, localLeftPoint2, localRightPoint1, localIntersectionPoint1);
                var intersectionPoint1Uv = b1.Interpolate(_uvs[t0],
                    _uvs[t1],
                    _uvs[t2]);

                var b2 = new Barycentric(localLeftPoint1, localLeftPoint2, localRightPoint1, localIntersectionPoint2);
                var intersectionPoint2Uv = b2.Interpolate(_uvs[t0],
                    _uvs[t1],
                    _uvs[t2]);

                AddUVs(_uvs[t0],
                    _uvs[t1],
                    intersectionPoint1Uv,
                    intersectionPoint2Uv);
            }

            var b3 = new Barycentric(localLeftPoint1, localLeftPoint2, localRightPoint1, localIntersectionPoint1);
            var localIntersectionPoint1Normal = b3.Interpolate(_normals[t0],
                _normals[t1],
                _normals[t2]);

            var b4 = new Barycentric(localLeftPoint1, localLeftPoint2, localRightPoint1, localIntersectionPoint2);
            var localIntersectionPoint2Normal = b4.Interpolate(_normals[t0],
                _normals[t1],
                _normals[t2]);

            AddNormals(_normals[t0],
                _normals[t1],
                localIntersectionPoint1Normal,
                localIntersectionPoint2Normal);

            AddVerts(localLeftPoint1,
                localLeftPoint2,
                localIntersectionPoint1,
                localIntersectionPoint2);


            if (n0 == 0 && n1 == 2)
            {
                AddTriangle(n, 1, 0, 2);
                AddTriangle(n, 1, 2, 3);
            }
            else //if (n0 == 1)
            {
                AddTriangle(n, 0, 1, 3);
                AddTriangle(n, 0, 3, 2);
            }
        }

        private void AddTangents(Vector4 n1, Vector4 n2, Vector4 n3, Vector4 n4)
        {
            _tangents.Add(n1);
            _tangents.Add(n2);
            _tangents.Add(n3);
            _tangents.Add(n4);
        }

        private void AddUVs(Vector2 n1, Vector2 n2, Vector2 n3, Vector2 n4)
        {
            _uvs.Add(n1);
            _uvs.Add(n2);
            _uvs.Add(n3);
            _uvs.Add(n4);
        }

        private void AddNormals(Vector3 n1, Vector3 n2, Vector3 n3, Vector3 n4)
        {
            _normals.Add(n1);
            _normals.Add(n2);
            _normals.Add(n3);
            _normals.Add(n4);
        }

        private void AddVerts(Vector3 n1, Vector3 n2, Vector3 n3, Vector3 n4)
        {
            _verts.Add(n1);
            _verts.Add(n2);
            _verts.Add(n3);
            _verts.Add(n4);
        }

        private void AddTriangle(int n, int i0, int i1, int i2)
        {
            _triangles.Add(n + i0);
            _triangles.Add(n + i1);
            _triangles.Add(n + i2);
        }

        private void AddTangents(Vector4 n1, Vector4 n2, Vector4 n3)
        {
            _tangents.Add(n1);
            _tangents.Add(n2);
            _tangents.Add(n3);
        }

        private void AddUVs(Vector2 n1, Vector2 n2, Vector2 n3)
        {
            _uvs.Add(n1);
            _uvs.Add(n2);
            _uvs.Add(n3);
        }

        private void AddNormals(Vector3 n1, Vector3 n2, Vector3 n3)
        {
            _normals.Add(n1);
            _normals.Add(n2);
            _normals.Add(n3);
        }

        private void AddVerts(Vector3 n1, Vector3 n2, Vector3 n3)
        {
            _verts.Add(n1);
            _verts.Add(n2);
            _verts.Add(n3);
        }
    }
}