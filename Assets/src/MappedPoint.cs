using UnityEngine;

namespace src
{
    // Point on plane and 2d mapped version(relative to plane) 
    public class MappedPoint
    {
        public Vector3 v3;
        public Vector2 v2;
        
        public MappedPoint(Vector3 point, Vector3 u, Vector3 v)
        {
            v3 = point;
            v2 = new Vector2(Vector3.Dot(point, u), Vector3.Dot(point, v));;
        }
        
        public  bool Equals(MappedPoint obj)
        {
            return v3.Equals(obj.v3) && v2.Equals(obj.v2);
        }

        public override int GetHashCode()
        {
            return  v3.GetHashCode()*v2.GetHashCode();
        }
    }
}