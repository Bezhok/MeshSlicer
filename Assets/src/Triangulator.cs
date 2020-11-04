using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace src
{
    public class Triangulator
    {
        private class Edge
        {
            public LinkedListNode<MappedPoint> f, s, discret;
            public Vector2 inter;
        }
        
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

        private List<Edge> IntersectedEdges(Vector2 child, SliceHierarchy father)
        {
            var interEdges = new List<Edge>();
            
            //ray
            var end = new Vector2(father.maxX + 100, child.y);
            for (var p = father._points.First; p != null; p = p.Next)
            {
                var interPoint = WholeSlicePlane.LinesIntersection(
                    child,
                    end,
                    p.Value.v2,
                    Next(p).Value.v2);
                    
                //intersection point with father(outer) points
                if (WholeSlicePlane.IsPointOnSection(interPoint, p.Value.v2, Next(p).Value.v2) &&
                    WholeSlicePlane.IsPointOnSection(interPoint, child, end))
                {
                    LinkedListNode<MappedPoint> point = null;
                    if (p.Value.v2.x > Next(p).Value.v2.x)
                        point = p;
                    else
                        point = Next(p);

                    var edge = new Edge {f = point, s = point, discret = p, inter = interPoint};
                    interEdges.Add(edge);
                }
            }

            return interEdges;
        }

        private List<LinkedListNode<MappedPoint>> PointsInTriangle(LinkedList<MappedPoint> points,  Vector2 p0, Vector2 p1, Vector2 p2)
        {
            var pointsInTriangle = new List<LinkedListNode<MappedPoint>>();

            for (var p = points.First; p != null; p = p.Next)
            {
                var isPointInTriangle = IsPointInTriangle(p.Value.v2, p0, p1, p2);
                if (isPointInTriangle)
                {
                    pointsInTriangle.Add(p);
                }
            }

            return pointsInTriangle;
        }

        private void ReverseIfShould(SliceHierarchy sliceHierarchy)
        {
            if (sliceHierarchy.level % 2 == 0)
            {
                if (!ArePointsCounterClockwise(sliceHierarchy._points))
                {
                    sliceHierarchy._points = ReversedLinkedList(sliceHierarchy._points);
                }
            }
            else
            {
                if (ArePointsCounterClockwise(sliceHierarchy._points))
                {
                    sliceHierarchy._points = ReversedLinkedList(sliceHierarchy._points);
                }
            }
        }
        
        public Triangulator(SliceHierarchy sliceHierarchy)
        {
            //even should be counterclockwise
            ReverseIfShould(sliceHierarchy);

            sliceHierarchy.childs.RemoveAll(item => item == null);
            sliceHierarchy.childs = new List<SliceHierarchy>(sliceHierarchy.childs.OrderByDescending(x => x.maxX));

            for (var j = 0; j < sliceHierarchy.childs.Count; j++)
            {
                var child = sliceHierarchy.childs[j];
                if (child == null) continue;

                //odd should be clockwise
                ReverseIfShould(child);

                // child point with max x
                var childMaxX = child.maxX;
                LinkedListNode<MappedPoint> childMaxXObj = null;
                for (var p = child._points.First; p != null; p = p.Next)
                    if (p.Value.v2.x == childMaxX) { childMaxXObj = p; break;}
                
                //intersected edges
                var interEdges = IntersectedEdges(childMaxXObj.Value.v2, child.father);
                if(!interEdges.Any()) continue;

                // nearest edge
                var nearestEdgeDist = interEdges.Min(x => Mathf.Abs(x.discret.Value.v2.x - childMaxXObj.Value.v2.x));
                Edge nearestEdge = null;
                for (var i = 0; i < interEdges.Count; i++)
                    if (Mathf.Abs(interEdges[i].discret.Value.v2.x - childMaxXObj.Value.v2.x)==nearestEdgeDist)
                    { nearestEdge = interEdges[i]; break; }
                
                // childMaxX..............................nearestEdge.Inter
                // .................MinAnglePoint.........
                // .................................P2....
                // .......................................nearestEdge.discret
                var pointsInTriangle = PointsInTriangle(
                    child.father._points,
                    childMaxXObj.Value.v2,
                    nearestEdge.discret.Value.v2,
                    nearestEdge.inter);
                
                // if points were catched than should pick with smallest angle
                if (pointsInTriangle.Any())
                {
                    var minAngleObj = pointsInTriangle.OrderBy(x =>
                            Vector2.Angle(nearestEdge.inter - childMaxXObj.Value.v2,
                            x.Value.v2 - childMaxXObj.Value.v2)
                    ).ThenBy(x => 
                        Mathf.Abs(x.Value.v2.x - childMaxXObj.Value.v2.x)).First();

                     nearestEdge.discret = minAngleObj;
                }

                // union father and child
                UnionFatherAndChildPoints(
                    child,
                    child.father,
                    childMaxXObj,
                    nearestEdge.discret);
                
                sliceHierarchy.childs[j] = null;
            }

            _pointsN = sliceHierarchy._points;
        }
        
        private void UnionFatherAndChildPoints(SliceHierarchy child, SliceHierarchy father, LinkedListNode<MappedPoint> childStartPoint, LinkedListNode<MappedPoint> fatherStartPoint)
        {
            var prev = fatherStartPoint;
            for (var p3 = childStartPoint; p3 != Previous(childStartPoint); p3 = Next(p3))
                prev = father._points.AddAfter(prev, p3.Value);

            prev = father._points
                .AddAfter(prev, Previous(childStartPoint).Value);
            prev = father._points.AddAfter(prev, childStartPoint.Value);
            prev = father._points.AddAfter(prev, fatherStartPoint.Value);

            father.maxX =
                father._points.Max(x => x.v2.x);
        }
        
        public static LinkedList<T> ReversedLinkedList<T>(LinkedList<T> linkedList)
        {
            var copyList = new LinkedList<T>();
            var start = linkedList.Last;
            while (start != null)
            {
                copyList.AddLast(start.Value);
                start = start.Previous;
            }

            return copyList;
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
                if (IsVertConvex(Previous(p).Value, p.Value, Next(p).Value))
                    _convexVerts.Add(p);
                else
                    _reflexVerts.Add(p);

            foreach (var node in _convexVerts)
                if (ShouldAddConvexToEar(node))
                    _earsVerts.Add(node);

            while (_earsVerts.Any())
            {
                var node = _earsVerts.First();
                var prevNode = Previous(node);
                var nextNode = Next(node);

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

            return _triangles;
        }

        private void UpdateSiblingConvexDeleting(LinkedListNode<MappedPoint> node)
        {
            if (!IsVertConvex(
                Previous(node).Value,
                node.Value,
                Next(node).Value)
            )
            {
                _earsVerts.Remove(node);
                _reflexVerts.Add(node);
                _convexVerts.Remove(node);
            }
            else
            {
                if (!ShouldAddConvexToEar(node)) _earsVerts.Remove(node);
                else _earsVerts.Add(node);
            }
        }

        private void UpdateSiblingReflexDeleting(LinkedListNode<MappedPoint> node)
        {
            if (IsVertConvex(
                Previous(node).Value,
                node.Value,
                Next(node).Value))
            {
                _convexVerts.Add(node);
                if (ShouldAddConvexToEar(node)) _earsVerts.Add(node);
                _reflexVerts.Remove(node);
            }
        }

        public static LinkedListNode<MappedPoint> Next(LinkedListNode<MappedPoint> n)
        {
            if (n.List.Last == n)
                return n.List.First;
            return n.Next;
        }

        public static LinkedListNode<MappedPoint> Previous(LinkedListNode<MappedPoint> n)
        {
            if (n.List.First == n)
                return n.List.Last;
            return n.Previous;
        }

        public static bool IsVertConvex(MappedPoint prev, MappedPoint curr, MappedPoint next)
        {
            return prev.v2.x * (next.v2.y - curr.v2.y) + curr.v2.x * (prev.v2.y - next.v2.y) +
                next.v2.x * (curr.v2.y - prev.v2.y) < 0;
        }

        public static bool IsVertConvex(Vector2 prev, Vector2 curr, Vector2 next)
        {
            return prev.x * (next.y - curr.y) + curr.x * (prev.y - next.y) + next.x * (curr.y - prev.y) < 0;
        }

        public static bool IsVertConvex(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return p1.x * p2.y * p3.z + p1.y * p2.z * p3.x + p1.z * p2.x * p3.y - p1.z * p2.y * p3.x -
                p1.x * p2.z * p3.y - p1.y * p2.x * p3.z > 0;
        }

        private bool ShouldAddConvexToEar(LinkedListNode<MappedPoint> node)
        {
            for (var p = _pointsN.First; p != null; p = p.Next)
                if (
                    !Previous(node).Value.v2.Equals(p.Value.v2) &&
                    !node.Value.v2.Equals(p.Value.v2) &&
                    !Next(node).Value.v2.Equals(p.Value.v2)
                )
                    if (IsPointInTriangle(
                        p.Value,
                        Previous(node).Value,
                        node.Value,
                        Next(node).Value)
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

            var s = p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y;
            var t = p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y;

            if (s < 0 != t < 0)
                return false;

            var a = -p1.y * p2.x + p0.y * (p2.x - p1.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y;

            return a < 0 ? s <= 0 && s + t >= a : s >= 0 && s + t <= a;
        }
    }
}