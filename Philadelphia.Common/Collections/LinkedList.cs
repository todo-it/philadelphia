using System.Collections.Generic;
using System.Linq;

namespace Philadelphia.Common {
    public static class LinkedList {
        public class Node<T> {
            public readonly Node<T> Tail;
            public readonly T Head;

            private Node(T head, Node<T> tail) {
                Tail = tail;
                Head = head;
            }

            public static readonly Node<T> Empty = new Node<T>(default(T), null);

            public static Node<T> Cons(T value, Node<T> tail) =>
                new Node<T>(value, tail);
        }

        public static Node<T> Empty<T>() => Node<T>.Empty;
        
        public static Node<T> Singleton<T>(T value) => Node<T>.Cons(value, Node<T>.Empty);

        public static Node<T> Add<T>(this Node<T> list, T value) => Node<T>.Cons(value, list);

        /// <summary>
        /// NOTE: this is in reverse order as items were added (LIFO)
        /// </summary>
        public static IEnumerable<T> AsEnumerableHeadToTail<T>(this Node<T> list) {
            var n = list;
            while (n.Tail != null) {
                yield return n.Head;
                n = n.Tail;
            }
        }

        public static Node<T> ToLinkedList<T>(this IEnumerable<T> e) => 
            e.Aggregate(Node<T>.Empty, (current, v) => current.Add(v));
    }
}