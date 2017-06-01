using CloudData;
using DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Loading {
    public class LRUCache {
        private const uint pointbytesize = 16;

        private uint maxPoints;
        private RandomAccessQueue<Node> queue;
        private uint currentPointCount = 0;

        public static LRUCache CacheFromByteSize(uint byteSize) {
            return new LRUCache(byteSize / pointbytesize);
        }

        public static LRUCache CacheFromPointCount(uint pointCount) {
            return new LRUCache(pointCount);
        }
        
        private LRUCache(uint maxPoints) {
            this.maxPoints = maxPoints;
            queue = new RandomAccessQueue<Node>();
        }

        /* Trys to insert a node into the cache. Might remove old nodes from the cache (which leads to their points being forgotten).
         * The node is stored until the memory is needed for other nodes.
         * If no space is left in the queue, the points are forgotten immediately and the node is not inserted
         */
        public void Insert(Node node) {
            lock (queue) {
                while (currentPointCount + node.PointCount > maxPoints && !queue.IsEmpty()) {
                    Node old = queue.Dequeue();
                    currentPointCount -= old.PointCount;
                    old.ForgetPoints();
                }
                if (currentPointCount + node.PointCount <= maxPoints) {
                    queue.Enqueue(node);
                    currentPointCount += node.PointCount;
                } else {
                    node.ForgetPoints();
                }
            }
        }

        /* Removes the node from the queue to be used again, so the points are not forgotten, so they can be used for game object creation again.
         */
        public void Withdraw(Node node) {
            lock (queue) {
                if (queue.Contains(node)) {
                    queue.Remove(node);
                    currentPointCount -= node.PointCount;
                    Debug.Log(currentPointCount);
                }
            }
        }


    }
}
