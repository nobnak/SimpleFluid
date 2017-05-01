Shader "Unlit/SimpleFluid_FluidityTexture" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
        _Fluidity ("Fluidity", Range(0,1)) = 0
        _FluidityTex ("Fluidity Texture", 2D) = "black" {}
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma multi_compile FLUIDABLE_OUTPUT_COLOR FLUIDABLE_OUTPUT_SOURCE

			#include "UnityCG.cginc"
            #include "Assets/Packages/SimpleFluid/Shaders/SimpleFluid_Fluidable.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
            sampler2D _FluidityTex;
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);
                float fluidity = tex2D(_FluidityTex, i.uv).x;
				return fluidOutMultiplier(col, fluidity);
			}
			ENDCG
		}
	}
}
