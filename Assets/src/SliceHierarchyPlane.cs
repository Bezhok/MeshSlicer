using System.Collections.Generic;
using Plugins;
using UnityEngine;

namespace src
{
    public class SliceHierarchy
    {
        public LinkedList<MappedPoint> _points = new LinkedList<MappedPoint>();
        private TreeNode<LinkedList<Vector2>> _tree;
        
        // public SliceHierarchy(LinkedList<Vector2> points)
        // {
        //     _points = points; 
        //     _tree = new TreeNode<LinkedList<Vector2>>(_points);
        // }
    }
}