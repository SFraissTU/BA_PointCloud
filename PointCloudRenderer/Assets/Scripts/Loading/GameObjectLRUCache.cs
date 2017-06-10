using CloudData;
using DataStructures;
using ObjectCreation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loading {
    public class GameObjectLRUCache {
        private const uint pointbytesize = 16;

        private uint maxPoints;
        private RandomAccessQueue<Node> queue;
        private uint currentPointCount = 0;
        private MeshConfiguration config;

        public static GameObjectLRUCache CacheFromByteSize(uint byteSize, MeshConfiguration config) {
            return new GameObjectLRUCache(byteSize / pointbytesize, config);
        }

        public static GameObjectLRUCache CacheFromPointCount(uint pointCount, MeshConfiguration config) {
            return new GameObjectLRUCache(pointCount, config);
        }

        private GameObjectLRUCache(uint maxPoints, MeshConfiguration config) {
            this.maxPoints = maxPoints;
            this.config = config;
            queue = new RandomAccessQueue<Node>();
        }

        /* Can only be called from the mainthread.
         * Call instead of RemoveGameObjects!
         */
        public void Insert(Node node) {
            lock (queue) {
                if (node.HasGameObjects()) {
                    while (currentPointCount + node.PointCount > maxPoints && !queue.IsEmpty()) {
                        Node old = queue.Dequeue();
                        currentPointCount -= (uint)old.PointCount;
                        old.RemoveGameObjects(config);
                    }
                    if (currentPointCount + node.PointCount <= maxPoints) {
                        queue.Enqueue(node);
                        node.DeactivateGameObjects();
                        currentPointCount += (uint)node.PointCount;
                    } else {
                        node.RemoveGameObjects(config);
                    }
                }
            }
        }

        /* Removes the node from the queue, so gameobjects can be reactivated again
         */
        public void Withdraw(Node node) {
            lock (queue) {
                if (queue.Contains(node)) {
                    queue.Remove(node);
                    currentPointCount -= (uint)node.PointCount;
                }
            }
        }
    }
}
