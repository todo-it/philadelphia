using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphia.Common {
    public static class LinkedList {
        public class Node<T> : IEnumerable<T> {
            public readonly Node<T> Tail;
            public readonly T Head;

            private Node(T head, Node<T> tail) {
                Tail = tail;
                Head = head;
            }

            public static readonly Node<T> Empty = new Node<T>(default(T), null);

            public static Node<T> Cons(T value, Node<T> tail) => new Node<T>(value, tail);

            public IEnumerable<T> AsEnumerableHeadToTail() {
                var n = this;
                while (n.Tail != null) {
                    yield return n.Head;
                    n = n.Tail;
                }
            }

            public static Node<T> FromSeq(IEnumerable<T> seq) => seq.Aggregate(Empty, (current, v) => current.Add(v));

            public IEnumerator<T> GetEnumerator() => AsEnumerableHeadToTail().Reverse().GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }

        public static Node<T> Empty<T>() => Node<T>.Empty;
        
        public static Node<T> Singleton<T>(T value) => Node<T>.Cons(value, Node<T>.Empty);

        public static Node<T> Add<T>(this Node<T> list, T value) => Node<T>.Cons(value, list);

        public static Node<T> Reverse<T>(this Node<T> list) => list.AsEnumerableHeadToTail().Reverse().ToLinkedList();

        public static Node<T> ToLinkedList<T>(this IEnumerable<T> e) => Node<T>.FromSeq(e);
    }
}