// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Quad4PointScreenSizeShader"
{
	/*
	This shader renders the given vertices as circles with the given color. The uv-coordinates are given as offset-vectors ((-1,-1), (-1,1) etc.) which then are multiplied with the wanted point size.
	The point size is the radius of the circle given in pixel
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
			#pragma fragment frag

			struct VertexInput
			{
				float4 position : POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct VertexOutput
			{
				float4 position : SV_POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			float _PointSize;
			int _ScreenWidth;
			int _ScreenHeight;
			int _Circles;

			VertexOutput vert(VertexInput v) {
				VertexOutput o;
				o.position = UnityObjectToClipPos(v.position);
				o.position.x += v.uv.x * o.position.w * _PointSize / _ScreenWidth;
				o.position.y += v.uv.y * o.position.w * _PointSize / _ScreenHeight;
				o.color = v.color;
				o.uv = v.uv;
				return o;
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
