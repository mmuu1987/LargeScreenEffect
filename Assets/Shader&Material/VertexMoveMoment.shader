Shader "Unlit/VertexMoveMoment"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Range("Range",Range(-2,2))=0
        _Frame("Frame",Range(0,100))=0
        _Angle("Angle",float) =0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma target 4.5

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "AutoLight.cginc"


           
            #include "Assets/Common/Shaders/Math.cginc"
            #include "Assets/ComputeShader/GPUParticle.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint id:SV_VERTEXID;
                float3 normal:NORMAL;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                uint index:SV_InstanceID;//告诉片元，输送实例ID 必须是uint,
                float3 ambient : TEXCOORD1;
                float3 diffuse : TEXCOORD2;
                float3 color : TEXCOORD3;
                SHADOW_COORDS(4)
               
            };
           
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _WHScale;
            float _Range;
            uint _VertexCount;
            uint _VertexSize;
            uint _Frame;
            float _Angle;
			#if SHADER_TARGET >= 45
            StructuredBuffer<PosAndDir> positionBuffer;
            StructuredBuffer<float3> VertexBuffer;
			StructuredBuffer<float4> colorBuffer;
            #endif

            ///以物体坐标中心，旋转顶点
            float3 rotationDir(float3 localPos)
            {
              float4 q = rotate_angle_axis(-90/RadianRatio,float3(1,0,0));//注意，这里的角度参数为弧度
              float3 newPos =  rotate_vector(localPos,q);
              return newPos;

            }

            v2f vert (appdata_full  v,uint instanceID : SV_InstanceID, uint id:SV_VERTEXID)
            { 

            #if SHADER_TARGET >= 45
                float4 data = positionBuffer[instanceID].position;
            #else
                float4 data = 0;
            #endif
                
               
                
                uint index = _VertexCount*_Frame +id;
               
                float3 localPos = VertexBuffer[index]; 

                localPos = rotationDir(localPos);
                float3 worldNormal = rotationDir(v.normal);
                //if(v.id==0)v.vertex.x +=_Range;

                float3 initialVelocity = positionBuffer[instanceID].initialVelocity;//获取宽高
                float3 localPosition = v.vertex * data.w;
                localPosition.x *= _WHScale.x * initialVelocity.x;
                localPosition.y *= _WHScale.y * initialVelocity.y;
                float3 worldPosition = data.xyz + localPos;
              
             



                float3 ndotl = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
                float3 ambient = ShadeSH9(float4(worldNormal, 1.0f));
                float3 diffuse = (ndotl * _LightColor0.rgb);
                float3 color = v.color;

               

                v2f o;
                o.vertex = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
                o.uv = v.texcoord;
				o.index = instanceID;
                o.ambient = ambient;
                o.diffuse = diffuse;
                o.color = color;
                TRANSFER_SHADOW(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                fixed4 albedo = tex2D(_MainTex, i.uv);
                fixed shadow = SHADOW_ATTENUATION(i);
                float3 lighting = i.diffuse * shadow + i.ambient;
                fixed4 output = fixed4(albedo.rgb * i.color * lighting, albedo.w);
                return output;
                
            }
            ENDCG
        }
    }
}
