using System.Collections.Generic;
using UnityEngine;

namespace src
{
    public class UpdatableSlicer
    {
        private readonly List<Slicer> _slicers;

        public UpdatableSlicer(GameObject srcObject)
        {
            var renderers = srcObject.GetComponentsInChildren(typeof(MeshFilter), true);

            _slicers = new List<Slicer>();

            for (var i = 0; i < renderers.Length; i++)
            {
                var meshRenderer = (MeshFilter) renderers[i];
                if (meshRenderer.mesh != null && meshRenderer.mesh.triangles != null &&
                    meshRenderer.mesh.triangles.Length != 0)
                {
                    var slicer = new Slicer(meshRenderer.sharedMesh, renderers[i].gameObject);
                    _slicers.Add(slicer);
                }
            }
        }

        public void Update(Vector3 slicerPoint, Vector3 slicerNormal, bool shouldDisplayLowerSide)
        {
            foreach (var slicer in _slicers)
            {
                slicer.Destroy();
                slicer.UpdatableSlice(slicerNormal, slicerPoint, shouldDisplayLowerSide);
            }
        }
    }
}