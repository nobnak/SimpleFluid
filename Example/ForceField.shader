Shader "SimpleFluid/ForceField" {
	Properties {
		_DirAndCenter ("Direction & Center", Vector) = (0, 1, 0.5, 0.5)
		_InvRadius ("Inv Radius", Float) = 10
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float4 _DirAndCenter;
			float _InvRadius;

			v2f vert (appdata v) {
                float2 uvb = v.uv;
                if (_MainTex_TexelSize.y < 0)
                    uvb.y = 1 - uvb.y;

				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = float4(v.uv, uvb);
				return o;
			}
			
			float4 frag (v2f i) : SV_Target {
				float2 dx =  (i.uv.xy - _DirAndCenter.zw) * _InvRadius;
				return float4(_DirAndCenter.xy * saturate(1.0 - dot(dx, dx)), 0, 0);
			}
			ENDCG
		}
	}
}
