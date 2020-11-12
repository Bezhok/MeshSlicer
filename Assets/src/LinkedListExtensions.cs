using System.Collections.Generic;

namespace src
{
    public class LinkedListExtensions
    {
        public static LinkedListNode<T> Next<T>(LinkedListNode<T> n)
        {
            if (n.List.Last == n)
                return n.List.First;
            return n.Next;
        }

        public static LinkedListNode<T> Previous<T>(LinkedListNode<T> n)
        {
            if (n.List.First == n)
                return n.List.Last;
            return n.Previous;
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
    }
}