using System.Collections.Generic;
using System.Linq;
using Plugins;
using UnityEngine;

namespace src
{
    public class Slicer : MonoBehaviour
    {
        private Mesh _lowerMesh;
        private Mesh _upperMesh;
        private GameObject _lowerObj;
        private GameObject _upperObj;

        private Mesh _mesh;

        private Vector3 _prevSlicerPos;
        private Vector3 _prevSlicerRotation;
        private Mesh _slicerMesh;

        private Vector3 _slicerNormal;
        private Vector3 _slicerPoint;
        

        [SerializeField] private GameObject slicerQuad;
        [SerializeField] private GameObject srcObject;


        private bool IsPointLower(Vector3 point, Vector3 planePoint, Vector3 planeNormal)
        {
            return Vector3.Dot(planePoint - point, planeNormal) < 0;
        }

        public bool shouldDisplayLowerSide = true;
        public bool shouldDisplayUpperSide = false;
        private void Slice()
        {
            var srcVerts = _mesh.vertices;
            var srcEbo = _mesh.triangles;
            
            var lowerEbo = new List<int>();
            var upperEbo = new List<int>();
            var upperMesh = Instantiate(_mesh);
            var lowerMesh = Instantiate(_mesh);
            
            _slicerNormal = slicerQuad.transform.TransformDirection(_slicerMesh.normals[0]);
            _slicerPoint = slicerQuad.transform.TransformPoint(_slicerMesh.vertices[0]);

            var interLow = new Intersector(_slicerPoint, _slicerNormal, srcObject, _mesh);
            var interUp = new Intersector(_slicerPoint, _slicerNormal, srcObject, _mesh);
            for (var i = 0; i < _mesh.triangles.Length; i += 3)
            {
                var objVert1 = srcObject.transform.TransformPoint(srcVerts[srcEbo[i]]);
                var objVert2 = srcObject.transform.TransformPoint(srcVerts[srcEbo[i + 1]]);
                var objVert3 = srcObject.transform.TransformPoint(srcVerts[srcEbo[i + 2]]);

                var isFirstLower = IsPointLower(objVert1, _slicerPoint, _slicerNormal);
                var isSecondLower = IsPointLower(objVert2, _slicerPoint, _slicerNormal);
                var isThirdLower = IsPointLower(objVert3, _slicerPoint, _slicerNormal);

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
                        CreateTriangle(interLow, i, objVert1, objVert2, objVert3, isFirstLower, isSecondLower, isThirdLower);
                    
                    if (shouldDisplayUpperSide)
                        CreateTriangle(interUp, i, objVert1, objVert2, objVert3, !isFirstLower, !isSecondLower, !isThirdLower);
                }
            }

            if (shouldDisplayLowerSide)
            {
                _lowerObj = Instantiate(srcObject);
                _lowerObj.GetComponent<MeshFilter>().sharedMesh = interLow.CreateMesh();
                lowerMesh.triangles = lowerEbo.ToArray();
                srcObject.GetComponent<MeshFilter>().sharedMesh = lowerMesh;
            }

            if (shouldDisplayUpperSide)
            {
                _upperObj = Instantiate(srcObject);
                _upperObj.GetComponent<MeshFilter>().sharedMesh = interUp.CreateMesh();

                upperMesh.triangles = upperEbo.ToArray();
                srcObject.GetComponent<MeshFilter>().sharedMesh = upperMesh;
            }
        }

        private void CreateTriangle(Intersector inter, int i, Vector3 objVert1, Vector3 objVert2, Vector3 objVert3,  bool isFirstLower, bool isSecondLower, bool isThirdLower)
        {
            if (isFirstLower && !isSecondLower && !isThirdLower)
            {
                inter.CreateTriangleOnePointOnLeft(objVert1, objVert2, objVert3, i, 0, 1, 2);
            }
            else if (isSecondLower && !isFirstLower && !isThirdLower)
            {
                inter.CreateTriangleOnePointOnLeft(objVert2, objVert1, objVert3, i, 1, 0, 2);
            }
            else if (isThirdLower && !isFirstLower && !isSecondLower)
            {
                inter.CreateTriangleOnePointOnLeft(objVert3, objVert1, objVert2, i, 2, 0, 1);
            }
            else if (isFirstLower && isSecondLower && !isThirdLower)
            {
                inter.CreateTrianglesTwoPointsOnLeft(objVert1, objVert2, objVert3, i, 0, 1, 2);
            }
            else if (!isFirstLower && isSecondLower && isThirdLower)
            {
                inter.CreateTrianglesTwoPointsOnLeft(objVert2, objVert3, objVert1, i, 1, 2, 0);
            }
            else if (isFirstLower && !isSecondLower && isThirdLower)
            {
                inter.CreateTrianglesTwoPointsOnLeft(objVert1, objVert3, objVert2, i, 0, 2, 1);
            }
        }
        private void Start()
        {
            _mesh = srcObject.GetComponent<MeshFilter>().sharedMesh;
            _slicerMesh = slicerQuad.GetComponent<MeshFilter>().sharedMesh;
            Slice();
            _prevSlicerPos = slicerQuad.transform.position;
            _prevSlicerRotation = slicerQuad.transform.rotation.eulerAngles;
        }

        private void Update()
        {
            if ((slicerQuad.transform.position - _prevSlicerPos).magnitude > 0.001f ||
                (slicerQuad.transform.rotation.eulerAngles - _prevSlicerRotation).magnitude > 0.001f)
            {
                Destroy(_lowerObj);
                Destroy(_upperObj);
                Slice();
            }

            _prevSlicerPos = slicerQuad.transform.position;
            _prevSlicerRotation = slicerQuad.transform.eulerAngles;
        }
    }
}