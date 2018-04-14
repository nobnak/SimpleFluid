Shader "SimpleFluid/Visualizer/Fluid Image" {
	Properties {
		_MainTex ("Texture", 2D) = "black" {}
        _Fluidity ("Fluidity", Range(0,1)) = 0
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZTest LEqual ZWrite Off
        //Blend One OneMinusSrcAlpha
        Blend SrcAlpha OneMinusSrcAlpha

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
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);

                #ifdef UNITY_COLORSPACE_GAMMA
                col.rgb = GammaToLinearSpace(col);
				col = fluidOutMultiplier(col);
                col.rgb = LinearToGammaSpace(col);
                #else
                col = fluidOutMultiplier(col);
                #endif

                return col;
			}
			ENDCG
		}
	}
}
