using System;
using System.Collections.Generic;

namespace BAPointCloudRenderer.DataStructures {
    /* Queue which uses a list as well as a dictionary to implement efficient enqueing, dequeuing and removal. This however increases the overhead.
     */
    class RandomAccessQueue<T> {

        private LinkedList<T> list;
        private Dictionary<T,LinkedListNode<T>> table;

        public RandomAccessQueue() {
            list = new LinkedList<T>();
            table = new Dictionary<T,LinkedListNode<T>>();
        }

        /// <summary>
        /// Inserts the element into the queue
        /// </summary>
        public void Enqueue(T element) {
            table.Add(element,list.AddLast(element));
        }

        /// <summary>
        /// Removes the last inserted element from the queue. Throws an InvalidOperationException if the queue is empty.
        /// </summary>
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

        /// <summary>
        /// Returns true if the queue is empty
        /// </summary>
        public bool IsEmpty() {
            return list.Count == 0;
        }

        /// <summary>
        /// Removes all elements from the queue
        /// </summary>
        public void Clear() {
            list.Clear();
            table.Clear();
        }

        /// <summary>
        /// Removes the given element from the queue if it exists. If it does not exist, an InvalidOperationException is thrown.
        /// </summary>
        /// <param name="element"></param>
        public void Remove(T element) {
            LinkedListNode<T> node;
            table.TryGetValue(element, out node);
            if (node == null) {
                throw new InvalidOperationException("Element is not in Queue!");
            }
            table.Remove(element);
            list.Remove(node);
        }

        /// <summary>
        /// Returns true, iff the element is contained in the queue
        /// </summary>
        public bool Contains(T element) {
            return table.ContainsKey(element);
        }
    }
}
