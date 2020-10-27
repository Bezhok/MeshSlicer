using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace src
{
    
    public class Triangle
    {
        Vector2 a, b, c;

        public Vector2 A => a;

        public Vector2 B => b;

        public Vector2 C => c;

        public Triangle(Vector2 a, Vector2 b, Vector2 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }


    public class Triangulator
    {
        public LinkedListNode<Vector2> Next(LinkedListNode<Vector2> n)
        {
            if (n.List.Last == n)
            {
                return n.List.First;
            }
            else
            {
                return n.Next;
            }
        }
        
        public LinkedListNode<Vector2> Previous(LinkedListNode<Vector2> n)
        {
            if (n.List.First == n)
            {
                return n.List.Last;
            }
            else
            {
                return n.Previous;
            }
        }
        
        private List<LinkedListNode<Vector2>> _convexVertsN = new List<LinkedListNode<Vector2>>();
        private List<LinkedListNode<Vector2>> _reflexVertsN = new List<LinkedListNode<Vector2>>();
        private List<LinkedListNode<Vector2>> _earsVertsN = new List<LinkedListNode<Vector2>>();
        private LinkedList<Vector2> _pointsN;

        public List<Triangle> _triangles = new List<Triangle>();


        private bool IsVertConvex(Vector2 prev, Vector2 curr, Vector2 next)
        {
            // return a > 180; // than > 180 degrees
            return prev.x *(next.y - curr.y)+curr.x*(prev.y - next.y)+next.x*(curr.y-prev.y) < 0;
        }
        
        public Triangulator(List<Vector2> points)
        {

            _pointsN = new LinkedList<Vector2>(points);

            for (var p = _pointsN.First; p!=null; p = p.Next )
            {
                if (IsVertConvex(Previous(p).Value, p.Value, Next(p).Value)) // than > 180 degrees
                {
                    _convexVertsN.Add(p);
                }
                else
                {
                    _reflexVertsN.Add(p);
                }
            }

            for (int i = 0; i < _convexVertsN.Count; i++)
            {
                if (ShouldAddConvexToEar(_convexVertsN[i]))
                {
                    _earsVertsN.Add(_convexVertsN[i]);
                }
            }

            while (_earsVertsN.Any())
            {
                var i = _earsVertsN[_earsVertsN.Count - 1];
                var iPrev = Previous(i);
                var iNext = Next(i);
                
                _earsVertsN.RemoveAt(_earsVertsN.Count-1);
                _triangles.Add(new Triangle(Previous(i).Value, i.Value, Next(i).Value));

                Debug.Log(i.Value);
                _pointsN.Remove(i);
                
                ////// update convex and reflex and 
                for (int j = 0; j < _convexVertsN.Count; j++)
                {
                    if (_convexVertsN[j] == i)
                    {
                        _convexVertsN[j] = null;
                    }

                    if (_convexVertsN[j] == iPrev || _convexVertsN[j] == iNext)
                    {
                        if (IsVertConvex(
                            Previous(_convexVertsN[j]).Value,
                            _convexVertsN[j].Value,
                            Next(_convexVertsN[j]).Value)) // than > 180 degrees
                        {
                            //nothing
                        }
                        else
                        {
                            _earsVertsN.Remove(_convexVertsN[j]);
                            
                            _reflexVertsN.Add(_convexVertsN[j]);
                            _convexVertsN[j] = null;
                        }
                    }
                }
                
                for (int j = 0; j < _reflexVertsN.Count; j++)
                {
                    if (_reflexVertsN[j] == iPrev || _reflexVertsN[j] == iNext)
                    {
                        if (IsVertConvex(
                            Previous(_reflexVertsN[j]).Value,
                            _reflexVertsN[j].Value,
                            Next(_reflexVertsN[j]).Value)) // than > 180 degrees
                        {
                            _convexVertsN.Add(_reflexVertsN[j]);
                            
                            if (ShouldAddConvexToEar(_reflexVertsN[j]))
                            {
                                _earsVertsN.Add(_reflexVertsN[j]);
                            }
                            
                            _reflexVertsN[j] = null;
                        }
                        else
                        {
                            //nothing

                        }
                    }
                }

            }
        }

        bool ShouldAddConvexToEar(LinkedListNode<Vector2> node)
        {
            for (var p = _pointsN.First; p!=null; p = p.Next )
            {
                if (Previous(node) !=p &&node != p && Next(node) !=p)
                {
                    // Debug.Log(_convexVerts[j]);
                    if (IsPointInTriangle(p.Value,
                        Previous(node).Value,
                        node.Value,
                        Next(node).Value))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        private bool IsPointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            var s = p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y;
            var t = p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y;

            if ((s < 0) != (t < 0))
                return false;

            var A = -p1.y * p2.x + p0.y * (p2.x - p1.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y;

            return A < 0 ?
                (s <= 0 && s + t >= A) :
                (s >= 0 && s + t <= A);
        }
    }
}