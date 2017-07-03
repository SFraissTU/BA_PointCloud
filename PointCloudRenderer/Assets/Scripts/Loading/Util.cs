using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudData;
using UnityEngine;

namespace Loading {
    class Util {

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
