using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataStructures;
using CloudData;

namespace Loading {
    class V2Cache {

        private uint maxPoints;
        private uint cachePointCount = 0;
        private RandomAccessQueue<Node> queue = new RandomAccessQueue<Node>();
        private ThreadSafeQueue<Node> toDestroy = new ThreadSafeQueue<Node>();

        public V2Cache(uint maxPoints) {
            this.maxPoints = maxPoints;
        }

        //Achtung: Deaktivieren und Reaktivieren wird nicht von Cache übernommen. Muss auserhalb gemacht werden
        //Node MUSS Points haben ODER GameObjects
        public void Insert(Node node) {
            lock (queue) {
                //Alte Objekte aus Cache entfernen
                while (cachePointCount + node.PointCount > maxPoints && !queue.IsEmpty()) {
                    Node old = queue.Dequeue();
                    cachePointCount -= (uint)old.PointCount;
                    if (old.HasGameObjects()) {
                        toDestroy.Enqueue(old);
                    } else {
                        old.ForgetPoints();
                    }
                }
                if (cachePointCount + node.PointCount <= maxPoints) {
                    //In Cache einfügen
                    queue.Enqueue(node);
                    cachePointCount += (uint)node.PointCount;
                } else {
                    //Nicht in Cache einfügen -> direkt entfernen
                    if (node.HasGameObjects()) {
                        toDestroy.Enqueue(node);
                    } else {
                        node.ForgetPoints();
                    }
                }
            }
        }

        //Reaktivieren muss außerhalb gemacht werden
        public void Withdraw(Node node) {
            lock (queue) {
                if (queue.Contains(node)) {
                    queue.Remove(node);
                    cachePointCount -= (uint)node.PointCount;
                }
            }
        }

        public Node NextToDestroy() {
            lock (queue) {
                if (toDestroy.Count == 0) {
                    return null;
                } else {
                    return queue.Dequeue();
                }
            }
        }
    }
}
