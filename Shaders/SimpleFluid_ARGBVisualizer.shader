Shader "SimpleFluid/ARGBVisualizer" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
        _Color ("Background Color", Color) = (0,0,0,0)
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma multi_compile One SrcAlpha 
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
            float4x4 _ColorMatrix;
            float4 _Color;

			fixed4 frag (v2f i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);
				col = mul(_ColorMatrix, col);
                #if defined(SrcAlpha)
                col = col.w * float4(col.xyz, 1);
                #endif
                //return col;
                return lerp(_Color, col, col.w);
			}
			ENDCG
		}
	}
}
