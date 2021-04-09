Shader "Unlit/VertexMoveMoment"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Range("Range",Range(-2,2))=0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
             #pragma target 4.5


            #include "UnityCG.cginc"
            #include "UnityCG.cginc"
            #include "Assets/Common/Shaders/Math.cginc"
            #include "Assets/ComputeShader/GPUParticle.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint id:SV_VERTEXID;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                uint index:SV_InstanceID;//告诉片元，输送实例ID 必须是uint,
               
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _WHScale;
            float _Range;
			#if SHADER_TARGET >= 45
            StructuredBuffer<PosAndDir> positionBuffer;
			StructuredBuffer<float4> colorBuffer;
            #endif

            v2f vert (appdata v,uint instanceID : SV_InstanceID)
            { 

            #if SHADER_TARGET >= 45
                float4 data = positionBuffer[instanceID].position;
            #else
                float4 data = 0;
            #endif

                if(v.id==0)v.vertex.x +=_Range;
                float3 initialVelocity = positionBuffer[instanceID].initialVelocity;//获取宽高
                float3 localPosition = v.vertex * data.w;
                localPosition.x *= _WHScale.x * initialVelocity.x;
                localPosition.y *= _WHScale.y * initialVelocity.y;
                float3 worldPosition = data.xyz + localPosition;
              
               

                v2f o;
                o.vertex = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
                o.uv = v.uv;
				o.index = instanceID;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
