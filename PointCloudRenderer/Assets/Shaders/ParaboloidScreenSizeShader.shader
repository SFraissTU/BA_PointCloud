Shader "Custom/ParaboloidScreenSizeShader"
{
	/*
	This shader renders the given vertices as circles with the given color.
	The point size is the radius of the circle given in pixel
	Implemented using geometry shader
	*/
	Properties{
		_PointSize("Point Size", Float) = 5
		_ScreenWidth("Screen Width", Int) = 0
		_ScreenHeight("Screen Height", Int) = 0
		[Toggle] _Circles("Circles", Int) = 0
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
					float distance : POINTSIZE;
					float4 position : SV_POSITION;
					float4 color : COLOR;
				};

				struct VertexOutput
				{
					float4 position : SV_POSITION;
					float4 color : COLOR;
					float2 uv : TEXCOORD0;
					float size : POINTSIZE;
					float distance : POINTSIZE;
				};

				struct FragmentOutput
				{
					float4 color : COLOR;
					float depth : SV_DEPTH;
				};

				float _PointSize;
				int _ScreenWidth;
				int _ScreenHeight;
				int _Circles;
				float4x4 _InverseProjMatrix;
				float3 _CameraPos;

				VertexMiddle vert(VertexInput v) {
					VertexMiddle o;
					o.position = UnityObjectToClipPos(v.position);
					o.color = v.color;
					o.distance = distance(v.position.xyz / v.position.w, _CameraPos);
					return o;
				}

				[maxvertexcount(4)]
				void geom(point VertexMiddle input[1], inout TriangleStream<VertexOutput> outputStream) {
					float xsize = _PointSize / _ScreenWidth;
					float ysize = _PointSize / _ScreenHeight;
					VertexOutput out1;
					out1.position = input[0].position;
					out1.color = input[0].color;
					out1.uv = float2(-1.0f, 1.0f);
					out1.position.x -= out1.position.w * xsize;
					out1.position.y += out1.position.w * ysize;
					float4 originalpos = mul(_InverseProjMatrix, out1.position);
					float3 originalpos3 = originalpos.xyz / originalpos.w;
					float3 middlepos = input[0].position.xyz / input[0].position.z;
					float wlen = length(originalpos3 - middlepos)/sqrt(2);
					out1.size = wlen;
					out1.distance = input[0].distance;
					VertexOutput out2;
					out2.position = input[0].position;
					out2.color = input[0].color;
					out2.uv = float2(1.0f, 1.0f);
					out2.position.x += out2.position.w * xsize;
					out2.position.y += out2.position.w * ysize;
					out2.size = wlen;
					out2.distance = input[0].distance;
					VertexOutput out3;
					out3.position = input[0].position;
					out3.color = input[0].color;
					out3.uv = float2(1.0f, -1.0f);
					out3.position.x += out3.position.w * xsize;
					out3.position.y -= out3.position.w * ysize;
					out3.size = wlen;
					out3.distance = input[0].distance;
					VertexOutput out4;
					out4.position = input[0].position;
					out4.color = input[0].color;
					out4.uv = float2(-1.0f, -1.0f);
					out4.position.x -= out4.position.w * xsize;
					out4.position.y -= out4.position.w * ysize;
					out4.size = wlen;
					out4.distance = input[0].distance;
					outputStream.Append(out1);
					outputStream.Append(out2);
					outputStream.Append(out4);
					outputStream.Append(out3);
					/*outputStream.RestartStrip();
					outputStream.Append(out1);
					outputStream.Append(out3);*/
					//outputStream.RestartStrip();
				}

				FragmentOutput frag(VertexOutput o)  {
					FragmentOutput fragout;
					float uvlen = o.uv.x*o.uv.x + o.uv.y*o.uv.y;
					if (_Circles >= 0.5 && uvlen > 1) {
						discard;
					}
					fragout.color = o.color;
					float dep = o.position.z;//o.distance;
					fragout.depth = dep;
					//fragout.color = float4(dep * 500, dep * 500, dep * 500, 1);
					//fragout.depth = o.position.w - (1 - uvlen) * o.size;
					//fragout.color = float4((o.position.z*o.position.w - (1 - uvlen)) * 3, (o.position.z*o.position.w - (1 - uvlen)) * 3, (o.position.z*o.position.w - (1 - uvlen)) * 3, 1);
					//fragout.depth = o.position.z;
					//fragout.depth = o.position.z;
					return fragout;
				}

			ENDCG
		}
	}
}
