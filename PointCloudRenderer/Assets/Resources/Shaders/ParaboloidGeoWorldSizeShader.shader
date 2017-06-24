// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/ParaboloidGeoWorldSizeShader"
{
	/*
	This shader renders the given vertices as circles with the given color.
	The point size is the radius of the circle given in WORLD COORDINATES
	Implemented using geometry shader
	*/
	Properties{
		_PointSize("Point Size", Float) = 5
		[Toggle] _Circles("Circles", Int) = 0
		_Details("DetailLevel", Int) = 2
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
				float4 N : NORMAL2;
			};

			struct VertexOutput
			{
				float4 position : SV_POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			float _PointSize;
			int _Circles;
			int _Details; //1, 2 or 3

			VertexMiddle vert(VertexInput v) {
				VertexMiddle o;
				o.position = mul(unity_ObjectToWorld, v.position);
				o.color = v.color;

				float3 view = normalize(UNITY_MATRIX_IT_MV[2].xyz);
				float3 upvec = normalize(UNITY_MATRIX_IT_MV[1].xyz);
				float3 R = normalize(cross(view, upvec));


				o.U = float4(upvec * _PointSize, 0);
				o.R = -float4(R * _PointSize, 0);
				return o;
			}

			VertexOutput createParaboloidPoint(VertexMiddle input, float u, float v) {
				VertexOutput nPoint;
				nPoint.position = input.position;
				nPoint.position += u*input.R;
				nPoint.position += v*input.U;
				float4 N = -float4(normalize(float3(nPoint.position - _WorldSpaceCameraPos))*_PointSize, 0);
				nPoint.position += (1 - (u*u + v*v))*N;
				nPoint.position = mul(UNITY_MATRIX_VP, nPoint.position);
				nPoint.color = input.color;
				nPoint.uv = float2(u, v);
				return nPoint;
			}

			[maxvertexcount(68)]
			void geom(point VertexMiddle input[1], inout TriangleStream<VertexOutput> outputStream) {
				float sqr = sqrt(2) / 2;
				float ringpositions[3] = { 0.25, 0.5, 1 };

				VertexOutput middle = createParaboloidPoint(input[0], 0, 0);

				//edges
				VertexOutput e11 = createParaboloidPoint(input[0], 1, 1);
				VertexOutput em11 = createParaboloidPoint(input[0], -1, 1);
				VertexOutput em1m1 = createParaboloidPoint(input[0], -1, -1);
				VertexOutput e1m1 = createParaboloidPoint(input[0], 1, -1);

				if (_Details > 0) {
					float ringval = ringpositions[3 - _Details];
					VertexOutput ir1 = createParaboloidPoint(input[0], ringval, 0);
					VertexOutput ir2 = createParaboloidPoint(input[0], sqr*ringval, sqr*ringval);
					VertexOutput ir3 = createParaboloidPoint(input[0], 0, ringval);
					VertexOutput ir4 = createParaboloidPoint(input[0], -sqr*ringval, sqr*ringval);
					VertexOutput ir5 = createParaboloidPoint(input[0], -ringval, 0);
					VertexOutput ir6 = createParaboloidPoint(input[0], -sqr*ringval, -sqr*ringval);
					VertexOutput ir7 = createParaboloidPoint(input[0], 0, -ringval);
					VertexOutput ir8 = createParaboloidPoint(input[0], sqr*ringval, -sqr*ringval);
					//Inner Circle
					outputStream.Append(ir1);
					outputStream.Append(middle);
					outputStream.Append(ir2);
					outputStream.Append(ir3);
					outputStream.RestartStrip();
					outputStream.Append(ir3);
					outputStream.Append(middle);
					outputStream.Append(ir4);
					outputStream.Append(ir5);
					outputStream.RestartStrip();
					outputStream.Append(ir5);
					outputStream.Append(middle);
					outputStream.Append(ir6);
					outputStream.Append(ir7);
					outputStream.RestartStrip();
					outputStream.Append(ir7);
					outputStream.Append(middle);
					outputStream.Append(ir8);
					outputStream.Append(ir1);
					outputStream.RestartStrip();

					for (int i = 4 - _Details; i < 3; i++) {
						float ringval = ringpositions[i];
						VertexOutput mr1 = createParaboloidPoint(input[0], ringval, 0);
						VertexOutput mr2 = createParaboloidPoint(input[0], sqr*ringval, sqr*ringval);
						VertexOutput mr3 = createParaboloidPoint(input[0], 0, ringval);
						VertexOutput mr4 = createParaboloidPoint(input[0], -sqr*ringval, sqr*ringval);
						VertexOutput mr5 = createParaboloidPoint(input[0], -ringval, 0);
						VertexOutput mr6 = createParaboloidPoint(input[0], -sqr*ringval, -sqr*ringval);
						VertexOutput mr7 = createParaboloidPoint(input[0], 0, -ringval);
						VertexOutput mr8 = createParaboloidPoint(input[0], sqr*ringval, -sqr*ringval);

						outputStream.Append(mr1);
						outputStream.Append(ir1);
						outputStream.Append(mr2);
						outputStream.Append(ir2);
						outputStream.Append(mr3);
						outputStream.Append(ir3);
						outputStream.Append(mr4);
						outputStream.Append(ir4);
						outputStream.Append(mr5);
						outputStream.Append(ir5);
						outputStream.Append(mr6);
						outputStream.Append(ir6);
						outputStream.Append(mr7);
						outputStream.Append(ir7);
						outputStream.Append(mr8);
						outputStream.Append(ir8);
						outputStream.Append(mr1);
						outputStream.Append(ir1);
						outputStream.RestartStrip();

						ir1 = mr1; ir2 = mr2; ir3 = mr3; ir4 = mr4; ir5 = mr5; ir6 = mr6; ir7 = mr7; ir8 = mr8;
					}


					outputStream.Append(ir1);
					outputStream.Append(ir2);
					outputStream.Append(e11);
					outputStream.Append(ir3);
					outputStream.RestartStrip();
					outputStream.Append(ir3);
					outputStream.Append(ir4);
					outputStream.Append(em11);
					outputStream.Append(ir5);
					outputStream.RestartStrip();
					outputStream.Append(ir5);
					outputStream.Append(ir6);
					outputStream.Append(em1m1);
					outputStream.Append(ir7);
					outputStream.RestartStrip();
					outputStream.Append(ir7);
					outputStream.Append(ir8);
					outputStream.Append(e1m1);
					outputStream.Append(ir1);
					outputStream.RestartStrip();
				}
				else {
					VertexOutput er = createParaboloidPoint(input[0], 1, 0);
					VertexOutput et = createParaboloidPoint(input[0], 0, 1);
					VertexOutput el = createParaboloidPoint(input[0], -1, 0);
					VertexOutput eb = createParaboloidPoint(input[0], -1, -1);

					outputStream.Append(er);
					outputStream.Append(middle);
					outputStream.Append(e11);
					outputStream.Append(et);
					outputStream.RestartStrip();
					outputStream.Append(et);
					outputStream.Append(middle);
					outputStream.Append(em11);
					outputStream.Append(el);
					outputStream.RestartStrip();
					outputStream.Append(el);
					outputStream.Append(middle);
					outputStream.Append(em1m1);
					outputStream.Append(eb);
					outputStream.RestartStrip();
					outputStream.Append(eb);
					outputStream.Append(middle);
					outputStream.Append(e1m1);
					outputStream.Append(er);
					outputStream.RestartStrip();
				}
			}

			float4 frag(VertexOutput o) : COLOR{
				if (_Circles >= 0.5 && o.uv.x*o.uv.x + o.uv.y*o.uv.y > 1) {
					discard;
				}
				return o.color;
			}

			ENDCG
		}
	}
}
