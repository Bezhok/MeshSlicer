using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace src
{
    public class WholeSlicePlane
    {
        private readonly List<SliceHierarchy> _subSlices = new List<SliceHierarchy>();
        private readonly Vector3 u;
        private readonly Vector3 v;

        public WholeSlicePlane(Vector3 planeNormalLocal)
        {
            u = Vector3.Normalize(Vector3.Cross(planeNormalLocal, Vector3.up));
            if (Vector3.zero == u) u = Vector3.Normalize(Vector3.Cross(planeNormalLocal, Vector3.forward));
            v = Vector3.Cross(u, planeNormalLocal);
        }

        public List<List<Triangle>> CreateSlicePlane()
        {
            List<List<Triangle>> slices = new List<List<Triangle>>();
            foreach (var sl in _subSlices)
            {
                if (sl == null) continue;
                sl._points.RemoveLast();
                var triangulator = new Triangulator(new List<MappedPoint>(sl._points));
                var triangles = triangulator.Triangulate();

                slices.Add(triangles);
            }
            
            Test.trisl.Clear();
            if (_subSlices.Any())
                Test._points11 = _subSlices;
            Test.trisl = new List<List<Triangle>>(slices);

            return slices;
        }

        // i: |......... to idx: |......... and
        // i: |......... to idx: .........|
        private void LinkListsFirst(int i, int idx, bool isFirst)
        {
            // idx == prevLinkedListIdx
            var p = _subSlices[i];
            if (isFirst)
            {
                p._points.RemoveFirst();
                while (p._points.Any())
                {
                    _subSlices[idx]._points.AddFirst(p._points.First.Value);
                    p._points.RemoveFirst();
                }

                _subSlices[i] = null;
            }
            else
            {
                p._points.RemoveFirst();
                while (p._points.Any())
                {
                    _subSlices[idx]._points.AddLast(p._points.First.Value);
                    p._points.RemoveFirst();
                }

                _subSlices[i] = null;
            }
        }

        // i: .........| to idx: |......... and
        // i: .........| to idx: .........|
        private void LinkListsLast(int i, int idx, bool isFirst)
        {
            // idx == prevLinkedListIdx
            var p = _subSlices[i];
            if (isFirst)
            {
                _subSlices[idx]._points.RemoveFirst();
                while (_subSlices[idx]._points.Any())
                {
                    p._points.AddLast(_subSlices[idx]._points.First.Value);
                    _subSlices[idx]._points.RemoveFirst();
                }

                _subSlices[idx] = null;
            }
            else
            {
                _subSlices[idx]._points.RemoveLast();
                while (_subSlices[idx]._points.Any())
                {
                    p._points.AddLast(_subSlices[idx]._points.Last.Value);
                    _subSlices[idx]._points.RemoveLast();
                }

                _subSlices[idx] = null;
            }
        }

        public void AddSlicePlanePoints(Vector3 point1, Vector3 point2)
        {
            var point1M = new MappedPoint(point1, u, v);
            var point2M = new MappedPoint(point2, u, v);

            var isLinked = false;
            var prevLinkedListIdx = -1;
            var isFirst = true;

            for (var i = 0; i < _subSlices.Count; i++)
            {
                var sl = _subSlices[i];
                if (sl == null) continue;

                if (sl._points.First.Value.v2.Equals(point1M.v2))
                {
                    if (!isLinked)
                    {
                        sl._points.AddFirst(point2M); prevLinkedListIdx = i; isLinked = true; isFirst = true;
                    }
                    else
                    {
                        LinkListsFirst(i, prevLinkedListIdx, isFirst);
                        break;
                    }
                }
                else if (sl._points.First.Value.v2.Equals(point2M.v2))
                {
                    if (!isLinked)
                    {
                        sl._points.AddFirst(point1M); prevLinkedListIdx = i; isLinked = true; isFirst = true;
                    }
                    else
                    {
                        LinkListsFirst(i, prevLinkedListIdx, isFirst);
                        break;
                    }
                }
                else if (sl._points.Last.Value.v2.Equals(point1M.v2))
                {
                    if (!isLinked)
                    {
                        sl._points.AddLast(point2M); prevLinkedListIdx = i; isLinked = true; isFirst = false;
                    }
                    else
                    {
                        LinkListsLast(i, prevLinkedListIdx, isFirst);
                        break;
                    }
                }
                else if (sl._points.Last.Value.v2.Equals(point2M.v2))
                {
                    if (!isLinked)
                    {
                        sl._points.AddLast(point1M); prevLinkedListIdx = i; isLinked = true; isFirst = false;
                    }
                    else
                    {
                        LinkListsLast(i, prevLinkedListIdx, isFirst);
                        break;
                    }
                }
            }

            if (!isLinked)
            {
                _subSlices.Add(new SliceHierarchy());
                _subSlices[_subSlices.Count - 1]._points.AddFirst(point1M);
                _subSlices[_subSlices.Count - 1]._points.AddFirst(point2M);
            }
        }
    }
}