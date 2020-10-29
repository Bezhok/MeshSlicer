using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace src
{
    public class Triangulator
    {
        private readonly HashSet<LinkedListNode<MappedPoint>> _convexVerts = new HashSet<LinkedListNode<MappedPoint>>();
        private readonly HashSet<LinkedListNode<MappedPoint>> _earsVerts = new HashSet<LinkedListNode<MappedPoint>>();
        private readonly HashSet<LinkedListNode<MappedPoint>> _reflexVerts = new HashSet<LinkedListNode<MappedPoint>>();
        private readonly List<Triangle> _triangles = new List<Triangle>();
        private readonly LinkedList<MappedPoint> _pointsN;

        public Triangulator(List<MappedPoint> points)
        {
            _pointsN = new LinkedList<MappedPoint>(points);
            if (!ArePointsCounterClockwise(_pointsN))
            {
                points.Reverse();
                _pointsN = new LinkedList<MappedPoint>(points);
            }
        }

        public static LinkedList<T> ReverseLinkedList<T>(LinkedList<T> linkedList)
        {
            LinkedList<T> copyList = new LinkedList<T>();
            LinkedListNode<T> start = linkedList.Last;
            while (start != null)
            {
                copyList.AddLast(start.Value);
                start = start.Previous;
            }
            return copyList;
        }

        bool ArePointsCounterClockwise(LinkedList<MappedPoint> testPoints)
        {
            float area = 0;
            
            for (var p = testPoints.First; p!=testPoints.Last; p = p.Next)
            {
                area += (p.Next.Value.v2.x - p.Value.v2.x) * (p.Next.Value.v2.y + p.Value.v2.y);
            }

            area += (testPoints.First.Value.v2.x - testPoints.Last.Value.v2.x) * (testPoints.First.Value.v2.y + testPoints.Last.Value.v2.y);

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

        private LinkedListNode<MappedPoint> Next(LinkedListNode<MappedPoint> n)
        {
            if (n.List.Last == n)
                return n.List.First;
            return n.Next;
        }

        private LinkedListNode<MappedPoint> Previous(LinkedListNode<MappedPoint> n)
        {
            if (n.List.First == n)
                return n.List.Last;
            return n.Previous;
        }

        public static bool IsVertConvex(MappedPoint prev, MappedPoint curr, MappedPoint next)
        {
            return prev.v2.x * (next.v2.y - curr.v2.y) + curr.v2.x * (prev.v2.y - next.v2.y) + next.v2.x * (curr.v2.y - prev.v2.y) < 0;
        }

        private bool ShouldAddConvexToEar(LinkedListNode<MappedPoint> node)
        {
            for (var p = _pointsN.First; p != null; p = p.Next)
                if (Previous(node) != p && node != p && Next(node) != p)
                    if (IsPointInTriangle(
                        p.Value,
                        Previous(node).Value,
                        node.Value,
                        Next(node).Value)
                    )
                        return false;

            return true;
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