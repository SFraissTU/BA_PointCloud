using System;
using System.Collections;
using System.Collections.Generic;

namespace BAPointCloudRenderer.DataStructures
{
     /// <summary>
     /// A threadsafe PriorityQueue.
     /// </summary>
     /// <typeparam name="I">The Priority-Type</typeparam>
     /// <typeparam name="T">The Value-Type</typeparam>
    public abstract class PriorityQueue<I, T> : IEnumerable<T> where I : IComparable<I>
    {
        /// <summary>
        /// Inserts an element with its priority into this queue
        /// </summary>
        public abstract void Enqueue(T element, I priority);

        /// <summary>
        /// Removes and returns the element with the highest priority from the queue. Throws an InvalidOperationException if no element exists
        /// </summary>
        public abstract T Dequeue();

        /// <summary>
        /// Removes and returns the element with the highest priority from the queue. The priority is given through the parameter. Throws an InvalidOperationExcpetion if no element exists
        /// </summary>
        public abstract T Dequeue(out I priority);

        /// <summary>
        /// Returns the highest priority
        /// </summary>
        public abstract I MaxPriority();

        /// <summary>
        /// Returns the element with the highest priority from the queue without removing it. Throws an InvalidOperationException if no element exists
        /// </summary>
        public abstract T Peek();

        /// <summary>
        /// Removes the given element from this queue, if it exists. It's also assured that the given priority matches this element. In some implementations, giving the priority may speed up the process. Only one element will be deleted, even if there are several equal ones
        /// </summary>
        public abstract void Remove(T element, I priority);

        /// <summary>
        /// Removes the given element from this queue, if it exists. Only one element will be deleted, even if there are several equal ones
        /// </summary>
        public abstract void Remove(T element);

        /// <summary>
        /// Removes all elements from the queue
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Returns true, iff the queue does not contain any elements
        /// </summary>
        public abstract bool IsEmpty();

        /// <summary>
        /// The number of elements in this queue
        /// </summary>
        public abstract int Count
        {
            get;
        }
        
         /// <summary>
         /// Returns a threadsafe enumerator, which means you can delete elements from the queue while enumerating over it.
         /// However, the changes might not be seen in the enumerator, depending on the implementation.
         /// </summary>
        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
