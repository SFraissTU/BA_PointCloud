// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/PointShader"
{
	/*
	This shader renders the given vertices as points with the given color.
	The point size is set to 30 (fixed), but unfortunately it doesn't seem to have any effect.
	*/
	SubShader
	{
		LOD 200

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct VertexInput
			{
				float4 position : POSITION;
				float4 color : COLOR;
			};

			struct VertexOutput
			{
				float4 position : SV_POSITION;
				float4 color : COLOR;
			};

			float _PointSize;

			VertexOutput vert(VertexInput v) {
				VertexOutput o;
				o.position = UnityObjectToClipPos(v.position);
				o.color = v.color;
				return o;
			}

			float4 frag(VertexOutput o) : COLOR{
				return o.color;
			}

			ENDCG
		}
	}
}
