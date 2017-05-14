using System;
using System.Collections;
using System.Collections.Generic;

namespace DataStructures
{
    /* This queue uses a sorted dictionary to work as a priority queue
     */
    public class DictionaryPriorityQueue<I, T> : PriorityQueue<I, T> where I : IComparable<I>
    {
        //Keys are stored negatively, so that the ordering is alright!
        private SortedDictionary<IWrapper, TWrapper> dictionary;
        private int count = 0;

        public DictionaryPriorityQueue()
        {
            dictionary = new SortedDictionary<IWrapper, TWrapper>();
        }

        //Inserts an element with its priority into this queue
        //Complexity: O(logn)
        public override void Enqueue(T element, I priority)
        {
            lock (dictionary)
            {
                TWrapper wrapper = new TWrapper(element);
                TWrapper existing;
                if (dictionary.TryGetValue(new IWrapper(priority), out existing))
                {
                    //Insert into list
                    wrapper.next = existing;
                    wrapper.last = existing.last;
                    existing.last.next = wrapper;
                    existing.last = wrapper;
                }
                else
                {
                    dictionary.Add(new IWrapper(priority), wrapper);
                }
                ++count;
            }
        }

        //Removes and returns the element with the highest priority from the queue. Throws an InvalidOperationException if no element exists
        //Complexity: O(logn)
        public override T Dequeue()
        {
            I p;
            return Dequeue(out p);
        }

        //Removes and returns the element with the highest priority from the queue. The priority is given through the parameter. Throws an InvalidOperationExcpetion if no element exists
        public override T Dequeue(out I priority) {
            lock (dictionary) {
                foreach (KeyValuePair<IWrapper, TWrapper> entry in dictionary) {
                    TWrapper twr = entry.Value;
                    --count;
                    priority = entry.Key.priority;
                    if (twr == twr.last) {
                        dictionary.Remove(entry.Key);
                        return twr.element;
                    } else {
                        //Remove next one from list
                        TWrapper toreturn = twr.next;
                        toreturn.next.last = toreturn.last;
                        toreturn.last.next = toreturn.next;
                        return toreturn.element;
                    }
                }
                throw new InvalidOperationException("Queue is empty!");
            }
        }

        public override I MaxPriority() {
            lock (dictionary) {
                foreach (KeyValuePair<IWrapper, TWrapper> entry in dictionary) {
                    TWrapper twr = entry.Value;
                    I priority = entry.Key.priority;
                    return priority;
                }
                throw new InvalidOperationException("Queue is empty!");
            }
        }

        //Returns the element with the highest priority from the queue without removing it. Throws an InvalidOperationException if no element exists
        //Complexity: O(logn)
        public override T Peek()
        {
            lock (dictionary)
            {
                foreach (KeyValuePair<IWrapper, TWrapper> entry in dictionary)
                {
                    return entry.Value.element;
                }
                throw new InvalidOperationException("Queue is empty!");
            }
        }

        //The number of elements in this queue
        public override int Count
        {
            get
            {
                return count;
            }
        }

        //Removes all elements from the queue
        public override void Clear()
        {
            lock (dictionary)
            {
                dictionary.Clear();
            }
        }

        //Returns true, iff the queue does not contain any elements
        public override bool IsEmpty()
        {
            lock (dictionary)
            {
                return count == 0;
            }
        }

        /* Returns a threadsafe enumerator, which means you can delete elements from the queue while enumerating over it.
         * However, the changes are not seen in the enumerator, as the data is copied at initializing the enumerator.
         * Complexity: O(nlogn) (Has to be copied and inserted into tree)
         */
        public override IEnumerator<T> GetEnumerator()
        {
            lock (dictionary) {
                return new QueueEnumerator(dictionary);
            }
        }

        //Removes the given element from this queue, if it exists. It's also assured that the given priority matches this element. Giving the priority speeds up the process. Only one element will be deleted, even if there are several equal ones
        //Complexity: O(log n)
        public override void Remove(T element, I priority)
        {
            lock (dictionary)
            {
                IWrapper priorityWr = new IWrapper(priority);
                TWrapper firsttw = null;
                dictionary.TryGetValue(priorityWr, out firsttw);
                TWrapper currenttw = firsttw;
                do
                {
                    if (currenttw.element.Equals(element))
                    {
                        if (currenttw == currenttw.next)
                        {
                            dictionary.Remove(priorityWr);
                            return;
                        } else
                        {
                            currenttw.last.next = currenttw.next;
                            currenttw.next.last = currenttw.last;
                            if (firsttw == currenttw)
                            {
                                dictionary.Remove(priorityWr);
                                dictionary.Add(priorityWr, currenttw.next);
                            }
                            currenttw.next = null;
                            currenttw.last = null;
                            currenttw.element = default(T);
                            return;
                        }
                    }
                    currenttw = currenttw.next;
                } while (currenttw != firsttw);
            }
        }

