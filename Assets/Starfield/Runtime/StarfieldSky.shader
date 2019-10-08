// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Starfield/StarfieldSky"
{
    Properties
    {
        _MaxDistance ("Max Distance", Float) = .1
        _MinThreshold ("Min Threshold", Float) = 0
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

            struct appdata
            {
                float4 vertex : POSITION;
                uint instanceID : SV_VertexID;
                
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
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

            float _MaxDistance;
            float _MinThreshold;
            int _ParticleCount;
            v2f vert (appdata v)
            {
                // float3 worldPosition =  mul (unity_ObjectToWorld, v.vertex).xyz;
                float3 lookupPosition =  normalize(v.vertex.xyz);
                float intensity = 0;
                for (int i = 0; i < _ParticleCount; i++)
                {
                    Particle p = _SrcParticleBuffer[i];
                    //distance on unit radius sphere
                    float distance = length(normalize(p.CurrentPosition) - lookupPosition);
                    intensity += distance <= _MaxDistance ? 1 : 0;
                }
                float3 color =
                    saturate( ( intensity - _MinThreshold ) / (float)_ParticleCount ).xxx;
                    
                // #if SHADER_TARGET >= 45
                //     fixed3 color = _SrcParticleBuffer[v.instanceID].Color;
                // #else
                //     fixed3 color = fixed3(1,0,0);
                // #endif

                // color = float3((_ParticleCount/1000.0).xxx);

                v2f o;
                o.vertex =  UnityObjectToClipPos(v.vertex);
                o.color = fixed4(color,1);

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
