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
        private uint traversalruns = 0;
        private Dictionary<long, List<Node>> oldLoaded;
        private int forgetstuff = 2; //floor(ntp/ntr) TODO

        public V2LoadingThread() {
            loadingQueue = new ThreadSafeQueue<Node>();
            oldLoaded = new Dictionary<long, List<Node>>();
        }

        public void Start() {
            new Thread(Run).Start();
        }

        private void Run() {
            try {
                long oldtraversalruns = traversalruns;
                while (running) {
                    Node n;
                    if (loadingQueue.TryDequeue(out n)) {
                        Monitor.Enter(n);
                        if (!n.HasPointsToRender() && !n.HasGameObjects()) {
                            Monitor.Exit(n);
                            CloudLoader.LoadPointsForNode(n);
                            if (oldLoaded.ContainsKey(oldtraversalruns)) {
                                List<Node> l;
                                oldLoaded.TryGetValue(oldtraversalruns, out l);
                                l.Add(n);
                            } else {
                                List<Node> l = new List<Node>();
                                l.Add(n);
                                oldLoaded.Add(oldtraversalruns, l);
                            }
                        } else {
                            Monitor.Exit(n);
                        }
                    }
                    if (traversalruns != oldtraversalruns) {
                        long passed = traversalruns - oldtraversalruns;
                        oldtraversalruns = traversalruns;
                        for (int i = forgetstuff; i <= passed; i++) {
                            List<Node> l;
                            if (oldLoaded.TryGetValue(oldtraversalruns - i, out l)) {
                                foreach (Node oldNode in l) {
                                    lock (n) {
                                        oldNode.ForgetPoints(); //Either it has no Game Objects, then its not relevant anymore, or it has them, then the points are already forgotten anyway
                                    }
                                }
                                oldLoaded.Remove(oldtraversalruns - i);
                            }
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

        public void ScheduleRemoveUnusedNodes() {
            ++traversalruns;
        }

    }
}
