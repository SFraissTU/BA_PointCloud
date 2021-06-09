using BAPointCloudRenderer.CloudData;
using UnityEngine;

namespace BAPointCloudRenderer.Loading {
    /// <summary>
    /// Various help functions
    /// </summary>
    class Util {

        /// <summary>
        /// Checks whether the bounding box is inside the frustum.
        /// Actually, there is a Unity function for this, however that one can only be called from the main thread.
        /// </summary>
        public static bool InsideFrustum(BoundingBox box, Plane[] frustum) {
            bool inside;
            for (int i = 0; i < 5; i++) {
                inside = false;
                Plane plane = frustum[i];   //Ignore Far Plane, because it doesnt work because of inf values
                inside |= plane.GetSide(new Vector3((float)box.Lx, (float)box.Ly, (float)box.Lz));
                if (inside) continue;
                inside |= plane.GetSide(new Vector3((float)box.Lx, (float)box.Ly, (float)box.Uz));
                if (inside) continue;
                inside |= plane.GetSide(new Vector3((float)box.Lx, (float)box.Uy, (float)box.Lz));
                if (inside) continue;
                inside |= plane.GetSide(new Vector3((float)box.Lx, (float)box.Uy, (float)box.Uz));
                if (inside) continue;
                inside |= plane.GetSide(new Vector3((float)box.Ux, (float)box.Ly, (float)box.Lz));
                if (inside) continue;
                inside |= plane.GetSide(new Vector3((float)box.Ux, (float)box.Ly, (float)box.Uz));
                if (inside) continue;
                inside |= plane.GetSide(new Vector3((float)box.Ux, (float)box.Uy, (float)box.Lz));
                if (inside) continue;
                inside |= plane.GetSide(new Vector3((float)box.Ux, (float)box.Uy, (float)box.Uz));
                if (!inside) return false;
            }
            return true;
        }

        /// <summary>
        /// Checks whether the vector is inside the frustum.
        /// Actually, there is a Unity function for this, however that one can only be called from the main thread.
        /// </summary>
        public static bool InsideFrustum(Vector3 vec, Plane[] frustum) {
            bool inside;
            for (int i = 0; i < 5; i++) {
                Plane plane = frustum[i];
                inside = plane.GetSide(vec);
                if (!inside) return false;
            }
            return true;
        }
    }
}
