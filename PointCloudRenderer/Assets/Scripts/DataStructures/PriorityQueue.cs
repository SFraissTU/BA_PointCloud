using System;
using System.Collections.Generic;

namespace DataStructures
{
    public interface PriorityQueue<I, T> : IEnumerable<T> where I : IComparable<I>
    {
        //Inserts an element with its priority into this queue
        void Enqueue(T element, I priority);

        //Removes the element with the highest priority from the queue
        T Dequeue();
                        //TODO: Exceptions when no elements exist
        T Peek();

        void Remove(T element, I priority); //Considers priority

        void Remove(T element); //Does not consider priority

        void Clear();

        bool IsEmpty();

        int Count
        {
            get;
        }
    }
}
