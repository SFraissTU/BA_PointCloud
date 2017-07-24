using CloudData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Loading {

    /// <summary>
    /// This struct can be used to describe the rendering priority. (See Bachelor Thesis chapter 3.1.5 "Loading Priority")
    /// The octree-level as well as the calculated priority value are considered.
    /// So (grand-)parents will have a higher priority than their (grand-)children.
    /// For nodes without such a relationship, the given priority-double-value determines the order.
    /// </summary>
    struct LoadingPriority : IComparable<LoadingPriority> {

        public readonly PointCloudMetaData cloud;
        public readonly string nodeName;
        public readonly double calculatedPriority;
        public readonly int invert;

        /// <summary>
        /// Creates a new Loading Priority
        /// </summary>
        /// <param name="cloud">CloudMetaData, to identify the cloud the node belongs to</param>
        /// <param name="nodeName">The name of the node</param>
        /// <param name="calculatedPriority">Calculated Priority Value</param>
        /// <param name="invert">True, if the priority should be inverted, such that high priority becomes low priority and vice versa.</param>
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

        /// <summary>
        /// Inverts the priority
        /// </summary>
        public static LoadingPriority operator- (LoadingPriority p) {
            return new LoadingPriority(p.cloud, p.nodeName, p.calculatedPriority, p.invert == +1);
        }
    }
}
