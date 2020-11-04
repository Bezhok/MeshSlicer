using System.Collections.Generic;

namespace src
{
    public class SliceHierarchy
    {
        public LinkedList<MappedPoint> _points = new LinkedList<MappedPoint>();
        public List<SliceHierarchy> childs = new List<SliceHierarchy>();
        public SliceHierarchy father = null;
        public float maxX;
        public int level = 0; 
    }
}