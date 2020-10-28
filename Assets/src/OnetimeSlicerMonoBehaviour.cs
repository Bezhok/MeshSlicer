using System;
using UnityEngine;

namespace src
{
    public class OnetimeSlicerMonoBehaviour : MonoBehaviour
    {
        public bool shouldDisplayLowerSide = true;
        public bool shouldDisplayUpperSide;

        [SerializeField] private GameObject slicerQuad;
        [SerializeField] private GameObject srcObject;

        private void Start()
        {
            if (slicerQuad == null) throw new NullReferenceException("slicerQuad is null");
            if (srcObject == null) throw new NullReferenceException("srcObject is null");

            var slicerMesh = slicerQuad.GetComponent<MeshFilter>().sharedMesh;
            var slicerNormal = slicerQuad.transform.TransformDirection(slicerMesh.normals[0]);
            var slicerPoint = slicerQuad.transform.TransformPoint(slicerMesh.vertices[0]);

            var onetimeSlicer = new OnetimeSlicer(srcObject);
            onetimeSlicer.Slice(slicerPoint, slicerNormal, shouldDisplayLowerSide, shouldDisplayUpperSide);
        }
    }
}