using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataStructures;
using CloudData;
using System.Threading;
using UnityEngine;

namespace Loading {
    class V2LoadingThread {

        private ThreadSafeQueue<Node> loadingQueue;
        private bool running = true;
        private V2Cache cache;

        public V2LoadingThread(V2Cache cache) {
            loadingQueue = new ThreadSafeQueue<Node>();
            this.cache = cache;
        }

        public void Start() {
            new Thread(Run).Start();
        }

        private void Run() {
            try {
                while (running) {
                    Node n;
                    if (loadingQueue.TryDequeue(out n)) {
                        Monitor.Enter(n);
                        if (!n.HasPointsToRender() && !n.HasGameObjects()) {
                            Monitor.Exit(n);
                            CloudLoader.LoadPointsForNode(n);
                            cache.Insert(n);
                        } else {
                            Monitor.Exit(n);
                        }
                    }
                }
            } catch (Exception ex) {
                Debug.LogError(ex);
            }
            Debug.Log("Loading Thread stopped");
        }

        public void Stop() {
            running = false;
        }

        public void ScheduleForLoading(Node node) {
            loadingQueue.Enqueue(node);
        }

    }
}
