using System.Collections.Generic;

namespace src
{
    public class SliceHierarchy
    {
        public LinkedList<MappedPoint> Points = new LinkedList<MappedPoint>();
        public List<SliceHierarchy> Childs = new List<SliceHierarchy>();
        public SliceHierarchy Father = null;
        public float MaxX;
        public int Level = 0;
        public bool IsIntersected = false;
    }
}