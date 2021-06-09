
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

			#include "UnityCG.cginc"

			struct VertexInput
			{
				float4 position : POSITION;
				float4 color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexMiddle {
				float4 position : SV_POSITION;
				float4 color : COLOR;
				float4 R : NORMAL0;
				float4 U : NORMAL1;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 position : SV_POSITION;
				float4 viewposition : TEXCOORD1;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
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

				// set all values in the v2g o to 0.0
				UNITY_INITIALIZE_OUTPUT(VertexMiddle, o);
				// setup the instanced id to be accessed
				UNITY_SETUP_INSTANCE_ID(v);
				// copy instance id in the appdata v to the v2g o
				UNITY_TRANSFER_INSTANCE_ID(v, o);

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
				
				//set all values in the g2f o to 0.0
				UNITY_INITIALIZE_OUTPUT(VertexOutput, out1);
				// setup the instanced id to be accessed
				UNITY_SETUP_INSTANCE_ID(input[0]);
				// copy instance id in the v2f i[0] to the g2f o
				UNITY_TRANSFER_INSTANCE_ID(input[0], out1);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(out1);
				
				out1.position = input[0].position;
				out1.color = input[0].color;
				out1.uv = float2(-1.0f, 1.0f);
				out1.position += (-input[0].R + input[0].U);
				out1.viewposition = mul(UNITY_MATRIX_MV, out1.position);
				out1.position = UnityObjectToClipPos(out1.position);
				
				VertexOutput out2;
				
				//set all values in the g2f o to 0.0
				UNITY_INITIALIZE_OUTPUT(VertexOutput, out2);
				// setup the instanced id to be accessed
				UNITY_SETUP_INSTANCE_ID(input[0]);
				// copy instance id in the v2f i[0] to the g2f o
				UNITY_TRANSFER_INSTANCE_ID(input[0], out2);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(out2);
				
				out2.position = input[0].position;
				out2.color = input[0].color;
				out2.uv = float2(1.0f, 1.0f);
				out2.position += (input[0].R + input[0].U);
				out2.viewposition = mul(UNITY_MATRIX_MV, out2.position);
				out2.position = UnityObjectToClipPos(out2.position);
				
				VertexOutput out3;
				
				//set all values in the g2f o to 0.0
				UNITY_INITIALIZE_OUTPUT(VertexOutput, out3);
				// setup the instanced id to be accessed
				UNITY_SETUP_INSTANCE_ID(input[0]);
				// copy instance id in the v2f i[0] to the g2f o
				UNITY_TRANSFER_INSTANCE_ID(input[0], out3);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(out3);
				
				out3.position = input[0].position;
				out3.color = input[0].color;
				out3.uv = float2(1.0f, -1.0f);
				out3.position += (input[0].R - input[0].U);
				out3.viewposition = mul(UNITY_MATRIX_MV, out3.position);
				out3.position = UnityObjectToClipPos(out3.position);
				
				VertexOutput out4;
				
				//set all values in the g2f o to 0.0
				UNITY_INITIALIZE_OUTPUT(VertexOutput, out4);
				// setup the instanced id to be accessed
				UNITY_SETUP_INSTANCE_ID(input[0]);
				// copy instance id in the v2f i[0] to the g2f o
				UNITY_TRANSFER_INSTANCE_ID(input[0], out4);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(ou4);
				
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
