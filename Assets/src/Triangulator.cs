using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

namespace src
{
    public class Triangulator
    {
        private readonly HashSet<LinkedListNode<MappedPoint>> _convexVerts = new HashSet<LinkedListNode<MappedPoint>>();
        private readonly HashSet<LinkedListNode<MappedPoint>> _earsVerts = new HashSet<LinkedListNode<MappedPoint>>();
        private readonly LinkedList<MappedPoint> _pointsN;
        private readonly HashSet<LinkedListNode<MappedPoint>> _reflexVerts = new HashSet<LinkedListNode<MappedPoint>>();
        private readonly List<Triangle> _triangles = new List<Triangle>();

        public Triangulator(List<MappedPoint> points)
        {
            _pointsN = new LinkedList<MappedPoint>(points);
            if (!ArePointsCounterClockwise(_pointsN))
            {
                points.Reverse();
                _pointsN = new LinkedList<MappedPoint>(points);
            }
        }

        public Triangulator(SliceHierarchy sliceHierarchy)
        {
            //even should be counterclockwise
            ReverseIfShould(sliceHierarchy);

            sliceHierarchy.Childs.RemoveAll(item => item == null);
            sliceHierarchy.Childs = new List<SliceHierarchy>(sliceHierarchy.Childs.OrderByDescending(x => x.MaxX));

            for (var j = 0; j < sliceHierarchy.Childs.Count; j++)
            {
                var child = sliceHierarchy.Childs[j];
                if (child == null) continue;

                //odd should be clockwise
                ReverseIfShould(child);

                // child point with max x
                var childMaxX = child.MaxX;
                LinkedListNode<MappedPoint> childMaxXObj = null;

                for (var p = child.Points.First; p != null; p = p.Next)
                    if (p.Value.v2.x == childMaxX)
                        childMaxXObj = p;

                //intersected edges
                var interEdges = IntersectedEdges(childMaxXObj.Value.v2, child.Father, new Vector2(1000000f, 0));
                if (!interEdges.Any()) continue;

                // nearest edge
                var nearestEdgeDist = interEdges.Min(x => Mathf.Abs(x.MaxXdiscret.Value.v2.x - childMaxXObj.Value.v2.x));
                Edge nearestEdge = null;
                foreach (var e in interEdges)
                    if (Mathf.Abs(e.MaxXdiscret.Value.v2.x - childMaxXObj.Value.v2.x) == nearestEdgeDist)
                    {
                        nearestEdge = e;
                        break;
                    }


                // childMaxX..............................nearestEdge.Inter
                // .................MinAnglePoint.........
                // .................................P2....
                // .......................................nearestEdge.discret
                var pointsInTriangle = PointsInTriangle(
                    child.Father.Points,
                    childMaxXObj.Value.v2,
                    nearestEdge.MaxXdiscret.Value.v2,
                    nearestEdge.Inter);


                // if points were caught than should pick with smallest angle
                if (pointsInTriangle.Any())
                {
                    var minAngleObj = pointsInTriangle
                        .OrderBy(x => 
                            Vector2.Angle(nearestEdge.Inter - childMaxXObj.Value.v2, x.Value.v2 - childMaxXObj.Value.v2))
                        .ThenBy(x => Mathf.Abs(x.Value.v2.x - childMaxXObj.Value.v2.x)).First();

                    nearestEdge.MaxXdiscret = minAngleObj;
                }
                
                //intersected edges
                var selfInterEdges = IntersectedEdges(childMaxXObj.Value.v2, child, nearestEdge.MaxXdiscret.Value.v2);
                if (selfInterEdges.Any())
                    childMaxXObj = selfInterEdges
                        .OrderBy(x => Vector2.Distance(x.MaxXdiscret.Value.v2, nearestEdge.MaxXdiscret.Value.v2))
                        .First()
                        .MaxXdiscret;

                // union father and child
                UnionFatherAndChildPoints(
                    child,
                    child.Father,
                    childMaxXObj,
                    nearestEdge.MaxXdiscret);

                sliceHierarchy.Childs[j] = null;
            }

            _pointsN = sliceHierarchy.Points;
        }

