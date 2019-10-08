﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BAPointCloudRenderer.CloudData;

namespace BAPointCloudRenderer.Utility
{
    class BBDraw
    {
        //This function has been provided by Nelson A. Cysneros. Adapted by Simon M. Fraiss
        public static void DrawBoundingBox(BoundingBox boundingBox, Transform transform, Color color, bool showPosition)
        {
            Vector3[] cornerPositions = new Vector3[8];

            GetCornerPositions(boundingBox.GetBoundsObject(), transform, ref cornerPositions);

            if (showPosition)
            {

                Debug.DrawLine(boundingBox.Center().ToFloatVector(), boundingBox.Center().ToFloatVector() + Vector3.up, Color.green);

                Debug.DrawLine(boundingBox.Center().ToFloatVector(), boundingBox.Center().ToFloatVector() + Vector3.forward, Color.blue);

                Debug.DrawLine(boundingBox.Center().ToFloatVector(), boundingBox.Center().ToFloatVector() + Vector3.right, Color.red);
            }

            Debug.DrawLine(cornerPositions[0], cornerPositions[1], color);

            Debug.DrawLine(cornerPositions[1], cornerPositions[3], color);

            Debug.DrawLine(cornerPositions[3], cornerPositions[2], color);

            Debug.DrawLine(cornerPositions[2], cornerPositions[0], color);

            Debug.DrawLine(cornerPositions[4], cornerPositions[5], color);

            Debug.DrawLine(cornerPositions[5], cornerPositions[7], color);

            Debug.DrawLine(cornerPositions[7], cornerPositions[6], color);

            Debug.DrawLine(cornerPositions[6], cornerPositions[4], color);

            Debug.DrawLine(cornerPositions[0], cornerPositions[4], color);

            Debug.DrawLine(cornerPositions[1], cornerPositions[5], color);

            Debug.DrawLine(cornerPositions[3], cornerPositions[7], color);

            Debug.DrawLine(cornerPositions[2], cornerPositions[6], color);

        }

        /// <summary>
        /// Gets all the corner points of the bounds in world space
        /// THIS FUNCTION IS TAKEN FROM https://github.com/microsoft/MixedReality213/blob/master/Assets/HoloToolkit/Common/Scripts/Extensions/BoundsExtensions.cs
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="positions"></param>
        /// <remarks>
        /// Use BoxColliderExtensions.{Left|Right}{Bottom|Top}{Front|Back} consts to index into the output
        /// corners array.
        /// </remarks>
        private static void GetCornerPositions(Bounds bounds, Transform transform, ref Vector3[] positions)
        {
            // Calculate the local points to transform.
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;
            float leftEdge = center.x - extents.x;
            float rightEdge = center.x + extents.x;
            float bottomEdge = center.y - extents.y;
            float topEdge = center.y + extents.y;
            float frontEdge = center.z - extents.z;
            float backEdge = center.z + extents.z;

            // Allocate the array if needed.
            const int numPoints = 8;
            if (positions == null || positions.Length != numPoints)
            {
                positions = new Vector3[numPoints];
            }

            if (transform != null)
            {
                // Transform all the local points to world space.
                positions[0] = transform.TransformPoint(leftEdge, bottomEdge, frontEdge);
                positions[1] = transform.TransformPoint(leftEdge, bottomEdge, backEdge);
                positions[2] = transform.TransformPoint(leftEdge, topEdge, frontEdge);
                positions[3] = transform.TransformPoint(leftEdge, topEdge, backEdge);
                positions[4] = transform.TransformPoint(rightEdge, bottomEdge, frontEdge);
                positions[5] = transform.TransformPoint(rightEdge, bottomEdge, backEdge);
                positions[6] = transform.TransformPoint(rightEdge, topEdge, frontEdge);
                positions[7] = transform.TransformPoint(rightEdge, topEdge, backEdge);
            }
            else
            {
                positions[0] = new Vector3(leftEdge, bottomEdge, frontEdge);
                positions[1] = new Vector3(leftEdge, bottomEdge, backEdge);
                positions[2] = new Vector3(leftEdge, topEdge, frontEdge);
                positions[3] = new Vector3(leftEdge, topEdge, backEdge);
                positions[4] = new Vector3(rightEdge, bottomEdge, frontEdge);
                positions[5] = new Vector3(rightEdge, bottomEdge, backEdge);
                positions[6] = new Vector3(rightEdge, topEdge, frontEdge);
                positions[7] = new Vector3(rightEdge, topEdge, backEdge);
            }
        }
    }
}
