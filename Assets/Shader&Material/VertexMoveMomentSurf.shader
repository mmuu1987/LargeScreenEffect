﻿ Shader "Instanced/VertexMoveMomentSurf" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
         _Range("Range",Range(-2,2))=0
        _Frame("Frame",Range(0,100))=0
        _Angle("Angle",float) =0
         _Amount ("Extrusion Amount", Range(-1,1)) = 0.5
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model
        #pragma surface surf Standard addshadow fullforwardshadows
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup
        #pragma vertex:vert
        #include "Assets/Common/Shaders/Math.cginc"

        sampler2D _MainTex;
         float4 _WHScale;
            float _Range;
            uint _VertexCount;
            uint _VertexSize;
            uint _Frame;
            float _Angle;
            float _Amount;
        struct Input {
            float2 uv_MainTex;
            
        };
      struct PosAndDir   {
        float4 position;
        float4 velocity;
		float3 initialVelocity;
        float4 originalPos;
		float3 moveTarget;
		float3 moveDir;
		float2 indexRC;
		int picIndex;
        int bigIndex;
        float4 uvOffset; 
	    float4 uv2Offset; 
		int stateCode;

      };


    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        StructuredBuffer<PosAndDir> positionBuffer;
        StructuredBuffer<float3> VertexBuffer;
    #endif

        void rotate2D(inout float2 v, float r)
        {
            float s, c;
            sincos(r, s, c);
            v = float2(v.x * c - v.y * s, v.x * s + v.y * c);
        }

         ///以物体坐标中心，旋转顶点
            float3 rotationDir(float3 localPos)
            {
              float4 q = rotate_angle_axis(-90/RadianRatio,float3(1,0,0));//注意，这里的角度参数为弧度
              float3 newPos =  rotate_vector(localPos,q);
              return newPos;

            }

         void vert (inout appdata_full v)
         {

             // #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
             //  uint index = _VertexCount * _Frame + id;
                
             //   float3 localPos = VertexBuffer[index]; 
             //   v.vertex.xyz = localPos;
             //#endif
          
         // v.vertex.xyz += v.normal * _Amount;
              
       }

        void setup( )
        {
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            float4 data = positionBuffer[unity_InstanceID].position;

            //float rotation = data.w * data.w * _Time.y * 0.5f;
            //rotate2D(data.xz, rotation);

             //uint index = _VertexCount * _Frame + id;
                
             //float3 localPos = VertexBuffer[index]; 

            unity_ObjectToWorld._11_21_31_41 = float4(data.w, 0, 0, 0);
            unity_ObjectToWorld._12_22_32_42 = float4(0, data.w, 0, 0);
            unity_ObjectToWorld._13_23_33_43 = float4(0, 0, data.w, 0);
            unity_ObjectToWorld._14_24_34_44 = float4(data.xyz, 1);
            unity_WorldToObject = unity_ObjectToWorld;
            unity_WorldToObject._14_24_34 *= -1;
            unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
        #endif
        }

        half _Glossiness;
        half _Metallic;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}