using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace src
{
    public class Triangulator
    {
        private readonly HashSet<LinkedListNode<Vector2>> _convexVerts = new HashSet<LinkedListNode<Vector2>>();
        private readonly HashSet<LinkedListNode<Vector2>> _earsVerts = new HashSet<LinkedListNode<Vector2>>();
        private readonly HashSet<LinkedListNode<Vector2>> _reflexVerts = new HashSet<LinkedListNode<Vector2>>();
        private readonly List<Triangle> _triangles = new List<Triangle>();
        private readonly LinkedList<Vector2> _pointsN;

        public Triangulator(List<Vector2> points)
        {
            _pointsN = new LinkedList<Vector2>(points);
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
                _triangles.Add(new Triangle(Previous(node).Value, node.Value, Next(node).Value));
                _pointsN.Remove(node);

                ////// update convex and reflex and points
                if (_convexVerts.Contains(prevNode)) UpdateSiblingConvexDeleting(prevNode);

                if (_convexVerts.Contains(nextNode)) UpdateSiblingConvexDeleting(nextNode);

                if (_convexVerts.Contains(node)) _convexVerts.Remove(node);

                if (_reflexVerts.Contains(prevNode)) UpdateSiblingReflexDeleting(prevNode);

                if (_reflexVerts.Contains(nextNode)) UpdateSiblingReflexDeleting(nextNode);
            }

            return _triangles;
        }
        private void UpdateSiblingConvexDeleting(LinkedListNode<Vector2> node)
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
        }

        private void UpdateSiblingReflexDeleting(LinkedListNode<Vector2> node)
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

        private LinkedListNode<Vector2> Next(LinkedListNode<Vector2> n)
        {
            if (n.List.Last == n)
                return n.List.First;
            return n.Next;
        }

        private LinkedListNode<Vector2> Previous(LinkedListNode<Vector2> n)
        {
            if (n.List.First == n)
                return n.List.Last;
            return n.Previous;
        }

        private bool IsVertConvex(Vector2 prev, Vector2 curr, Vector2 next)
        {
            return prev.x * (next.y - curr.y) + curr.x * (prev.y - next.y) + next.x * (curr.y - prev.y) < 0;
        }

        private bool ShouldAddConvexToEar(LinkedListNode<Vector2> node)
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

        private bool IsPointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            var s = p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y;
            var t = p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y;

            if (s < 0 != t < 0)
                return false;

            var a = -p1.y * p2.x + p0.y * (p2.x - p1.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y;

            return a < 0 ? s <= 0 && s + t >= a : s >= 0 && s + t <= a;
        }
    }
}