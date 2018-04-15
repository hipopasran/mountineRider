// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Copyright © 2015 NordicEdu Ltd.

Shader "Simply SVG/Simply SVG - Default" {
  Properties {
      _Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
  }

  SubShader{
    Tags {
        "RenderType" = "Opaque"
        "Queue" = "Overlay+1"
    }

    Pass {
      ZWrite Off
      Blend SrcAlpha OneMinusSrcAlpha

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      struct v2f {
        float4 pos : SV_POSITION;
        fixed4 color : COLOR;
      };

      struct appdata {
        float4 vertex : POSITION;
        float4 color : COLOR;
      };

      fixed4 _Color;

      v2f vert(appdata v) {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.color = v.color;
        return o;
      }

      fixed4 frag(v2f i) : SV_Target {
        fixed4 color = i.color;
        color *= _Color;
        return color;
      }
      ENDCG
    }
  }
}