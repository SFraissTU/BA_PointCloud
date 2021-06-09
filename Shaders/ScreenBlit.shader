// Author: Kazys Stepanas
// Replaces the EDLShader when we simply want to Blit without the EDL effect.
Shader "Hidden/ScreenBlit"
{
  Properties
  {
    _MainTex("Base (RGB)", 2D) = "white" {}
  }

  SubShader
  {
    Pass
    {
      Tags{ "Queue" = "Overlay" }
      ZTest Always
      Cull Off
      ZWrite Off

      CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#include "UnityCG.cginc"

      sampler2D _MainTex;
      //uniform float4 _MainTex_TexelSize;

      fixed4 frag(v2f_img i) : SV_Target
      {
        float2 uv = i.uv;
        // The following code may be required in some instances. If the rendered image is flipped
        // then enable this code and uncomment the uniform declaration of '_MainTex_TexelSize'
        // above.
//#if UNITY_UV_STARTS_AT_TOP
//        if (_MainTex_TexelSize.y < 0)
//        {
//          uv.y = 1 - uv.y;
//        }
//#endif // UNITY_UV_STARTS_AT_TOP        
        fixed4 color = tex2D(_MainTex, uv);

        if (color.a == 0.0)
        {
          discard;
        }

        return fixed4(color.rgb, 1.0f);
      }

      ENDCG
    }
  }
}
