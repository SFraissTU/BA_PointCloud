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

        public V2Cache(uint maxPoints) {
            this.maxPoints = maxPoints;
        }
        
        public void Insert(Node node) {
            lock (queue) {
                Withdraw(node); //it might be in the queue already but has to be moved to the front
                //Alte Objekte aus Cache entfernen
                while (cachePointCount + node.PointCount > maxPoints && !queue.IsEmpty()) {
                    Node old = queue.Dequeue();
                    cachePointCount -= (uint)old.PointCount;
                    old.ForgetPoints();
                }
                if (cachePointCount + node.PointCount <= maxPoints) {
                    //In Cache einfügen
                    queue.Enqueue(node);
                    cachePointCount += (uint)node.PointCount;
                } else {
                    //Nicht in Cache einfügen -> direkt entfernen
                    node.ForgetPoints();
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

        public uint PointCount() {
            lock (queue) {
                return cachePointCount;
            }
        }
    }
}
