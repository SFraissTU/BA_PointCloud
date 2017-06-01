using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataStructures {
    /* Queue which uses a list as well as a dictionary to implement efficient enqueing, dequeuing and removal. This however increases the overhead.
     */
    class RandomAccessQueue<T> {

        private LinkedList<T> list;
        private Dictionary<T,LinkedListNode<T>> table;

        public RandomAccessQueue() {
            list = new LinkedList<T>();
            table = new Dictionary<T,LinkedListNode<T>>();
        }

        public void Enqueue(T element) {
            table.Add(element,list.AddLast(element));
        }

        public T Dequeue() {
            if (IsEmpty()) {
                throw new InvalidOperationException("Queue is empty");
            } else {
                T element = list.First.Value;
                list.RemoveFirst();
                table.Remove(element);
                return element;
            }
        }

        public bool IsEmpty() {
            return list.Count == 0;
        }

        public void Clear() {
            list.Clear();
            table.Clear();
        }

        public void Remove(T element) {
            LinkedListNode<T> node;
            table.TryGetValue(element, out node);
            if (node == null) {
                throw new InvalidOperationException("Element is not in Queue!");
            }
            table.Remove(element);
            list.Remove(node);
        }

        public bool Contains(T element) {
            return table.ContainsKey(element);
        }
    }
}
