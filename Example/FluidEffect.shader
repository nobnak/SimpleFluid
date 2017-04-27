Shader "SimpleFluid/FluidEffect" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
        _ImageTex ("Image", 2D) = "black" {}
        _Power ("Power", Float) = 1
        _Shift ("Color Shift", Vector) = (0,0,0,0)
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always
        ColorMask RGB

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
            #include "Assets/Packages/ColorCorrection/ColorSpace.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            sampler2D _ImageTex;
            float _Power;
            float4 _Shift;

            sampler2D _CameraDepthTexture;

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

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
				float4 csrc = tex2D(_MainTex, i.uv.xy);
                float4 cfluid = tex2D(_ImageTex, i.uv.zw);
                float z = tex2D(_CameraDepthTexture, i.uv.zw);
                float d = Linear01Depth(z);

                cfluid.xyz = HSVShift(cfluid.xyz * _Power, _Shift); 
                return lerp(csrc, cfluid, d);
			}
			ENDCG
		}
	}
}
