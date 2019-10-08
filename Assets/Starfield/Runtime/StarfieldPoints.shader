Shader "Starfield/StarfieldPoints"
{
    Properties
    {
    _PointSize ("Point Size", Float) = 1
    [Toggle(NORMALIZE)]_NORMALIZE ("Normalize", Float) = 0
    _Scale ("Scale", Float) = 1

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
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma target 4.5
            #pragma shader_feature NORMALIZE

            struct appdata
            {
                float4 vertex : POSITION;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
            #if SHADER_TARGET >= 45
                float size : PSIZE;
            #endif			
            };

        struct Particle {
            float3 CurrentPosition;
            float3 OldPosition;
            float3 Velocity;
            float3 Color;
            float Scale;
        };
        #if SHADER_TARGET >= 45
		    StructuredBuffer<Particle> _SrcParticleBuffer;
        #endif

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerMaterial)
        float _PointSize = 1;
        float _Scale = 1;
CBUFFER_END

            v2f vert (appdata v)
            {

            #if SHADER_TARGET >= 45
                float4 data = float4(_SrcParticleBuffer[v.instanceID].CurrentPosition,0);
                fixed4 color = fixed4(_SrcParticleBuffer[v.instanceID].Color,1);
                // fixed4 color = fixed4(0,1,0,1);
            #else
                float4 data = 0;
                fixed4 color = fixed4(1,0,0,1);
            #endif


                float3 localPosition = v.vertex.xyz;// * data.w;
                
            #ifdef NORMALIZE
                float3 particlePosition = _Scale * normalize(data.xyz) + localPosition;
            #else
                float3 particlePosition = _Scale * data.xyz + localPosition;
            #endif


                v2f o;
            #if SHADER_TARGET >= 45
                o.size = _PointSize;
            #endif
                // o.vertex =  float4(particlePosition, 1.0f);
                // o.vertex =  UnityObjectToClipPos(float4(particlePosition, 1.0f));
                o.vertex = mul(UNITY_MATRIX_VP, float4(particlePosition, 1.0f));
			    o.color = color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = i.color;
                return col;
            }
            ENDCG
        }
    }
}