        //Removes the given element from this queue, if it exists. Only one element will be deleted, even if there are several equal ones
        //Complexity: O(n)
        public override void Remove(T element)
        {
            lock (dictionary)
            {
                foreach (KeyValuePair<IWrapper, TWrapper> entry in dictionary)
                {
                    TWrapper firsttw = entry.Value;
                    TWrapper currenttw = firsttw;

                    do
                    {
                        if (currenttw.element.Equals(element))
                        {
                            if (currenttw == currenttw.next)
                            {
                                dictionary.Remove(entry.Key);
                                return;
                            }
                            else
                            {
                                currenttw.last.next = currenttw.next;
                                currenttw.next.last = currenttw.last;
                                if (firsttw == currenttw)
                                {
                                    dictionary.Remove(entry.Key);
                                    dictionary.Add(entry.Key, currenttw.next);
                                }
                                currenttw.next = null;
                                currenttw.last = null;
                                currenttw.element = default(T);
                                return;
                            }
                        }
                        currenttw = currenttw.next;
                    } while (currenttw != firsttw);
                }
                
            }
        }

        //Internal thing, so that one priority can be mapped to several elements
        private class TWrapper
        {
            public T element;
            public TWrapper next = null;
            public TWrapper last = null;

            public TWrapper(T t)
            {
                element = t;
                next = last = this;
            }
        }

        private class IWrapper : IComparable<IWrapper>
        {
            public I priority;

            public IWrapper (I priority)
            {
                this.priority = priority;
            }

            public int CompareTo(IWrapper other)
            {
                return other.priority.CompareTo(priority);
            }

            public override bool Equals(object other)
            {
                if (other.GetType() == typeof(IWrapper)) {
                    return priority.Equals(((IWrapper)other).priority);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return priority.GetHashCode();
            }
        }

        /* Not able to handle concurrent modification!
         */
        private class QueueEnumerator : IEnumerator<T>
        {
            SortedDictionary<IWrapper,TWrapper> dict;
            IEnumerator<KeyValuePair<IWrapper, TWrapper>> dictionaryEnumerator;
            TWrapper currentList;
            TWrapper currentElement;
            bool invalidState = true;

            public QueueEnumerator(SortedDictionary<IWrapper, TWrapper> dict)
            {
                //We have to make a deep copy!
                this.dict = new SortedDictionary<IWrapper, TWrapper>();
                foreach (KeyValuePair<IWrapper, TWrapper> entry in dict) {
                    IWrapper key = new IWrapper(entry.Key.priority);
                    TWrapper firstOriginal = entry.Value;
                    TWrapper firstNew = new TWrapper(firstOriginal.element);
                    TWrapper currentOriginal = firstOriginal.next;
                    TWrapper lastNew = firstNew;
                    while (currentOriginal != firstOriginal)
                    {
                        TWrapper currentNew = new TWrapper(currentOriginal.element);
                        currentNew.last = lastNew;
                        lastNew.next = currentNew;
                        currentOriginal = currentOriginal.next;
                        lastNew = currentNew;
                    }
                    lastNew.next = firstNew;
                    firstNew.last = lastNew;
                    this.dict.Add(key, firstNew);
                }
                Reset();
            }

            public T Current
            {
                get
                {
                    if (invalidState)
                    {
                        throw new InvalidOperationException();
                    }
                    return currentElement.element;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public void Dispose()
            {
                dictionaryEnumerator = null;
                currentList = currentElement = null;
            }

            public bool MoveNext()
            {
                if (invalidState)
                {
                    if (!dictionaryEnumerator.MoveNext())
                    {
                        return false;
                    } else
                    {
                        invalidState = false;
                        currentList = dictionaryEnumerator.Current.Value;
                        currentElement = currentList;
                        return true;
                    }
                } else
                {
                    currentElement = currentElement.next;
                    if (currentList == currentElement)
                    {
                        invalidState = true;
                        return MoveNext();
                    } else
                    {
                        return true;
                    }
                }
            }

            public void Reset()
            {
                Dispose();
                invalidState = true;
                dictionaryEnumerator = dict.GetEnumerator();
            }
        }
    }
}
