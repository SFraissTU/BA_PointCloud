using System.Collections;
using System.Collections.Generic;

namespace BAPointCloudRenderer.DataStructures
{
    /// <summary>
    /// A thredsafe queue
    /// </summary>
    /// <typeparam name="T">Value-Type</typeparam>
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

        public bool TryDequeue(out T result) {
            lock (queue) {
                if (queue.Count == 0) {
                    result = default(T);
                    return false;
                } else {
                    result = queue.Dequeue();
                    return true;
                }
            }
        }

        public bool IsEmpty()
        {
            lock (queue)
            {
                return queue.Count == 0;
            }
        }

        public void Clear() {
            lock (queue) {
                queue.Clear();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (queue) {
                return new Queue<T>(queue).GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count {
            get {
                lock (queue) {
                    return queue.Count;
                }
            }
        }
    }
}
