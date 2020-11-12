using System;
using System.Collections.Generic;
using UnityEngine;

namespace src
{
    public class UpdatableSlicerMonoBehaviour : MonoBehaviour
    {
        private Vector3 _prevSlicerPos;
        private Vector3 _prevSlicerRotation;
        private Mesh _slicerMesh;

        public bool shouldDisplayLowerSide = true;

        [SerializeField] private GameObject slicerQuad;
        [SerializeField] private GameObject srcObject;

        private UpdatableSlicer _updatableSlicer;
        private void Start()
        {
            if (slicerQuad == null) throw new NullReferenceException("slicerQuad is null");
            if (srcObject == null) throw new NullReferenceException("srcObject is null");
            
            _slicerMesh = slicerQuad.GetComponent<MeshFilter>().sharedMesh;
            var slicerNormal = slicerQuad.transform.TransformDirection(_slicerMesh.normals[0]);
            var slicerPoint = slicerQuad.transform.TransformPoint(_slicerMesh.vertices[0]);
            
            _updatableSlicer = new UpdatableSlicer(srcObject);
            _updatableSlicer.Update(slicerPoint, slicerNormal, shouldDisplayLowerSide);

            _prevSlicerPos = slicerQuad.transform.position;
            _prevSlicerRotation = slicerQuad.transform.rotation.eulerAngles;

            Test.obj = srcObject;
        }

        private void Update()
        {
            if ((slicerQuad.transform.position - _prevSlicerPos).magnitude > 0.001f ||
                (slicerQuad.transform.rotation.eulerAngles - _prevSlicerRotation).magnitude > 0.001f)
            {
                Test.gizmos.Clear();
                var slicerNormal = slicerQuad.transform.TransformDirection(_slicerMesh.normals[0]);
                var slicerPoint = slicerQuad.transform.TransformPoint(_slicerMesh.vertices[0]);
                
                _updatableSlicer.Update(slicerPoint, slicerNormal, shouldDisplayLowerSide);
            }

            _prevSlicerPos = slicerQuad.transform.position;
            _prevSlicerRotation = slicerQuad.transform.eulerAngles;
        }
    }
}