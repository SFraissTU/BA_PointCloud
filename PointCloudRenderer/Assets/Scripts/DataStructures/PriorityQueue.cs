using System;
using System.Collections;
using System.Collections.Generic;

namespace DataStructures
{
    /* A >threadsafe< PriorityQueue.
     */
    public abstract class PriorityQueue<I, T> : IEnumerable<T> where I : IComparable<I>
    {
        //Inserts an element with its priority into this queue
        public abstract void Enqueue(T element, I priority);

        //Removes and returns the element with the highest priority from the queue. Throws an InvalidOperationException if no element exists
        public abstract T Dequeue();

        //Removes and returns the element with the highest priority from the queue. The priority is given through the parameter. Throws an InvalidOperationExcpetion if no element exists
        public abstract T Dequeue(out I priority);

        //Returns the highest priority
        public abstract I MaxPriority();

        //Returns the element with the highest priority from the queue without removing it. Throws an InvalidOperationException if no element exists
        public abstract T Peek();

        //Removes the given element from this queue, if it exists. It's also assured that the given priority matches this element. In some implementations, giving the priority may speed up the process. Only one element will be deleted, even if there are several equal ones
        public abstract void Remove(T element, I priority);

        //Removes the given element from this queue, if it exists. Only one element will be deleted, even if there are several equal ones
        public abstract void Remove(T element);

        //Removes all elements from the queue
        public abstract void Clear();

        //Returns true, iff the queue does not contain any elements
        public abstract bool IsEmpty();

        //The number of elements in this queue
        public abstract int Count
        {
            get;
        }

        /* Returns a threadsafe enumerator, which means you can delete elements from the queue while enumerating over it.
         * However, the changes might not be seen in the enumerator, depending on the implementation.
         */
        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
