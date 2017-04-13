using System;
using System.Collections;
using System.Collections.Generic;

namespace DataStructures
{
    /* This queue uses a sorted dictionary to work as a priority queue
     */
    public class DictionaryQueue<I, T> : PriorityQueue<I, T> where I : IComparable<I>
    {
        //Keys are stored negatively, so that the ordering is alright!
        private SortedDictionary<IWrapper, TWrapper> dictionary;
        private int count = 0;

        public DictionaryQueue()
        {
            dictionary = new SortedDictionary<IWrapper, TWrapper>();
        }

        public void Enqueue(T element, I priority)
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

        public T Dequeue()
        {
            lock (dictionary)
            {
                foreach (KeyValuePair<IWrapper, TWrapper> entry in dictionary)
                {
                    TWrapper twr = entry.Value;
                    --count;
                    if (twr == twr.last)
                    {
                        dictionary.Remove(entry.Key);
                        return twr.element;
                    }
                    else
                    {
                        //Remove next one from list
                        TWrapper toreturn = twr.next;
                        toreturn.next.last = toreturn.last;
                        toreturn.last.next = toreturn.next;
                        return toreturn.element;
                    }
                }
                return default(T);
            }
        }

        public T Peek()
        {
            lock (dictionary)
            {
                foreach (KeyValuePair<IWrapper, TWrapper> entry in dictionary)
                {
                    return entry.Value.element;
                }
                return default(T);
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
        }

        public void Clear()
        {
            lock (dictionary)
            {
                dictionary.Clear();
            }
        }

        public bool IsEmpty()
        {
            lock (dictionary)
            {
                return dictionary.Count == 0;
            }
        }

        /* Not able to handle concurrent modification!
         */
        public IEnumerator<T> GetEnumerator()
        {
            return new QueueEnumerator(dictionary);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Remove(T element, I priority)
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

        public void Remove(T element)
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
                this.dict = new SortedDictionary<IWrapper, TWrapper>(dict);
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