        private List<Edge> IntersectedEdges(Vector2 edgeStart, SliceHierarchy father, Vector2 edgeEnd)
        {
            var interEdges = new List<Edge>();

            for (var p = father.Points.First; p != null; p = p.Next)
            {
                if (p.Value.v2.Equals(edgeStart)) continue;
                
                var areIntersect = WholeSlicePlane.LineSegmentsIntersection(
                    edgeStart,
                    edgeEnd,
                    p.Value.v2,
                    LinkedListExtensions.Next(p).Value.v2, out var interPoint);

                //intersection point with father(outer) points
                if (areIntersect)
                {
                    var point = p.Value.v2.x > LinkedListExtensions.Next(p).Value.v2.x ? p : LinkedListExtensions.Next(p);

                    var edge = new Edge {FDiscret = p, SDiscret = LinkedListExtensions.Next(p), MaxXdiscret = point, Inter = interPoint};
                    interEdges.Add(edge);
                }
            }

            return interEdges;
        }

        private List<LinkedListNode<MappedPoint>> PointsInTriangle(LinkedList<MappedPoint> points, Vector2 p0,
            Vector2 p1, Vector2 p2)
        {
            var pointsInTriangle = new List<LinkedListNode<MappedPoint>>();

            for (var p = points.First; p != null; p = p.Next)
            {
                var isPointInTriangle = IsPointInTriangle(p.Value.v2, p0, p1, p2);
                if (isPointInTriangle) pointsInTriangle.Add(p);
            }

            return pointsInTriangle;
        }

        private void ReverseIfShould(SliceHierarchy sliceHierarchy)
        {
            if (sliceHierarchy.Level % 2 == 0)
            {
                if (!ArePointsCounterClockwise(sliceHierarchy.Points))
                    sliceHierarchy.Points = LinkedListExtensions.ReversedLinkedList(sliceHierarchy.Points);
            }
            else
            {
                if (ArePointsCounterClockwise(sliceHierarchy.Points))
                    sliceHierarchy.Points = LinkedListExtensions.ReversedLinkedList(sliceHierarchy.Points);
            }
        }

        private void UnionFatherAndChildPoints(SliceHierarchy child, SliceHierarchy father,
            LinkedListNode<MappedPoint> childStartPoint, LinkedListNode<MappedPoint> fatherStartPoint)
        {
            var prev = fatherStartPoint;
            for (var p3 = childStartPoint; p3 != LinkedListExtensions.Previous(childStartPoint); p3 = LinkedListExtensions.Next(p3))
                prev = father.Points.AddAfter(prev, p3.Value);

            prev = father.Points
                .AddAfter(prev, LinkedListExtensions.Previous(childStartPoint).Value);
            prev = father.Points.AddAfter(prev, childStartPoint.Value);
            father.Points.AddAfter(prev, fatherStartPoint.Value);

            father.MaxX = Mathf.Max(father.MaxX, child.MaxX);
        }
        

        private bool ArePointsCounterClockwise(LinkedList<MappedPoint> testPoints)
        {
            float area = 0;

            for (var p = testPoints.First; p != testPoints.Last; p = p.Next)
                area += (p.Next.Value.v2.x - p.Value.v2.x) * (p.Next.Value.v2.y + p.Value.v2.y);

            area += (testPoints.First.Value.v2.x - testPoints.Last.Value.v2.x) *
                    (testPoints.First.Value.v2.y + testPoints.Last.Value.v2.y);

            return area < 0;
        }

        public List<Triangle> Triangulate()
        {
            for (var p = _pointsN.First; p != null; p = p.Next)
                if (IsVertConvex(LinkedListExtensions.Previous(p).Value, p.Value, LinkedListExtensions.Next(p).Value))
                    _convexVerts.Add(p);
                else
                    _reflexVerts.Add(p);

            foreach (var node in _convexVerts)
                if (ShouldAddConvexToEar(node))
                    _earsVerts.Add(node);

            while (_earsVerts.Any()) CreateTriangle();

            return _triangles;
        }

