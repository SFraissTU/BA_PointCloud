using CloudData;
using DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loading {
    public class CloudRenderState {

        private uint pointBudget;
        private int minNodeSize;

        private PriorityQueue<double, Node> toRender;
        //Points in toRender, that should not be rendered because they would exceed the point budget
        private HashSet<Node> notToRender;
        //Points that are supposed to be deleted. Normal Queue (but threadsafe)
        private ThreadSafeQueue<Node> toDelete;

        public CloudRenderState(uint pointBudget, int minNodeSize) {
            this.pointBudget = pointBudget;
            this.minNodeSize = minNodeSize;
            toRender = new HeapPriorityQueue<double, Node>();
            notToRender = new HashSet<Node>();
            toDelete = new ThreadSafeQueue<Node>();
        }

        public void ClearQueues() {
            toRender.Clear();
            notToRender.Clear();
            toDelete.Clear();
        }

    }
}
