using System.Collections.Generic;
using UnityEngine;

namespace src
{
    public class OnetimeSlicer
    {
        private readonly List<Slicer> _slicers;
        private readonly GameObject _srcCloneLower;
        private readonly GameObject _srcCloneUpper;

        public OnetimeSlicer(GameObject srcObject)
        {
            _srcCloneLower = Object.Instantiate(srcObject);
            _srcCloneUpper = Object.Instantiate(srcObject);
            var renderersLower = _srcCloneLower.GetComponentsInChildren(typeof(MeshFilter), true);
            var renderersUpper = _srcCloneUpper.GetComponentsInChildren(typeof(MeshFilter), true);
            var renderersSrc = srcObject.GetComponentsInChildren(typeof(MeshFilter), true);
            
            _slicers = new List<Slicer>();
            for (var i = 0; i < renderersSrc.Length; i++)
            {
                var meshRenderer = (MeshFilter) renderersSrc[i];
                if (meshRenderer.mesh != null && meshRenderer.mesh.triangles != null &&
                    meshRenderer.mesh.triangles.Length != 0)
                {
                    var slicer = new Slicer(meshRenderer.sharedMesh, renderersSrc[i].gameObject, renderersLower[i].gameObject,
                        renderersUpper[i].gameObject);
                    _slicers.Add(slicer);
                }
            }

            srcObject.SetActive(false);
        }

        public void Slice(Vector3 slicerPoint, Vector3 slicerNormal, bool shouldDisplayLowerSide,
            bool shouldDisplayUpperSide)
        {
            if (!shouldDisplayLowerSide) Object.Destroy(_srcCloneLower);

            if (!shouldDisplayUpperSide) Object.Destroy(_srcCloneUpper);

            foreach (var slicer in _slicers)
                slicer.OneTimeSlice(slicerNormal, slicerPoint, shouldDisplayLowerSide, shouldDisplayUpperSide);
        }
    }
}