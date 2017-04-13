using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataStructures
{
    class ThreadSafeQueue<T> : IEnumerable<T>
    {
        private Queue<T> queue;

        public ThreadSafeQueue()
        {
            queue = new Queue<T>();
        }

        public void Enqueue(T element)
        {
            lock (queue)
            {
                queue.Enqueue(element);
            }
        }

        public T Dequeue()
        {
            lock (queue)
            {
                return queue.Dequeue();
            }
        }

        public bool IsEmpty()
        {
            lock (queue)
            {
                return queue.Count == 0;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Queue<T>(queue).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
