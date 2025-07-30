Shader "Custom/MyShader"
{
    Properties
    {
        // 머티리얼에 필요한 프로퍼티 정의 (예: 색상, 텍스처 등)
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _MainTex ("Base Texture", 2D) = "white" { }
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"  // URP에서 필요한 헤더 포함

            // 쉐이더에 필요한 상수, 텍스처, 샘플러 변수 정의
            float4 _BaseColor;
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // 입력 구조체 (appdata_t)
            struct appdata_t {
                float4 vertex : POSITION;  // 정점 위치
                float2 texcoord : TEXCOORD0; // 텍스처 좌표
            };

            // 출력 구조체 (v2f)
            struct v2f {
                float4 pos : SV_POSITION;    // 변환된 위치 (클립 공간)
                float2 uv : TEXCOORD0;       // 텍스처 좌표
            };

            // 버텍스 셰이더 함수 (정점 처리)
            void vert(appdata_t v, out appdata_t o)
            {
                o.vertex = TransformObjectToHClip(v.vertex);  // 변환: 오브젝트 공간 -> 클립 공간
                o.texcoord = v.texcoord;
            }

            // 프래그먼트 셰이더 함수 (픽셀 색상 처리)
            half4 frag(v2f i) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
                return texColor * _BaseColor;
            }

            ENDHLSL
        }
    }

    FallBack "Diffuse"  // 해당 쉐이더가 실패하면 대체 쉐이더
}
