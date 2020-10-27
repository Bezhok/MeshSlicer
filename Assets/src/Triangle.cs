using UnityEngine;

namespace src
{
    public class Triangle
    {
        public Triangle(Vector2 a, Vector2 b, Vector2 c)
        {
            A = a;
            B = b;
            C = c;
        }

        public Vector2 A { get; }

        public Vector2 B { get; }

        public Vector2 C { get; }
    }
}