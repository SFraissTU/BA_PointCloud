using CloudData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Loading {
    /* This struct can be used to describe the rendering priority.
     * The special thing here is, that the octree-level is also considered.
     * So nodes with a lower level will have a higher priority than nodes with a higher level
     * For nodes at the same level, the given priority-double-value determines the order.
     */
    struct LoadingPriority : IComparable<LoadingPriority> {

        public readonly PointCloudMetaData cloud;
        public readonly string nodeName;
        public readonly double calculatedPriority;
        public readonly int invert;

        public LoadingPriority(PointCloudMetaData cloud, string nodeName, double calculatedPriority, bool invert) {
            this.cloud = cloud;
            this.nodeName = nodeName;
            this.calculatedPriority = calculatedPriority;
            this.invert = invert ? -1 : +1;
        }
        
        //-1 = other kommt vor this, +1 = other kommt nach this, 0 = gleichzeitig
        public int CompareTo(LoadingPriority other) {
            //Es kommen zuerst niedrige Levels dran. Pro Level wird nach Priority sortiert
            if (this.cloud != other.cloud) {
                return invert*CompareCalculatedPriority(other);
            } else {
                string otherName = other.nodeName;
                int commonlength = Math.Min(nodeName.Length, otherName.Length);
                for (int i = 0; i < commonlength; i++) {
                    if (nodeName[i] != otherName[i]) {
                        return invert * CompareCalculatedPriority(other);
                    }
                }
                if (nodeName.Length < otherName.Length) {
                    return +1 * invert;
                } else if (nodeName.Length > otherName.Length) {
                    return -1 * invert;
                } else {
                    return 0;
                }
            }
        }

        private int CompareCalculatedPriority(LoadingPriority other) {
            if (other.calculatedPriority > this.calculatedPriority) {
                return -1;
            } else if (other.calculatedPriority < this.calculatedPriority) {
                return +1;
            } else {
                return 0;
            }
        }

        public static bool operator< (LoadingPriority a, LoadingPriority b) {
            return a.CompareTo(b) < 0;
        }

        public static bool operator> (LoadingPriority a, LoadingPriority b) {
            return a.CompareTo(b) > 0;
        }

        public static LoadingPriority operator- (LoadingPriority p) {
            return new LoadingPriority(p.cloud, p.nodeName, p.calculatedPriority, p.invert == +1);
        }
    }
}
