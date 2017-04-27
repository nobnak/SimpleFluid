Shader "SimpleFluid/Solver" {
	Properties {
        _MainTex ("Main", 2D) = "white" {}
		_FluidTex ("Fluid", 2D) = "white" {}
		_ImageTex ("Image", 2D) = "black" {}
		_ForceTex ("Force", 2D) = "black" {}
		_BoundaryTex ("Boundary", 2D) = "white" {}
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always

		CGINCLUDE
			#define DX 1.0
			#define DIFF (1.0 / (2.0 * DX))
			#define DDIFF (1.0 / (DX * DX))
			#pragma target 5.0
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			// (u, v, w, rho)
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
			sampler2D _FluidTex;
			float4 _FluidTex_TexelSize;
			sampler2D _ImageTex;
			float4 _ImageTex_TexelSize;

			sampler2D _BoundaryTex;
			float _Dt;
			float _KVis;
			float _S;
			sampler2D _ForceTex;
			float4 _ForceTex_ST;
			float _ForcePower;

			v2f vert(appdata v) {
                float2 uvb = v.uv;
                if (_MainTex_TexelSize.y < 0)
                    uvb.y = 1 - uvb.y;

				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = float4(v.uv, uvb);
				return o;
			}
		ENDCG

		// Fluid
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4 frag (v2f i) : SV_Target {
				float2 duv = _FluidTex_TexelSize.xy;
				float4 u = tex2D(_FluidTex, i.uv.zw);
				float4 ul = tex2D(_FluidTex, i.uv.zw - float2(duv.x, 0));
				float4 ur = tex2D(_FluidTex, i.uv.zw + float2(duv.x, 0));
				float4 ub = tex2D(_FluidTex, i.uv.zw - float2(0, duv.y));
				float4 ut = tex2D(_FluidTex, i.uv.zw + float2(0, duv.y));

				float2 uLaplacian = DDIFF * (ul.xy + ur.xy + ub.xy + ut.xy - 4.0 * u.xy);

				float4 dudx = DIFF * (ur - ul);
				float4 dudy = DIFF * (ut - ub);

				// Mass Conservation (Density)
				float2 rGrad = float2(dudx.w, dudy.w);
				float uDiv = dudx.x + dudy.y;
				u.w -= _Dt * dot(u.xyw, float3(rGrad, uDiv));
				u.w = clamp(u.w, 0.3, 1.7);

				// Momentum Conservation (Velocity)
				u.xy = tex2D(_FluidTex, i.uv.zw - _Dt * duv * u.xy).xy;
				float4 fTex = tex2D(_ForceTex, i.uv.zw);
				float2 f = _ForcePower * fTex.xy;
				u.xy += _Dt * (-_S * rGrad + f + _KVis * uLaplacian);

				float2 boundary = tex2D(_BoundaryTex, i.uv.zw);
				u.xy *= boundary;

				return u;
			}
			ENDCG
		}
	}
}
