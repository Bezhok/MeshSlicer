using System.Collections.Generic;
using UnityEngine;

namespace src
{
    public class Slicer
    {
        private readonly Mesh _mesh;
        private readonly GameObject _srcObject;
        private GameObject _lowerObj;
        private GameObject _upperObj;

        public Slicer(Mesh mesh, GameObject gameObject)
        {
            _mesh = mesh;
            _srcObject = gameObject;
        }

        public void Destroy()
        {
            Object.Destroy(_lowerObj);
            Object.Destroy(_upperObj);
        }

        private bool IsPointLower(Vector3 point, Vector3 planePoint, Vector3 planeNormal)
        {
            return Vector3.Dot(planePoint - point, planeNormal) < 0;
        }

        public void Slice(Vector3 slicerNormal, Vector3 slicerPoint, bool shouldDisplayLowerSide,
            bool shouldDisplayUpperSide)
        {
            var srcVerts = _mesh.vertices;
            var srcEbo = _mesh.triangles;

            var lowerEbo = new List<int>();
            var upperEbo = new List<int>();
            var upperMesh = Object.Instantiate(_mesh);
            var lowerMesh = Object.Instantiate(_mesh);

            var interLow = new Intersector(slicerPoint, slicerNormal, _srcObject, _mesh);
            var interUp = new Intersector(slicerPoint, slicerNormal, _srcObject, _mesh);
            for (var i = 0; i < _mesh.triangles.Length; i += 3)
            {
                var objVert1 = _srcObject.transform.TransformPoint(srcVerts[srcEbo[i]]);
                var objVert2 = _srcObject.transform.TransformPoint(srcVerts[srcEbo[i + 1]]);
                var objVert3 = _srcObject.transform.TransformPoint(srcVerts[srcEbo[i + 2]]);

                var isFirstLower = IsPointLower(objVert1, slicerPoint, slicerNormal);
                var isSecondLower = IsPointLower(objVert2, slicerPoint, slicerNormal);
                var isThirdLower = IsPointLower(objVert3, slicerPoint, slicerNormal);

                if (isFirstLower && isSecondLower && isThirdLower)
                {
                    if (shouldDisplayLowerSide)
                    {
                        lowerEbo.Add(srcEbo[i]);
                        lowerEbo.Add(srcEbo[i + 1]);
                        lowerEbo.Add(srcEbo[i + 2]);
                    }
                }
                else if (!isFirstLower && !isSecondLower && !isThirdLower)
                {
                    if (shouldDisplayUpperSide)
                    {
                        upperEbo.Add(srcEbo[i]);
                        upperEbo.Add(srcEbo[i + 1]);
                        upperEbo.Add(srcEbo[i + 2]);
                    }
                }
                else
                {
                    if (shouldDisplayLowerSide)
                        CreateTriangle(interLow, i, objVert1, objVert2, objVert3, isFirstLower, isSecondLower,
                            isThirdLower);

                    if (shouldDisplayUpperSide)
                        CreateTriangle(interUp, i, objVert1, objVert2, objVert3, !isFirstLower, !isSecondLower,
                            !isThirdLower);
                }
            }

            if (shouldDisplayLowerSide) UpdateMesh(ref _lowerObj, interLow, lowerMesh, lowerEbo);

            if (shouldDisplayUpperSide) UpdateMesh(ref _upperObj, interUp, upperMesh, upperEbo);
        }

        private void UpdateMesh(ref GameObject borderObj, Intersector intersector, Mesh mesh, List<int> triangles)
        {
            borderObj = Object.Instantiate(_srcObject, _srcObject.transform.parent);
            foreach (Transform child in borderObj.transform) {
                MonoBehaviour.Destroy(child.gameObject);
            }
            
            borderObj.GetComponent<MeshFilter>().sharedMesh = intersector.CreateMesh();

            mesh.triangles = triangles.ToArray();
            _srcObject.GetComponent<MeshFilter>().sharedMesh = mesh;

            borderObj.transform.position = _srcObject.transform.position;
            borderObj.transform.rotation = _srcObject.transform.rotation;
            borderObj.transform.localScale = _srcObject.transform.localScale;
        }

        private void CreateTriangle(Intersector inter, int i, Vector3 objVert1, Vector3 objVert2, Vector3 objVert3,
            bool isFirstLower, bool isSecondLower, bool isThirdLower)
        {
            if (isFirstLower && !isSecondLower && !isThirdLower)
                inter.CreateTriangleOnePointOnLeft(objVert1, objVert2, objVert3, i, 0, 1, 2);
            else if (isSecondLower && !isFirstLower && !isThirdLower)
                inter.CreateTriangleOnePointOnLeft(objVert2, objVert1, objVert3, i, 1, 0, 2);
            else if (isThirdLower && !isFirstLower && !isSecondLower)
                inter.CreateTriangleOnePointOnLeft(objVert3, objVert1, objVert2, i, 2, 0, 1);
            else if (isFirstLower && isSecondLower && !isThirdLower)
                inter.CreateTrianglesTwoPointsOnLeft(objVert1, objVert2, objVert3, i, 0, 1, 2);
            else if (!isFirstLower && isSecondLower && isThirdLower)
                inter.CreateTrianglesTwoPointsOnLeft(objVert2, objVert3, objVert1, i, 1, 2, 0);
            else if (isFirstLower && !isSecondLower && isThirdLower)
                inter.CreateTrianglesTwoPointsOnLeft(objVert1, objVert3, objVert2, i, 0, 2, 1);
        }
    }
}