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

        public static Vector2 LinesIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            var va = a1 - a2;
            var vb = b1 - b2;

            var u = (b1.x - a1.x + vb.x / vb.y * a1.y - b1.y * vb.x / vb.y) / (va.x - vb.x);
            return a1 + u * va;
        }

        public static Vector3 LinesIntersection(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
        {
            var va = a1 - a2;
            var vb = b1 - b2;

            var u = (b1.x - a1.x + vb.x / vb.y * a1.y - b1.y * vb.x / vb.y) / (va.x - vb.x);
            return a1 + u * va;
        }
        public static bool IsPointOnSection(Vector2 p, Vector2 start, Vector2 end)
        {
            return Vector2.Dot(start - p, end - p) < 0;
        }

        private int IntersectionCount(SliceHierarchy outerPoints, MappedPoint point)
        {
            int intersectionCount1 = 0;
            for (var p = outerPoints._points.First; p != null; p = p.Next)
            {
                var end = new Vector2(outerPoints.maxX + 1000f, point.v2.y+100);

                var va = point.v2 - end;
                var vb = p.Value.v2 - Triangulator.Next(p).Value.v2;

                // if not parallel lines
                float f1 = (va.normalized - vb.normalized).magnitude;
                float f2 = (va.normalized + vb.normalized).magnitude;
                if (va.normalized != vb.normalized && va.normalized != -vb.normalized)
                {
                    var interPoint = LinesIntersection(point.v2,
                        end,
                        p.Value.v2,
                        Triangulator.Next(p).Value.v2);

                    if (IsPointOnSection(interPoint, p.Value.v2, Triangulator.Next(p).Value.v2)
                        &&
                        IsPointOnSection(interPoint, point.v2, end)
                    )
                    {
                        intersectionCount1++;
                    }
                }
            }
            
            return intersectionCount1;
        }

        private void FilterSubSlices()
        {
            for (var i = 0; i < _subSlices.Count; i++)
            {
                if (_subSlices[i] == null) continue;

                // cant form triangle
                if (_subSlices[i]._points.Count <= 3)
                {
                    _subSlices[i] = null;
                    continue;
                }

                // is not looped
                bool isClosedLoop = _subSlices[i]._points.Last.Value.Equals(_subSlices[i]._points.First.Value);
                if (!isClosedLoop)
                {
                    _subSlices[i] = null;
                }
                
            }
        }

        private bool IsPolygonInsidePolygon(SliceHierarchy intendedСhild, SliceHierarchy intendedFather)
        {
            var intersectionCount1 = -1;
            foreach (var point in intendedСhild._points)
            {
                // var intersectionCount2 = IntersectionCount(intendedFather, p);
                //
                // if (intersectionCount1 != -1 && intersectionCount1!=intersectionCount2)
                // {//independent parts(including slices intersections
                //     intersectionCount1 = 0;
                //     break;
                // }
                //
                // intersectionCount1 = intersectionCount2;
                // it++;

                intersectionCount1 = Mathf.Max(intersectionCount1, IntersectionCount(intendedFather, point));
            }

            return intersectionCount1 % 2 == 1;
        }
        public List<List<Triangle>> CreateSlicePlane()
        {
            var slices = new List<List<Triangle>>();
            FilterSubSlices();
            _subSlices.RemoveAll(item => item == null);
            
            // calc max for each slice
            _subSlices.ForEach(sub=>sub.maxX = sub._points.Max(x => x.v2.x));

            foreach (var intendedChild in _subSlices)
            {
                foreach (var intendedFather in _subSlices)
                {
                    if (intendedChild == intendedFather) continue;
                    
                    if (IsPolygonInsidePolygon(intendedChild, intendedFather))
                    {
                        intendedChild.level++;
                        intendedFather.childs.Add(intendedChild);
                    }
                }   
            }

            _subSlices.ForEach(x=>x._points.RemoveLast());
            SliceHierarchyDFSStart();
            
            //////////////
            foreach (var sl in _subSlices)
            {
                if (sl == null) continue;
                var triangulator = new Triangulator(sl); 
                var triangles = triangulator.Triangulate(); 
                slices.Add(triangles);
            }
            
            /////////////////Test
            if (_subSlices.Any())
                Test.slices = _subSlices;

            return slices;
        }

        private void SliceHierarchyDFSStart()
        {
            for (var i = 0; i < _subSlices.Count; i++)
            {
                if (_subSlices[i].level != 0)
                {
                    _subSlices[i] = null;
                    continue;
                }

                SliceHierarchyDFS(_subSlices[i], 1);
            }
        }
        private void SliceHierarchyDFS(SliceHierarchy sl, int level)
        {
            for (var i = 0; i < sl.childs.Count; i++)
            {
                if (sl.childs[i] == null) continue;
                if (sl.childs[i].level != level)
                {
                    sl.childs[i] = null;
                    continue;
                }

                sl.childs[i].father = sl;
                SliceHierarchyDFS(sl.childs[i], level + 1);
            }
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
                        sl._points.AddFirst(point2M);
                        prevLinkedListIdx = i;
                        isLinked = true;
                        isFirst = true;
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
                        sl._points.AddFirst(point1M);
                        prevLinkedListIdx = i;
                        isLinked = true;
                        isFirst = true;
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
                        sl._points.AddLast(point2M);
                        prevLinkedListIdx = i;
                        isLinked = true;
                        isFirst = false;
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
                        sl._points.AddLast(point1M);
                        prevLinkedListIdx = i;
                        isLinked = true;
                        isFirst = false;
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