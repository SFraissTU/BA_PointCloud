    Shader "Custom/Point Cloud"
    {
        Properties
        {
            _PointSize("Point Size", Float) = 0.05
        }
        SubShader
        {
            Pass
            {
                CGPROGRAM        
                #pragma multi_compile_fog
                #pragma vertex VSMain
                #pragma fragment PSMain
                #pragma target 5.0
     
                StructuredBuffer<uint> _ColorsBuffer;
                StructuredBuffer<float3> _PointsBuffer;
                float _PointSize;
                float4x4 _Transform;
                
                #include "UnityCG.cginc"
               
                struct shaderdata
                {
                    float4 vertex : SV_POSITION;
                    half3 color : COLOR;
                    float psize : PSIZE;
                    UNITY_FOG_COORDS(0)
                };
                
                half3 PcxDecodeColor(uint data)
                {
                    half r = (data      ) & 0xff;
                    half g = (data >>  8) & 0xff;
                    half b = (data >> 16) & 0xff;
                    half a = (data >> 24) & 0xff;
                    return half3(r, g, b) * a * 16 / (255 * 255);
                }
     
                shaderdata VSMain(uint id : SV_VertexID)
                {
                    shaderdata vs;
                    float3 pt = _PointsBuffer[id];
                    vs.vertex = UnityObjectToClipPos(pt);
                    vs.color = PcxDecodeColor(_ColorsBuffer[id]);
                    vs.psize = _PointSize;
                    UNITY_TRANSFER_FOG(vs, vs.vertex);
                    return vs;
                }
     
                float4 PSMain(shaderdata ps) : SV_TARGET
                {
                    float4 c = float4(ps.color, 255);
                    UNITY_APPLY_FOG(ps.fogCoord, c);
                    return c;
                }
               
                ENDCG
            }
        }
    }
