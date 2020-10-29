using UnityEngine;

namespace src
{
    public class Triangle
    {
        public Triangle(MappedPoint a, MappedPoint b, MappedPoint c)
        {
            A = a;
            B = b;
            C = c;
        }

        public MappedPoint A { get; }

        public MappedPoint B { get; }

        public MappedPoint C { get; }
    }
}