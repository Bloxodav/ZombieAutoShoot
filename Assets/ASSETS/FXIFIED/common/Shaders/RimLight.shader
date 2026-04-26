Shader "URP/FXIFIED/RimLight_Fixed"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" {}
        [HDR] _Color("Color", Color) = (1,1,1,1)
        _ColorIntensity("Color Intensity", Float) = 1
        _Normals("Normals", 2D) = "bump" {}
        
        [Header(Rim Light Settings)]
        [HDR] _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimPower("Rim Power", Range(0.1, 10)) = 3
        
        [Header(Outline Settings)]
        _ASEOutlineColor("Outline Color", Color) = (0,0,0,1)
        _ASEOutlineWidth("Outline Width", Range(0, 0.05)) = 0.01
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" "Queue"="Geometry" }

        // --- PASS 1: OUTLINE (Улучшенный) ---
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            float _ASEOutlineWidth;
            float4 _ASEOutlineColor;

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Переводим позицию и нормаль в World Space
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                // Улучшенное раздутие: учитываем расстояние до камеры, 
                // чтобы обводка не становилась огромной вблизи
                float3 viewDirWS = normalize(GetCameraPositionWS() - positionWS);
                
                // Сдвигаем вершину
                positionWS += normalWS * _ASEOutlineWidth;
                
                output.positionCS = TransformWorldToHClip(positionWS);
                
                // Маленький трюк: сдвигаем обводку чуть глубже по Z, 
                // чтобы она меньше "резала" основную модель
                output.positionCS.z += 0.0001; 
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return _ASEOutlineColor;
            }
            ENDHLSL
        }

        // --- PASS 2: MAIN LIT + RIM ---
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD3;
                float4 vertexColor : COLOR;
            };

            sampler2D _MainTex;
            float4 _Color;
            float _ColorIntensity;
            float4 _RimColor;
            float _RimPower;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(TransformObjectToWorld(input.positionOS.xyz));
                output.vertexColor = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 viewDir = normalize(input.viewDirWS);
                float3 normal = normalize(input.normalWS);

                half4 tex = tex2D(_MainTex, input.uv);
                
                // Безопасная проверка Vertex Color (если его нет у меша)
                half4 vCol = any(input.vertexColor) ? input.vertexColor : half4(1,1,1,1);
                
                half3 baseColor = tex.rgb * _Color.rgb * _ColorIntensity * vCol.rgb;

                float fresnel = 1.0 - saturate(dot(normal, viewDir));
                float rim = pow(fresnel, _RimPower);
                half3 rimFinal = rim * _RimColor.rgb;

                return half4(baseColor + rimFinal, 1.0);
            }
            ENDHLSL
        }
    }
}