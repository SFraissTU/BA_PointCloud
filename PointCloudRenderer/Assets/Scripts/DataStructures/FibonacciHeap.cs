using System;
using System.Collections.Generic;

namespace DataStructures
{
    /*class FibonacciHeap<I, T> : PriorityQueue<I, T> where I : IComparable<I> where T : Object 
    {
        private HeapNode maxNode = null;
        private HeapNode list = null;

        public FibonacciHeap() {
        }

        public void Enqueue(T element, I priority)
        {
            HeapNode node = new HeapNode(priority, element);
            InsertIntoRootList(node);
            if (list == null)
            {
                maxNode = list;
            } else
            {
                //if maxNode.priority < node.priority
                if (maxNode.priority.CompareTo(node.priority) < 0)
                {
                    maxNode = node;
                }
            }
        }

        public T Dequeue()
        {
            if (maxNode == null) return null;
            //Entferne MaxNode aus Liste
            maxNode.next.prev = maxNode.prev;
            maxNode.prev.next = maxNode.next;
            //Und aus Hierarchie
            if (maxNode.parent != null)
            {
                if (maxNode.next == maxNode)
                {
                    maxNode.parent.firstKid = null;
                }
                else
                {
                    maxNode.parent = maxNode.next;
                }
                maxNode.parent.degree--;
            }
            //Kinder in Wurzelliste hängen
            HeapNode firstKid = maxNode.firstKid;
            if (firstKid != null) {
                HeapNode currentKid = firstKid;
                do
                {
                    HeapNode nextKid = currentKid.next;
                    InsertIntoRootList(currentKid); //<- Hier wird der Parent der Kinder gesetzt
                    currentKid = nextKid;
                } while (currentKid != firstKid);
            }

            T result = maxNode.element;

            //Neue MaxNode suchen und heap aufräumen
            //Wurzelliste durchgehen
            if (list != null)
            {
                Dictionary<int, HeapNode> B = new Dictionary<int, HeapNode>();
                HeapNode lastRoot = list.prev;
                HeapNode currentRoot = list;
                do
                {
                    bool end = false;
                    while (!end)
                    {
                        int d = currentRoot.degree;
                        HeapNode Bd;
                        B.TryGetValue(d, Bd);
                        if (Bd == null)
                        {
                            B.Add(d, currentRoot);
                            end = true;
                        } else
                        {
                            if (Bd.priority.CompareTo(currentRoot.priority) < 0)
                            {
                                //Mache currentRoot zum Kind von v
                                //TODO
                            }
                        }
                    }
                } while (currentRoot != firstRoot);
            }
        }

        private void InsertIntoRootList(HeapNode node)
        {
            node.parent = null;
            if (list == null)
            {
                list = node;
                list.prev = list.next = list;
            }
            else
            {
                //An Ende von Liste anhängen
                node.next = list;
                node.prev = list.prev;
                list.prev.next = node;
                list.prev = node;
            }
        }

        private class HeapNode
        {
            public HeapNode prev = null;
            public HeapNode next = null;
            public HeapNode firstKid = null;
            public HeapNode parent = null;
            public int degree = 0;
            public I priority;
            public T element;

            public HeapNode(I priority, T element)
            {
                this.priority = priority;
                this.element = element;
            }
        }
    }*/
}
