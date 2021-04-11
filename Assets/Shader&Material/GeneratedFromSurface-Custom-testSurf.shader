// Upgrade NOTE: replaced 'defined FOG_COMBINED_WITH_WORLD_POS' with 'defined (FOG_COMBINED_WITH_WORLD_POS)'

Shader "Custom/testSurfOriginal"
{
  Properties{
      _Color("Color", Color) = (1, 1, 1, 1)
          _Range("Range", Range(-2, 2)) = 0 _Frame("Frame", Range(0, 100)) = 0 _Angle("Angle", float) = 0 _MainTex("Albedo (RGB)", 2D) = "white" {} _Glossiness("Smoothness", Range(0, 1)) = 0.5 _Metallic("Metallic", Range(0, 1)) = 0.0} SubShader
  {
    Tags{"RenderType" = "Opaque"} LOD 200

        // ------------------------------------------------------------
        // Surface shader code generated out of a CGPROGRAM block:

        // ---- forward rendering base pass:
        Pass
    {
      Name "FORWARD" Tags{"LightMode" = "ForwardBase"}

      CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma target 4.5
#pragma multi_compile_instancing
#pragma multi_compile_fog
#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
#include "HLSLSupport.cginc"
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"

// -------- variant for: <when no other keywords are defined>

// -------- variant for: INSTANCING_ON

// Surface shader code generated based on:
// writes to per-pixel normal: no
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: no
// reads from normal: no
// 1 texcoords actually used
//   float2 _MainTex
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"
#include "Assets/Common/Shaders/Math.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data, normal) data.worldRefl
#define WorldNormalVector(data, normal) normal

      // Original surface shader snippet:

#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
          /* UNITY: Original start of shader */
          // Physically based Standard lighting model, and enable shadows on all light types
          //#pragma surface surf Standard fullforwardshadows

          // Use shader model 3.0 target, to get nicer looking lighting
          //#pragma target 3.0

          sampler2D _MainTex;

      struct Input
      {
        float2 uv_MainTex;
      };
      struct PosAndDir
      {
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

      half _Glossiness;
      half _Metallic;
      fixed4 _Color;
      float4 _WHScale;
      float _Range;
      uint _VertexCount;
      uint _VertexSize;
      uint _Frame;
      float _Angle;
      float _Amount;

#if SHADER_TARGET >= 45
      StructuredBuffer<PosAndDir> positionBuffer;
      StructuredBuffer<float3> VertexBuffer;
      StructuredBuffer<float4> colorBuffer;
#endif
      ///以物体坐标中心，旋转顶点
      float3 rotationDir(float3 localPos)
      {
        float4 q = rotate_angle_axis(-90 / RadianRatio, float3(1, 0, 0)); //注意，这里的角度参数为弧度
        float3 newPos = rotate_vector(localPos, q);
        return newPos;
      }

      void surf(Input IN, inout SurfaceOutputStandard o)
      {
        // Albedo comes from a texture tinted by color
        fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
        o.Albedo = c.rgb;
        // Metallic and smoothness come from slider variables
        o.Metallic = _Metallic;
        o.Smoothness = _Glossiness;
        o.Alpha = c.a;
      }

      // vertex-to-fragment interpolation data
      // no lightmaps:

      // half-precision fragment shader registers:

      // high-precision fragment shader registers:

      struct v2f_surf
      {
        UNITY_POSITION(pos);
        float2 pack0 : TEXCOORD0; // _MainTex
        float3 worldNormal : TEXCOORD1;
        float3 worldPos : TEXCOORD2;

        UNITY_FOG_COORDS(4)
        UNITY_SHADOW_COORDS(5)

        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
      };

      // with lightmaps:

      float4 _MainTex_ST;

      // vertex shader
      v2f_surf vert_surf(appdata_full v, uint id
                         : SV_VERTEXID, uint instanceID
                         : SV_InstanceID)
      {
#if SHADER_TARGET >= 45
        float4 data = positionBuffer[instanceID].position;
#else
        float4 data = float4(unity_ObjectToWorld._14_24_34_44.xyz, 1);
#endif

        if (id == 1)
          v.vertex.x += _Range;

        UNITY_SETUP_INSTANCE_ID(v);
        v2f_surf o;
        float3 worldPosition = data.xyz + v.vertex.xyz;

        float4 pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));

        UNITY_INITIALIZE_OUTPUT(v2f_surf, o);
        UNITY_TRANSFER_INSTANCE_ID(v, o);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        o.pos = UnityObjectToClipPos(v.vertex);
        //o.pos = pos;
        o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
        float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        float3 worldNormal = UnityObjectToWorldNormal(v.normal);

        o.worldPos.xyz = worldPos;
        o.worldNormal = worldNormal;

        UNITY_TRANSFER_LIGHTING(o, v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader

        UNITY_TRANSFER_FOG(o, o.pos); // pass fog coordinates to pixel shader

        return o;
      }

      // fragment shader
      fixed4 frag_surf(v2f_surf IN) : SV_Target
      {
        UNITY_SETUP_INSTANCE_ID(IN);
        // prepare and unpack data
        Input surfIN;

        UNITY_EXTRACT_FOG(IN);

        UNITY_INITIALIZE_OUTPUT(Input, surfIN);
        surfIN.uv_MainTex.x = 1.0;
        surfIN.uv_MainTex = IN.pack0.xy;
        float3 worldPos = IN.worldPos.xyz;

        fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));

        float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));

        SurfaceOutputStandard o;

        o.Albedo = 0.0;
        o.Emission = 0.0;
        o.Alpha = 0.0;
        o.Occlusion = 1.0;
        fixed3 normalWorldVertex = fixed3(0, 0, 1);
        o.Normal = IN.worldNormal;
        normalWorldVertex = IN.worldNormal;

        // call surface function
        surf(surfIN, o);

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

        giInput.lightmapUV = 0.0;

        giInput.ambient.rgb = 0.0;

        giInput.probeHDR[0] = unity_SpecCube0_HDR;
        giInput.probeHDR[1] = unity_SpecCube1_HDR;

#if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
        giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
#endif

        LightingStandard_GI(o, giInput, gi);

        // realtime lighting: call lighting function
        c += LightingStandard(o, worldViewDir, gi);
        UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
        UNITY_OPAQUE_ALPHA(c.a);
        //fixed4 coltmp = fixed4(1, 1, 1, 1);
        //return coltmp;
        return c;
      }

      ENDCG
    }
  }
  FallBack "Diffuse"
}
