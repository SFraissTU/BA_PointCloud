using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loading {
    /* This struct can be used to describe the rendering priority.
     * The special thing here is, that the octree-level is also considered.
     * So nodes with a lower level will have a higher priority than nodes with a higher level
     * For nodes at the same level, the given priority-double-value determines the order.
     */
    struct LoadingPriority : IComparable<LoadingPriority> {

        public readonly int level;
        public readonly double calculatedPriority;

        public LoadingPriority(int level, double calculatedPriority) {
            this.level = level;
            this.calculatedPriority = calculatedPriority;
        }

        public int CompareTo(LoadingPriority other) {
            //Es kommen zuerst niedrige Levels dran. Pro Level wird nach Priority sortiert
            if (other.level < this.level) {
                return -1;
            } else if (other.level > this.level) {
                return +1;
            } else {
                if (other.calculatedPriority > this.calculatedPriority) {
                    return -1;
                } else if (other.calculatedPriority < this.calculatedPriority) {
                    return +1;
                } else {
                    return 0;
                }
            }
        }

        public static bool operator< (LoadingPriority a, LoadingPriority b) {
            return a.CompareTo(b) < 0;
        }

        public static bool operator> (LoadingPriority a, LoadingPriority b) {
            return a.CompareTo(b) > 0;
        }

        public static LoadingPriority operator- (LoadingPriority p) {
            return new LoadingPriority(-p.level, -p.calculatedPriority);
        }
    }
}
