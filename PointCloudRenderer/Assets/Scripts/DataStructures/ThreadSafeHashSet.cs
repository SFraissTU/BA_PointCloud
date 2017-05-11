using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataStructures {
    class ThreadSafeHashSet<T> {

        private HashSet<T> set;

        public ThreadSafeHashSet() {
            set = new HashSet<T>();
        }

        public void Add(T element) {
            lock (set) {
                set.Add(element);
            }
        }

        public bool Contains(T element) {
            lock (set) {
                return set.Contains(element);
            }
        }

        public void Clear() {
            lock (set) {
                set.Clear();
            }
        }

        public void Remove(T element) {
            lock (set) {
                set.Remove(element);
            }
        }
    }
}
