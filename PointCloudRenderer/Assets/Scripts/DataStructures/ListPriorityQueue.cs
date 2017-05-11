using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures {
    /* A priorityqueue using a linkedlist, which is sorted when neccessary
     */
    public class ListPriorityQueue<I, T> : PriorityQueue<I, T> where I : IComparable<I> {

        private LinkedList<KeyValuePair<I, T>> list = new LinkedList<KeyValuePair<I, T>>();
        private KeyReverseComparer comparer = new KeyReverseComparer();
        private bool sorted = true;

        //Removes all elements from the queue
        public override void Clear() {
            lock (list) {
                list.Clear();
            }
        }

        //Removes and returns the element with the highest priority from the queue. Throws an InvalidOperationException if no element exists
        //Complexity: Worst Case: O(nlogn) (Assuming thats the sorting complexity), Best Case (when already sorted): O(1)
        public override T Dequeue() {
            lock (list) {
                if (list.Count == 0) {
                    throw new InvalidOperationException("Queue is empty!");
                }
                assertSorting();
                T element = list.First.Value.Value;
                list.RemoveFirst();
                return element;
            }
        }

        //Removes and returns the element with the highest priority from the queue. The priority is given through the parameter. Throws an InvalidOperationExcpetion if no element exists
        public override T Dequeue(out I priority) {
            lock (list) {
                if (list.Count == 0) {
                    throw new InvalidOperationException("Queue is empty!");
                }
                assertSorting();
                KeyValuePair<I, T> pair = list.First.Value;
                T element = pair.Value;
                priority = pair.Key;
                list.RemoveFirst();
                return element;
            }
        }

        //Returns the element with the highest priority from the queue without removing it. Throws an InvalidOperationException if no element exists
        //Complexity: Worst Case: O(nlogn) (Assuming thats the sorting complexity), Best Case (when already sorted): O(1)
        public override T Peek() {
            lock (list) {
                if (list.Count == 0) {
                    throw new InvalidOperationException("Queue is empty!");
                }
                assertSorting();
                return list.First.Value.Value;
            }
        }

        //Inserts an element with its priority into this queue
        //Complexity: O(1)
        public override void Enqueue(T element, I priority) {
            lock (list) {
                if (list.Count != 0) {
                    I smallestValue = list.Last.Value.Key;
                    // priority > smallestValue
                    if (priority.CompareTo(smallestValue) > 0) {
                        sorted = false;
                    }
                }
                list.AddLast(new KeyValuePair<I, T>(priority, element));
            }
        }

        //The number of elements in this queue
        public override int Count {
            get {
                return list.Count;
            }
        }

        /* Returns a threadsafe enumerator, which means you can delete elements from the queue while enumerating over it.
         * However, the changes are not seen in the enumerator, as the list is copied at initialization.
         * Complexity: O(n) (elements have to be copied)
         */
        public override IEnumerator<T> GetEnumerator() {
            lock (list) {
                assertSorting();
                return new QueueEnumerator(list);
            }
        }

        private void assertSorting() {
            if (!sorted) {
                list = new LinkedList<KeyValuePair<I, T>>(list.OrderBy((x) => x.Key, comparer));
                sorted = true;
            }
        }

        //Returns true, iff the queue does not contain any elements
        public override bool IsEmpty() {
            return list.Count == 0;
        }

        //Removes the given element from this queue, if it exists. It's also assured that the given priority matches this element. Only one element will be deleted, even if there are several equal ones
        //Complexity: O(n)
        public override void Remove(T element, I priority) {
            lock (list) {
                LinkedListNode<KeyValuePair<I, T>> node = list.First;
                while (node != null) {
                    if (node.Value.Value.Equals(element) && node.Value.Key.Equals(priority)) {
                        list.Remove(node);
                        return;
                    }
                    node = node.Next;
                }
            }
        }

        //Removes the given element from this queue, if it exists. Only one element will be deleted, even if there are several equal ones
        //Complexity: O(n)
        public override void Remove(T element) {
            lock (list) {
                LinkedListNode<KeyValuePair<I, T>> node = list.First;
                while (node != null) {
                    if (node.Value.Value.Equals(element)) {
                        list.Remove(node);
                        return;
                    }
                    node = node.Next;
                } 
            }
        }

        //Removes the element with the least priority from the queue and returns it
        public T Pop() {
            lock (list) {
                if (list.Count == 0) {
                    throw new InvalidOperationException("Queue is empty!");
                }
                assertSorting();
                T element = list.Last.Value.Value;
                list.RemoveLast();
                return element;
            }
        }

        private class QueueEnumerator : IEnumerator<T> {
            private IEnumerator<KeyValuePair<I, T>> enumerator;

            public QueueEnumerator(LinkedList<KeyValuePair<I, T>> list) {
                enumerator = new LinkedList<KeyValuePair<I, T>>(list).GetEnumerator();
            }

            public T Current {
                get {
                    return enumerator.Current.Value;
                }
            }

            object IEnumerator.Current {
                get {
                    return enumerator.Current.Value;
                }
            }

            public void Dispose() {
                enumerator.Dispose();
            }

            public bool MoveNext() {
                return enumerator.MoveNext();
            }

            public void Reset() {
                enumerator.Reset();
            }
        }

        private class KeyReverseComparer : IComparer<I> {
            public int Compare(I x, I y) {
                return y.CompareTo(x);
            }
        }
    }
}
