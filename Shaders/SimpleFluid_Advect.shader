Shader "SimpleFluid/Advect" {
	Properties {
		_MainTex ("Image", 2D) = "black" {}
		_FluidTex ("Fluid", 2D) = "white" {}
		_Dt ("Delta Time", Float) = 0.1
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always
        //ColorMask RGB

		Pass {
			CGPROGRAM
			#pragma target 5.0
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
			sampler2D _FluidTex;
			float4 _FluidTex_TexelSize;

			float _Dt;

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
				float2 duv = _FluidTex_TexelSize.xy;
				float4 u = tex2D(_FluidTex, i.uv.zw);
				float4 c = tex2D(_MainTex, i.uv.xy - _Dt * duv * u.xy);

				return clamp(c, 0.0, 2.0);
			}
			ENDCG
		}
	}
}
