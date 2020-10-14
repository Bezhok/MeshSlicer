using System.Collections.Generic;
using UnityEngine;

namespace src
{
    public class Main : MonoBehaviour
    {
        private Mesh _mesh;

        private Vector3 _prevSlicerPos;
        private Vector3 _prevSlicerRotation;
        private Mesh _slicerMesh;

        private Vector3 _slicerNormal;
        private Vector3 _slicerPoint;
    
        private List<Slicer> _slicers;

        public bool shouldDisplayLowerSide = true;
        public bool shouldDisplayUpperSide = false;

        [SerializeField] private GameObject slicerQuad;
        [SerializeField] private GameObject srcObject;

        private void Start()
        {
            Component[] renderers = srcObject.GetComponentsInChildren(typeof(MeshFilter), true);

            _slicerMesh = slicerQuad.GetComponent<MeshFilter>().sharedMesh;
            _slicerNormal = slicerQuad.transform.TransformDirection(_slicerMesh.normals[0]);
            _slicerPoint = slicerQuad.transform.TransformPoint(_slicerMesh.vertices[0]);
            _slicers = new List<Slicer>();

            foreach (var component in renderers)
            {
                var meshRenderer = (MeshFilter) component;
                if (meshRenderer.mesh != null && meshRenderer.mesh.triangles != null &&
                    meshRenderer.mesh.triangles.Length != 0)
                {
                    var slicer = new Slicer(meshRenderer.sharedMesh, meshRenderer.gameObject);
                    _slicers.Add(slicer);
                    slicer.Slice(_slicerNormal, _slicerPoint, shouldDisplayLowerSide, shouldDisplayUpperSide);
                }
            }

            _prevSlicerPos = slicerQuad.transform.position;
            _prevSlicerRotation = slicerQuad.transform.rotation.eulerAngles;
        }

        private void Update()
        {
            if ((slicerQuad.transform.position - _prevSlicerPos).magnitude > 0.001f ||
                (slicerQuad.transform.rotation.eulerAngles - _prevSlicerRotation).magnitude > 0.001f)
            {
                _slicerNormal = slicerQuad.transform.TransformDirection(_slicerMesh.normals[0]);
                _slicerPoint = slicerQuad.transform.TransformPoint(_slicerMesh.vertices[0]);

                foreach (var slicer in _slicers)
                {
                    slicer.Destroy();
                    slicer.Slice(_slicerNormal, _slicerPoint, shouldDisplayLowerSide, shouldDisplayUpperSide);
                }
            }

            _prevSlicerPos = slicerQuad.transform.position;
            _prevSlicerRotation = slicerQuad.transform.eulerAngles;
        }
    }
}