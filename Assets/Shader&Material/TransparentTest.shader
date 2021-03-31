Shader "Unlit/TransparentTest"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Rnage("Range",Range(0,1)) =0.5
	}
	SubShader
	{
		 Tags { "RenderType"="Transparent" "Queue"="Transparent"}

		// ZWrite off
		
		// Blend SrcAlpha  OneMinusSrcAlpha
		 LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
			
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Rnage;
			inline float ValueNoise(float3 pos)
			{
				float3 Noise_skew = pos + 0.2127 + pos.x * pos.y * pos.z * 0.3713;
				float3 Noise_rnd = 4.789 * sin(489.123 * (Noise_skew));
				return frac(Noise_rnd.x * Noise_rnd.y * Noise_rnd.z * (1.0 + Noise_skew.x));
			}
			
			float random(float2 st, float n) {
				st = floor(st * n);
				return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
			 //col = random(i.uv.xy, 10);
			col = ValueNoise(i.vertex.xyz);
			return col;
			}
			ENDCG
		}
	}
}
