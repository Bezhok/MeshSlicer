using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace src
{
    public class Slicer
    {
        private readonly Mesh _mesh;
        private readonly GameObject _srcObject;
        private GameObject _lowerObj;
        private GameObject _upperObj;
        
        public Slicer(Mesh mesh, GameObject gameObject,GameObject lowerObj,GameObject upperObj)
        {
            // one time slice
            _mesh = mesh;
            _srcObject = gameObject;
            _lowerObj = lowerObj;
            _upperObj = upperObj;
        }
        public Slicer(Mesh mesh, GameObject gameObject)
        {
            // updatable slice
            _mesh = mesh;
            _srcObject = gameObject;
        }
        public void Destroy()
        {
            Object.Destroy(_lowerObj);
            Object.Destroy(_upperObj);
        }

        public static bool IsPointLower(Vector3 point, Vector3 planePoint, Vector3 planeNormal)
        {
            return Vector3.Dot(planePoint - point, planeNormal) < 0;
        }

        public void OneTimeSlice(Vector3 slicerNormal, Vector3 slicerPoint, bool shouldDisplayLowerSide,
            bool shouldDisplayUpperSide)
        {
            if (_lowerObj == null || _upperObj == null)
            {
                throw new Exception("Your didn't pass lower and upper objects, but try to create onetime slice");    
            }
            
            Slice(slicerNormal, slicerPoint, shouldDisplayLowerSide, shouldDisplayUpperSide, out Intersector interLow, out Intersector interUp);
            
            if (shouldDisplayLowerSide) _lowerObj.GetComponent<MeshFilter>().sharedMesh = interLow.CreateMesh();
            // else _lowerObj.SetActive(false);//Object.Destroy(_lowerObj);
            
            if (shouldDisplayUpperSide) _upperObj.GetComponent<MeshFilter>().sharedMesh = interUp.CreateMesh();
            // else _upperObj.SetActive(false);//Object.Destroy(_upperObj);
        }

        public void UpdatableSlice(Vector3 slicerNormal, Vector3 slicerPoint, bool shouldDisplayLowerSide)
        {
            Slice(slicerNormal, slicerPoint, shouldDisplayLowerSide, !shouldDisplayLowerSide, out Intersector interLow, out Intersector interUp);

            if (shouldDisplayLowerSide) UpdateMesh(ref _lowerObj, interLow);
            else UpdateMesh(ref _upperObj, interUp);
        }
        
        private void Slice(Vector3 slicerNormal, Vector3 slicerPoint, bool shouldDisplayLowerSide,
            bool shouldDisplayUpperSide, out Intersector interLow, out Intersector interUp)
        {
            var srcVerts = _mesh.vertices;
            var srcEbo = _mesh.triangles;

            var lowerEbo = new List<int>();
            var upperEbo = new List<int>();

            interLow = new Intersector(slicerPoint, slicerNormal, _srcObject, _mesh, lowerEbo);
            interUp = new Intersector(slicerPoint, slicerNormal, _srcObject, _mesh, upperEbo);

            int len = _mesh.triangles.Length;
            for (var i = 0; i < len; i += 3)
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
        }
        
        private void UpdateMesh(ref GameObject borderObj, Intersector intersector)
        {
            borderObj = Object.Instantiate(_srcObject, _srcObject.transform.parent);
            foreach (Transform child in borderObj.transform) {
                MonoBehaviour.Destroy(child.gameObject);
            }
            
            borderObj.GetComponent<MeshFilter>().sharedMesh = intersector.CreateMesh();
            borderObj.GetComponent<Renderer>().enabled = true;

            borderObj.transform.position = _srcObject.transform.position;
            borderObj.transform.rotation = _srcObject.transform.rotation;
            borderObj.transform.localScale = _srcObject.transform.localScale;
            
            _srcObject.GetComponent<Renderer>().enabled = false;
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