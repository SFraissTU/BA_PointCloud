using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace BAPointCloudRenderer.CloudController {
    public class ComputeBufferPointMesh : MonoBehaviour {
        private ComputeBuffer _pointsbuffer;
        private ComputeBuffer _colorsbuffer;
        private Material _material;
        private int _nbPoints = 0;
        
        private void OnDestroy () {
            _pointsbuffer.Dispose ();
            _colorsbuffer.Dispose ();
        }

        public void Init (Shader shader, Vector3[] vertexData, Color[] colorData) {
            if (_pointsbuffer == null && _colorsbuffer == null) {
                int count = 110000;
                _pointsbuffer = new ComputeBuffer (count, sizeof (float) * 3, ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
                _colorsbuffer = new ComputeBuffer (count, sizeof (uint), ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
            }

            if (_material == null) {
                _material = new Material (shader);
                _material.SetFloat ("_PointSize", 10);
                _material.SetBuffer ("_PointsBuffer", _pointsbuffer);
                _material.SetBuffer ("_ColorsBuffer", _colorsbuffer);
                _material.enableInstancing = true;
            }

            _nbPoints = vertexData.Length;
            int nbColors = colorData.Length;

            var vertices = new NativeArray<float3> (_nbPoints, Allocator.Temp);
            var colors = new NativeArray<uint> (nbColors, Allocator.Temp);
            for (var i = 0; i < _nbPoints; i++) {
                Vector3 pos = vertexData[i] + gameObject.transform.position;
                vertices[i] = new float3 (pos.x, pos.y, pos.z);
                colors[i] = EncodeColor (colorData[i]);
            }

            NativeArray<float3> tmpPoints = _pointsbuffer.BeginWrite<float3> (0, _nbPoints);
            tmpPoints.CopyFrom (vertices);
            _pointsbuffer.EndWrite<float3> (_nbPoints);

            NativeArray<uint> tmpColors = _colorsbuffer.BeginWrite<uint> (0, nbColors);
            tmpColors.CopyFrom (colors);
            _colorsbuffer.EndWrite<uint> (nbColors);

            vertices.Dispose ();
            colors.Dispose ();
        }

        void OnRenderObject () {
            if (_pointsbuffer == null || _colorsbuffer == null) {
                return;
            }
            _material.SetPass (0);
            Graphics.DrawProceduralNow (MeshTopology.Points, _nbPoints);
        }

        static uint EncodeColor (Color c) {
            const float kMaxBrightness = 16;

            var y = Mathf.Max (Mathf.Max (c.r, c.g), c.b);
            y = Mathf.Clamp (Mathf.Ceil (y * 255 / kMaxBrightness), 1, 255);

            var rgb = new Vector3 (c.r, c.g, c.b);
            rgb *= 255 * 255 / (y * kMaxBrightness);

            return ((uint) rgb.x) |
                   ((uint) rgb.y << 8) |
                   ((uint) rgb.z << 16) |
                   ((uint) y << 24);
        }
    }
}