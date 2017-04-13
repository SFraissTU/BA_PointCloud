using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures
{
    public class ListPriorityQueue<I, T> : PriorityQueue<I, T> where I : IComparable<I>
    {

        LinkedList<KeyValuePair<I,T>> list = new LinkedList<KeyValuePair<I, T>>();
        KeyReverseComparer comparer = new KeyReverseComparer();
        bool sorted = true;

        public void Clear()
        {
            lock (list)
            {
                list.Clear();
            }
        }

        public T Dequeue()
        {
            lock (list)
            {
                if (list.Count == 0)
                {
                    return default(T);
                }
                assertSorting();
                T element = list.First.Value.Value;
                list.RemoveFirst();
                return element;
            }
        }

        public T Peek()
        {
            lock (list)
            {
                if (list.Count == 0)
                {
                    return default(T);
                }
                assertSorting();
                return list.First.Value.Value;
            }
        }

        public void Enqueue(T element, I priority)
        {
            lock (list)
            {
                if (list.Count != 0)
                {
                    I smallestValue = list.Last.Value.Key;
                    // priority > smallestValue
                    if (priority.CompareTo(smallestValue) > 0)
                    {
                        sorted = false;
                    }
                }
                list.AddLast(new KeyValuePair<I, T>(priority, element));
            }
        }

        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        /* This enumerator copys the state of the list so iterating can be continued while elements are removed from the queue
         */
        public IEnumerator<T> GetEnumerator()
        {
            assertSorting();
            return new QueueEnumerator(list);
        }

        private void assertSorting()
        {
            if (!sorted)
            {
                list = new LinkedList<KeyValuePair<I,T>>(list.OrderBy((x) => x.Key, comparer));
                sorted = true;
            }
        }

        public bool IsEmpty()
        {
            return list.Count == 0;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Remove(T element, I priority)
        {
            lock (list)
            {
                foreach (KeyValuePair<I,T> entry in list)
                {
                    if (entry.Value.Equals(element) && entry.Key.Equals(priority))
                    {
                        list.Remove(entry);
                        return;
                    } 
                }
            }
        }

        public void Remove(T element)
        {
            lock (list)
            {
                foreach (KeyValuePair<I, T> entry in list)
                {
                    if (entry.Value.Equals(element))
                    {
                        list.Remove(entry);
                        return;
                    }
                }
            }
        }

        private class QueueEnumerator : IEnumerator<T>
        {
            private IEnumerator<KeyValuePair<I, T>> enumerator;

            public QueueEnumerator(LinkedList<KeyValuePair<I,T>> list)
            {
                enumerator = new LinkedList<KeyValuePair<I, T>>(list).GetEnumerator();
            }

            public T Current
            {
                get
                {
                    return enumerator.Current.Value;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return enumerator.Current.Value;
                }
            }

            public void Dispose()
            {
                enumerator.Dispose();
            }

            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }

            public void Reset()
            {
                enumerator.Reset();
            }
        }

        private class KeyReverseComparer : IComparer<I>
        {
            public int Compare(I x, I y)
            {
                return y.CompareTo(x);
            }
        }
    }
}