        private void CreateTriangle()
        {
            var node = _earsVerts.First();
            var prevNode = LinkedListExtensions.Previous(node);
            var nextNode = LinkedListExtensions.Next(node);

            _earsVerts.Remove(node);
            _triangles.Add(new Triangle(prevNode.Value, node.Value, nextNode.Value));
            _pointsN.Remove(node);

            ////// update convex and reflex and points
            if (_convexVerts.Contains(prevNode))
                UpdateSiblingConvexDeleting(prevNode);

            if (_convexVerts.Contains(nextNode))
                UpdateSiblingConvexDeleting(nextNode);

            if (_convexVerts.Contains(node))
                _convexVerts.Remove(node);

            if (_reflexVerts.Contains(prevNode))
                UpdateSiblingReflexDeleting(prevNode);

            if (_reflexVerts.Contains(nextNode))
                UpdateSiblingReflexDeleting(nextNode);
        }

        private void UpdateSiblingConvexDeleting(LinkedListNode<MappedPoint> node)
        {
            if (!IsVertConvex(
                LinkedListExtensions.Previous(node).Value,
                node.Value,
                LinkedListExtensions.Next(node).Value))
            {
                _earsVerts.Remove(node);
                _reflexVerts.Add(node);
                _convexVerts.Remove(node);
            }
            else
            {
                if (!ShouldAddConvexToEar(node))
                {
                    _earsVerts.Remove(node);
                }
                else
                {
                    if (!_earsVerts.Contains(node)) _earsVerts.Add(node);
                }
            }
        }

        private void UpdateSiblingReflexDeleting(LinkedListNode<MappedPoint> node)
        {
            if (IsVertConvex(
                LinkedListExtensions.Previous(node).Value,
                node.Value,
                LinkedListExtensions.Next(node).Value))
            {
                _convexVerts.Add(node);
                if (ShouldAddConvexToEar(node)) _earsVerts.Add(node);
                _reflexVerts.Remove(node);
            }
        }

        public bool IsVertConvex(MappedPoint prev, MappedPoint curr, MappedPoint next)
        {
            float area = 0;
            
                area += (next.v2.x - curr.v2.x) * (next.v2.y + curr.v2.y);
                area += (curr.v2.x - prev.v2.x) * (curr.v2.y + prev.v2.y);
                area += (prev.v2.x - next.v2.x) * (prev.v2.y + next.v2.y);
            
            return area < 0;
        }

        private bool ShouldAddConvexToEar(LinkedListNode<MappedPoint> node)
        {
            for (var p = _pointsN.First; p != null; p = p.Next)
                if (
                    !LinkedListExtensions.Previous(node).Value.v2.Equals(p.Value.v2) &&
                    !node.Value.v2.Equals(p.Value.v2) &&
                    !LinkedListExtensions.Next(node).Value.v2.Equals(p.Value.v2)
                )
                    if (IsPointInTriangle(
                        p.Value,
                        LinkedListExtensions.Previous(node).Value,
                        node.Value,
                        LinkedListExtensions.Next(node).Value)
                    )
                        return false;

            return true;
        }

        private bool IsPointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            var s = p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y;
            var t = p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y;

            if (s < 0 != t < 0)
                return false;

            var a = -p1.y * p2.x + p0.y * (p2.x - p1.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y;

            return a < 0 ? s <= 0 && s + t >= a : s >= 0 && s + t <= a;
        }

        private bool IsPointInTriangle(MappedPoint mp, MappedPoint mp0, MappedPoint mp1, MappedPoint mp2)
        {
            var p0 = mp0.v2;
            var p = mp.v2;
            var p1 = mp1.v2;
            var p2 = mp2.v2;

            return IsPointInTriangle(p, p0, p1, p2);
        }

        private class Edge
        {
            public LinkedListNode<MappedPoint> FDiscret, SDiscret, MaxXdiscret;
            public Vector2 Inter;
        }
    }
}