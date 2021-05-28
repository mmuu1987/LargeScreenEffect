Shader "Unlit/VertexMoveMoment"
{
   Properties {
    _MainTex ("Albedo (RGB)", 2D) = "white" {}
    _Glossiness ("Smoothness", Range(0,1)) = 0.5
    _Metallic ("Metallic", Range(0,1)) = 0.0
    _Range("Range",Range(-180,180))=0
    _Frame("Frame",Range(0,100))=0
    _Angle("Angle",float) =0
    _Color("Color",color) = (1,1,1,1)
  }
  SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 200

   
    // ------------------------------------------------------------
    // Surface shader code generated out of a CGPROGRAM block:
    

    // ---- forward rendering base pass:
    Pass {
      Name "FORWARD"
      Tags { "LightMode" = "ForwardBase" }

      CGPROGRAM
      // compile directives
      #pragma vertex vert_surf
      #pragma fragment frag_surf
      #pragma multi_compile_instancing 
      #pragma multi_compile_fog
      #pragma multi_compile_fwdbase  nolightmap nodirlightmap nodynlightmap novertexlight
      #include "HLSLSupport.cginc"
      #pragma target 4.5
      #define UNITY_INSTANCED_LOD_FADE
      #define UNITY_INSTANCED_SH
      #define UNITY_INSTANCED_LIGHTMAPSTS
      #include "UnityShaderVariables.cginc"
      #include "UnityShaderUtilities.cginc"
      
      #include "UnityCG.cginc"
      #include "Lighting.cginc"
      #include "UnityPBSLighting.cginc"
      #include "AutoLight.cginc"

      #include "Assets/Common/Shaders/Math.cginc"
      #include "Assets/ComputeShader/GPUParticle.cginc"
      #define INTERNAL_DATA
      #define WorldReflectionVector(data,normal) data.worldRefl
      #define WorldNormalVector(data,normal) normal


      sampler2D _MainTex;
      float4 _WHScale;
      float _Range;
      uint _VertexCount;
      uint _VertexSize;
      uint _Frame;
      float _Angle;
      fixed4 _Color;
      struct Input {
        float2 uv_MainTex;
      };
      


      #if SHADER_TARGET >= 45
        StructuredBuffer<PosAndDir> positionBuffer;
      #endif

      void rotate2D(inout float2 v, float r)
      {
        float s, c;
        sincos(r, s, c);
        v = float2(v.x * c - v.y * s, v.x * s + v.y * c);
      }

      
      half _Glossiness;
      half _Metallic;

      void surf (Input IN, inout SurfaceOutputStandard o) {
        fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
        o.Albedo = c.rgb* _Color;;
        o.Metallic = _Metallic;
        o.Smoothness = _Glossiness;
        o.Alpha = c.a;
      }
      
      
      
      // half-precision fragment shader registers:
      #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
        #define FOG_COMBINED_WITH_WORLD_POS
        struct v2f_surf {
          UNITY_POSITION(pos);
          float2 pack0 : TEXCOORD0; // _MainTex
          float3 worldNormal : TEXCOORD1;
          float4 worldPos : TEXCOORD2;
          #if UNITY_SHOULD_SAMPLE_SH
            half3 sh : TEXCOORD3; // SH
          #endif
          UNITY_LIGHTING_COORDS(4,5)
          #if SHADER_TARGET >= 30
            float4 lmap : TEXCOORD6;
          #endif
          UNITY_VERTEX_INPUT_INSTANCE_ID
          UNITY_VERTEX_OUTPUT_STEREO 
        };
      #endif
      // high-precision fragment shader registers:
      #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
        struct v2f_surf {
          UNITY_POSITION(pos);
          float2 pack0 : TEXCOORD0; // _MainTex
          float3 worldNormal : TEXCOORD1;
          float3 worldPos : TEXCOORD2;
          #if UNITY_SHOULD_SAMPLE_SH
            half3 sh : TEXCOORD3; // SH
          #endif
          UNITY_FOG_COORDS(4)
          UNITY_SHADOW_COORDS(5)
          #if SHADER_TARGET >= 30
            float4 lmap : TEXCOORD6;
          #endif
          UNITY_VERTEX_INPUT_INSTANCE_ID
          UNITY_VERTEX_OUTPUT_STEREO
        };
      #endif
      
      
      float4 _MainTex_ST;

      // vertex shader
      v2f_surf vert_surf (appdata_full v,uint instanceID : SV_InstanceID, uint id:SV_VERTEXID) {

        //if(id==1)v.vertex.x+=_Range;

        float4 pos;
        float3 worldPos;
        #if SHADER_TARGET >= 45
          float4 data = positionBuffer[instanceID].position;
          float4 uv2Offset = positionBuffer[instanceID].uv2Offset;
          float4 uvOffset = positionBuffer[instanceID].uvOffset;
          // unity_ObjectToWorld._11_21_31_41 = float4(data.w, 0, 0, 0);
          // unity_ObjectToWorld._12_22_32_42 = float4(0, data.w, 0, 0);
          // unity_ObjectToWorld._13_23_33_43 = float4(0, 0, data.w, 0);
          // unity_ObjectToWorld._14_24_34_44 = float4(data.xyz, 1);
          // unity_WorldToObject = unity_ObjectToWorld;
          // unity_WorldToObject._14_24_34 *= -1;
          // unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;

          float angle = _SinTime.y * uv2Offset.y * uv2Offset.z* uv2Offset.w;//尽量让旋转多一点随机，这里用到的参数都很多随机
          float4 rot = rotate_angle_axis(angle,float3(0,1,0));
          float3 newVector = rotate_vector_at(v.vertex,float3(0,0,0),rot);
          worldPos = data.xyz + newVector *data.w;
          v.normal = rotate_vector_at( v.normal,float3(0,0,0),rot);
          pos= mul(UNITY_MATRIX_VP, float4(worldPos, 1.0f));
        #endif
        
        UNITY_SETUP_INSTANCE_ID(v);
        v2f_surf o;
        UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
        UNITY_TRANSFER_INSTANCE_ID(v,o);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        
        #if SHADER_TARGET >= 45
          o.pos = pos;
        #else
          o.pos = UnityObjectToClipPos(v.vertex);
        #endif
        
        o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);


        #if SHADER_TARGET < 45
          worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        #endif




        float3 worldNormal = UnityObjectToWorldNormal(v.normal);
        o.worldPos.xyz = worldPos;
        o.worldNormal = worldNormal;
        UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
        #ifdef FOG_COMBINED_WITH_TSPACE
          UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,o.pos); // pass fog coordinates to pixel shader
        #elif defined (FOG_COMBINED_WITH_WORLD_POS)
          UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,o.pos); // pass fog coordinates to pixel shader
        #else
          UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
        #endif
        return o;
      }

      // fragment shader
      fixed4 frag_surf (v2f_surf IN) : SV_Target {
        UNITY_SETUP_INSTANCE_ID(IN);
        // prepare and unpack data
        Input surfIN;
        #ifdef FOG_COMBINED_WITH_TSPACE
          UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
        #elif defined (FOG_COMBINED_WITH_WORLD_POS)
          UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
        #else
          UNITY_EXTRACT_FOG(IN);
        #endif
        UNITY_INITIALIZE_OUTPUT(Input,surfIN);
        surfIN.uv_MainTex.x = 1.0;
        surfIN.uv_MainTex = IN.pack0.xy;
        float3 worldPos = IN.worldPos.xyz;
        #ifndef USING_DIRECTIONAL_LIGHT
          fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
        #else
          fixed3 lightDir = _WorldSpaceLightPos0.xyz;
        #endif
        float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
        #ifdef UNITY_COMPILER_HLSL
          SurfaceOutputStandard o = (SurfaceOutputStandard)0;
        #else
          SurfaceOutputStandard o;
        #endif
        o.Albedo = 0.0;
        o.Emission = 0.0;
        o.Alpha = 0.0;
        o.Occlusion = 1.0;
        fixed3 normalWorldVertex = fixed3(0,0,1);
        o.Normal = IN.worldNormal;
        normalWorldVertex = IN.worldNormal;

        // call surface function
        surf (surfIN, o);

        // compute lighting & shadowing factor
        UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
        fixed4 c = 0;

        // Setup lighting environment
        UnityGI gi;
        UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
        gi.indirect.diffuse = 0;
        gi.indirect.specular = 0;
        gi.light.color = _LightColor0.rgb;
        gi.light.dir = lightDir;
        // Call GI (lightmaps/SH/reflections) lighting function
        UnityGIInput giInput;
        UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
        giInput.light = gi.light;
        giInput.worldPos = worldPos;
        giInput.worldViewDir = worldViewDir;
        giInput.atten = atten;
        #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
          giInput.lightmapUV = IN.lmap;
        #else
          giInput.lightmapUV = 0.0;
        #endif
        #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
          giInput.ambient = IN.sh;
        #else
          giInput.ambient.rgb = 0.0;
        #endif
        giInput.probeHDR[0] = unity_SpecCube0_HDR;
        giInput.probeHDR[1] = unity_SpecCube1_HDR;
        #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
          giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
        #endif
        #ifdef UNITY_SPECCUBE_BOX_PROJECTION
          giInput.boxMax[0] = unity_SpecCube0_BoxMax;
          giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
          giInput.boxMax[1] = unity_SpecCube1_BoxMax;
          giInput.boxMin[1] = unity_SpecCube1_BoxMin;
          giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
        #endif
        LightingStandard_GI(o, giInput, gi);

        // realtime lighting: call lighting function
        c += LightingStandard (o, worldViewDir, gi);
        UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
        UNITY_OPAQUE_ALPHA(c.a);
        return c;
      }
      ENDCG

    }


    // ---- shadow caster pass:
    Pass {
      Name "ShadowCaster"
      Tags { "LightMode" = "ShadowCaster" }
      ZWrite On ZTest LEqual

      CGPROGRAM
      // compile directives
      #pragma vertex vert_surf
      #pragma fragment frag_surf
      #pragma multi_compile_instancing
      #pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
      #pragma multi_compile_shadowcaster
      #include "HLSLSupport.cginc"
      #pragma target 4.5
      #define UNITY_INSTANCED_LOD_FADE
      #define UNITY_INSTANCED_SH
      #define UNITY_INSTANCED_LIGHTMAPSTS
      #include "UnityShaderVariables.cginc"
      #include "UnityShaderUtilities.cginc"
      #include "UnityCG.cginc"
      #include "Lighting.cginc"
      #include "UnityPBSLighting.cginc"

      #define INTERNAL_DATA
      #define WorldReflectionVector(data,normal) data.worldRefl
      #define WorldNormalVector(data,normal) normal

      #include "Assets/Common/Shaders/Math.cginc"
      #include "Assets/ComputeShader/GPUParticle.cginc"
      

      sampler2D _MainTex;
      float4 _WHScale;
      float _Range;
      uint _VertexCount;
      uint _VertexSize;
      uint _Frame;
      float _Angle;
      struct Input {
        float2 uv_MainTex;
      };
      #if SHADER_TARGET >= 45
        StructuredBuffer<PosAndDir> positionBuffer;
      #endif
      half _Glossiness;
      half _Metallic;

      void surf (Input IN, inout SurfaceOutputStandard o) {
        fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
        o.Albedo = c.rgb;
        o.Metallic = _Metallic;
        o.Smoothness = _Glossiness;
        o.Alpha = c.a;
      }
      

      // vertex-to-fragment interpolation data
      struct v2f_surf {
        V2F_SHADOW_CASTER;
        float3 worldPos : TEXCOORD1;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
      };

      // vertex shader
      v2f_surf vert_surf (appdata_full v,uint instanceID : SV_InstanceID, uint id:SV_VERTEXID) {

        //if(id==1)v.vertex.x+=_Range;
        float3 worldPos;
        #if SHADER_TARGET >= 45
          float4 data = positionBuffer[instanceID].position;
          float4 uv2Offset = positionBuffer[instanceID].uv2Offset;
          float4 uvOffset = positionBuffer[instanceID].uvOffset;
          //其他宏用到改变的矩阵
          unity_ObjectToWorld._11_21_31_41 = float4(data.w, 0, 0, 0);
          unity_ObjectToWorld._12_22_32_42 = float4(0, data.w, 0, 0);
          unity_ObjectToWorld._13_23_33_43 = float4(0, 0, data.w, 0);
          unity_ObjectToWorld._14_24_34_44 = float4(data.xyz, 1);
          unity_WorldToObject = unity_ObjectToWorld;
          unity_WorldToObject._14_24_34 *= -1;
          unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;


          float angle = _SinTime.y * uv2Offset.y * uv2Offset.z* uv2Offset.w;//尽量让旋转多一点随机，这里用到的参数都很多随机
          float4 rot = rotate_angle_axis(angle,float3(0,1,0));

          float3 newVector = rotate_vector_at(v.vertex,float3(0,0,0),rot);
          worldPos = data.xyz + newVector *data.w;
          v.normal = rotate_vector_at( v.normal,float3(0,0,0),rot);
          v.vertex.xyz = newVector;
        #endif

        UNITY_SETUP_INSTANCE_ID(v);
        v2f_surf o;
        UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
        UNITY_TRANSFER_INSTANCE_ID(v,o);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

        #if SHADER_TARGET < 45
          worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        #endif

        float3 worldNormal = UnityObjectToWorldNormal(v.normal);
        o.worldPos.xyz = worldPos;

        TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
        return o;
      }

      // fragment shader
      fixed4 frag_surf (v2f_surf IN) : SV_Target {
        UNITY_SETUP_INSTANCE_ID(IN);
        // prepare and unpack data
        Input surfIN;
        #ifdef FOG_COMBINED_WITH_TSPACE
          UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
        #elif defined (FOG_COMBINED_WITH_WORLD_POS)
          UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
        #else
          UNITY_EXTRACT_FOG(IN);
        #endif
        UNITY_INITIALIZE_OUTPUT(Input,surfIN);
        surfIN.uv_MainTex.x = 1.0;
        float3 worldPos = IN.worldPos.xyz;
        #ifndef USING_DIRECTIONAL_LIGHT
          fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
        #else
          fixed3 lightDir = _WorldSpaceLightPos0.xyz;
        #endif
        #ifdef UNITY_COMPILER_HLSL
          SurfaceOutputStandard o = (SurfaceOutputStandard)0;
        #else
          SurfaceOutputStandard o;
        #endif
        o.Albedo = 0.0;
        o.Emission = 0.0;
        o.Alpha = 0.0;
        o.Occlusion = 1.0;
        fixed3 normalWorldVertex = fixed3(0,0,1);

        // call surface function
        surf (surfIN, o);
        SHADOW_CASTER_FRAGMENT(IN)
      }
      ENDCG

    }
  }
  FallBack "Diffuse"
}
