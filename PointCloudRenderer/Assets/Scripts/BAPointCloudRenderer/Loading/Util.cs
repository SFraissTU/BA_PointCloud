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
            for (int i = 1; i < 5; i++) {
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
		
		        /// <summary>
        /// Checks whether:
        /// (a) the camera is inside the node bounding box,
        /// (b) the bounding box center is inside the camera frustum, and
        /// (c) a vertex of the bounding box is inside the camera frustum
        /// </summary>
        public static bool CameraFrustumBoundingBoxIntersection(BoundingBox box, Vector3 cameraPos, Plane[] frustum)
        {
            Vector3 sphereCenter = box.GetBoundsObject().center;
            float sphereRadius = (float) box.Radius();
            Vector3 heading = sphereCenter - cameraPos;
            float distance = heading.magnitude;

            if (sphereRadius > distance) return true; // Camera is inside the bounding box of the node
            if (InsideFrustum(sphereCenter, frustum)) return true; // BoundingBox center lies within the frustum
            if (InsideFrustum(new Vector3((float) box.Lx, (float) box.Ly, (float) box.Lz), frustum)) return true; // BoundingBox vertex lies withing the frustum
            if (InsideFrustum(new Vector3((float) box.Lx, (float) box.Ly, (float) box.Uz), frustum)) return true; // BoundingBox vertex lies withing the frustum
            if (InsideFrustum(new Vector3((float) box.Lx, (float) box.Uy, (float) box.Lz), frustum)) return true; // BoundingBox vertex lies withing the frustum
            if (InsideFrustum(new Vector3((float) box.Lx, (float) box.Uy, (float) box.Uz), frustum)) return true; // BoundingBox vertex lies withing the frustum
            if (InsideFrustum(new Vector3((float) box.Ux, (float) box.Ly, (float) box.Lz), frustum)) return true; // BoundingBox vertex lies withing the frustum
            if (InsideFrustum(new Vector3((float) box.Ux, (float) box.Ly, (float) box.Uz), frustum)) return true; // BoundingBox vertex lies withing the frustum
            if (InsideFrustum(new Vector3((float) box.Ux, (float) box.Uy, (float) box.Lz), frustum)) return true; // BoundingBox vertex lies withing the frustum
            if (InsideFrustum(new Vector3((float) box.Ux, (float) box.Uy, (float) box.Uz), frustum)) return true; // BoundingBox vertex lies withing the frustum
            return false;
        }
    }
}
