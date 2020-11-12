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

        public static bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4,
            out Vector2 intersection)
        {
            intersection = Vector2.zero;

            var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

            if (d == 0.0f) return false;

            var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
            var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

            if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f) return false;

            intersection.x = p1.x + u * (p2.x - p1.x);
            intersection.y = p1.y + u * (p2.y - p1.y);

            return true;
        }

        private int IntersectionCount(SliceHierarchy outerPoints, MappedPoint point)
        {
            var intersectionCount = 0;
            for (var p = outerPoints.Points.First; p != null; p = p.Next)
            {
                var end = new Vector2(1000000f, 0);

                var areIntersect = LineSegmentsIntersection(point.v2,
                    end,
                    p.Value.v2,
                    LinkedListExtensions.Next(p).Value.v2, out var intersection);

                if (areIntersect)
                    intersectionCount++;
            }

            return intersectionCount;
        }

        private void FilterSubSlices()
        {
            for (var i = 0; i < _subSlices.Count; i++)
            {
                if (_subSlices[i] == null) continue;

                // cant form triangle
                if (_subSlices[i].Points.Count <= 3) _subSlices[i] = null;

                // is not looped
                // bool isClosedLoop = _subSlices[i]._points.Last.Value.Equals(_subSlices[i]._points.First.Value);
                // if (!isClosedLoop)
                // {
                // _subSlices[i] = null;
                // }
            }
        }

        private bool IsPolygonInsidePolygon(SliceHierarchy intendedСhild, SliceHierarchy intendedFather)
        {
            var intersectionCountPrev = IntersectionCount(intendedFather, intendedСhild.Points.First.Value);
            return intersectionCountPrev % 2 == 1;
        }

        private bool ArePolygonsIntersect(SliceHierarchy pol1, SliceHierarchy pol2)
        {
            for (var p1 = pol1.Points.First; p1 != null; p1 = p1.Next)
            for (var p2 = pol2.Points.First; p2 != null; p2 = p2.Next)
            {
                var areIntersect = LineSegmentsIntersection(p1.Value.v2, LinkedListExtensions.Next(p1).Value.v2, p2.Value.v2,
                    LinkedListExtensions.Next(p2).Value.v2, out var interPoint);

                if (areIntersect)
                    return true;
            }

            return false;
        }

        public List<List<Triangle>> CreateSlicePlane()
        {
            var slices = new List<List<Triangle>>();
            FilterSubSlices();
            _subSlices.RemoveAll(item => item == null);

            // calc max for each slice
            _subSlices.ForEach(sub => sub.MaxX = sub.Points.Max(x => x.v2.x));

            foreach (var intendedChild in _subSlices)
            {
                if (intendedChild.IsIntersected) continue;
                foreach (var intendedFather in _subSlices)
                {
                    if (intendedFather.IsIntersected) continue;
                    if (intendedChild == intendedFather) continue;
                    if (ArePolygonsIntersect(intendedChild, intendedFather))
                    {
                        intendedChild.IsIntersected = true;
                        intendedFather.IsIntersected = true;

                        intendedChild.Level = 0;
                        intendedFather.Level = 0;
                    }
                    else if (IsPolygonInsidePolygon(intendedChild, intendedFather))
                    {
                        intendedChild.Level++;
                        intendedFather.Childs.Add(intendedChild);
                    }
                }
            }
            
            _subSlices.ForEach(x =>
            {
                if (x.Points.Last.Value.Equals(x.Points.First.Value)) x.Points.RemoveLast();
            });
            
            RearrangeSlices();

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

        private void RearrangeSlices()
        {
            for (var i = 0; i < _subSlices.Count; i++)
            {
                // even is outer
                if (_subSlices[i].Level % 2 != 0)
                {
                    _subSlices[i] = null;
                    continue;
                }

                var sl = _subSlices[i];
                for (var j = 0; j < sl.Childs.Count; j++)
                {
                    if (sl.Childs[j] == null) continue;

                    // if intersects than should be outer
                    if (sl.Childs[j].IsIntersected)
                    {
                        sl.Childs[j] = null;
                        continue;
                    }

                    // odd is inner
                    if (sl.Childs[j].Level % 2 == 1 &&
                        sl.Childs[j].Level - sl.Level == 1)
                    {
                        sl.Childs[j].Father = sl;
                    }
                    else
                    {
                        sl.Childs[j] = null;
                    }
                }
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
                p.Points.RemoveFirst();
                while (p.Points.Any())
                {
                    _subSlices[idx].Points.AddFirst(p.Points.First.Value);
                    p.Points.RemoveFirst();
                }

                _subSlices[i] = null;
            }
            else
            {
                p.Points.RemoveFirst();
                while (p.Points.Any())
                {
                    _subSlices[idx].Points.AddLast(p.Points.First.Value);
                    p.Points.RemoveFirst();
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
                _subSlices[idx].Points.RemoveFirst();
                while (_subSlices[idx].Points.Any())
                {
                    p.Points.AddLast(_subSlices[idx].Points.First.Value);
                    _subSlices[idx].Points.RemoveFirst();
                }

                _subSlices[idx] = null;
            }
            else
            {
                _subSlices[idx].Points.RemoveLast();
                while (_subSlices[idx].Points.Any())
                {
                    p.Points.AddLast(_subSlices[idx].Points.Last.Value);
                    _subSlices[idx].Points.RemoveLast();
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

                if (sl.Points.First.Value.v2.Equals(point1M.v2))
                {
                    if (!isLinked)
                    {
                        sl.Points.AddFirst(point2M);
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
                else if (sl.Points.First.Value.v2.Equals(point2M.v2))
                {
                    if (!isLinked)
                    {
                        sl.Points.AddFirst(point1M);
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
                else if (sl.Points.Last.Value.v2.Equals(point1M.v2))
                {
                    if (!isLinked)
                    {
                        sl.Points.AddLast(point2M);
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
                else if (sl.Points.Last.Value.v2.Equals(point2M.v2))
                {
                    if (!isLinked)
                    {
                        sl.Points.AddLast(point1M);
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
                _subSlices[_subSlices.Count - 1].Points.AddFirst(point1M);
                _subSlices[_subSlices.Count - 1].Points.AddFirst(point2M);
            }
        }
    }
}