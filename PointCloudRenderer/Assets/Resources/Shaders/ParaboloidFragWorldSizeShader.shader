
Shader "Custom/ParaboloidFragWorldSizeShader"
{
	/*
	This shader renders the given vertices as circles with the given color.
	The point size is the radius of the circle given in WORLD COORDINATES
	Implemented using geometry shader.
	Interpolation is done by creating screen facing paraboloids in the fragment shader!
	*/
	Properties{
		_PointSize("Point Size", Float) = 5
		[Toggle] _Circles("Circles", Int) = 0
		[Toggle] _Cones("Cones", Int) = 0
	}

	SubShader
	{
		LOD 200

		Pass
		{
			Cull off

			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			struct VertexInput
			{
				float4 position : POSITION;
				float4 color : COLOR;
			};

			struct VertexMiddle {
				float4 position : SV_POSITION;
				float4 color : COLOR;
				float4 R : NORMAL0;
				float4 U : NORMAL1;
			};

			struct VertexOutput
			{
				float4 position : SV_POSITION;
				float4 viewposition : TEXCOORD1;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct FragmentOutput {
				float4 color : SV_TARGET;
				float depth : SV_DEPTH;
			};

			float _PointSize;
			int _Circles;
			int _Cones;

			VertexMiddle vert(VertexInput v) {
				VertexMiddle o;
				o.position = v.position;
				o.color = v.color;
				float3 view = normalize(UNITY_MATRIX_IT_MV[2].xyz);
				float3 upvec = normalize(UNITY_MATRIX_IT_MV[1].xyz);
				float3 R = normalize(cross(view, upvec));
				o.U = float4(upvec * _PointSize, 0);
				o.R = -float4(R * _PointSize, 0);
				return o;
			}

			[maxvertexcount(4)]
			void geom(point VertexMiddle input[1], inout TriangleStream<VertexOutput> outputStream) {
				VertexOutput out1;
				out1.position = input[0].position;
				out1.color = input[0].color;
				out1.uv = float2(-1.0f, 1.0f);
				out1.position += (-input[0].R + input[0].U);
				out1.viewposition = mul(UNITY_MATRIX_MV, out1.position);
				out1.position = UnityObjectToClipPos(out1.position);
				VertexOutput out2;
				out2.position = input[0].position;
				out2.color = input[0].color;
				out2.uv = float2(1.0f, 1.0f);
				out2.position += (input[0].R + input[0].U);
				out2.viewposition = mul(UNITY_MATRIX_MV, out2.position);
				out2.position = UnityObjectToClipPos(out2.position);
				VertexOutput out3;
				out3.position = input[0].position;
				out3.color = input[0].color;
				out3.uv = float2(1.0f, -1.0f);
				out3.position += (input[0].R - input[0].U);
				out3.viewposition = mul(UNITY_MATRIX_MV, out3.position);
				out3.position = UnityObjectToClipPos(out3.position);
				VertexOutput out4;
				out4.position = input[0].position;
				out4.color = input[0].color;
				out4.uv = float2(-1.0f, -1.0f);
				out4.position += (-input[0].R - input[0].U);
				out4.viewposition = mul(UNITY_MATRIX_MV, out4.position);
				out4.position = UnityObjectToClipPos(out4.position);
				outputStream.Append(out1);
				outputStream.Append(out2);
				outputStream.Append(out4);
				outputStream.Append(out3);
			}

			FragmentOutput frag(VertexOutput o) {
				FragmentOutput fragout;
				float uvlen = o.uv.x*o.uv.x + o.uv.y*o.uv.y;
				if (_Circles >= 0.5 && uvlen > 1) {
					discard;
				}
				if (_Cones < 0.5) {
					o.viewposition.z += (1 - uvlen) * _PointSize;
				}
				else {
					o.viewposition.z += (1 - sqrt(uvlen)) * _PointSize;
				}
				float4 pos = mul(UNITY_MATRIX_P, o.viewposition);
				pos /= pos.w;
				fragout.depth = pos.z;
				fragout.color = o.color;
				return fragout;
			}

			ENDCG
		}
	}
}
