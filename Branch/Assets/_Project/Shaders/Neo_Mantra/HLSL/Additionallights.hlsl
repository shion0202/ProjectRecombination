#ifndef ADDITIONALLIGHTS_INCLUDED
#define ADDITIONALLIGHTS_INCLUDED

//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

void GetPointLights_float(float3 PositionWS, float3 NormalWS, out float3 OutColor, out float3 AvgDirection)
{
#if defined(SHADERGRAPH_PREVIEW)
    OutColor = float3(0, 0, 0);
    AvgDirection = float3(0, 0, 1); // 기본 방향 설정
#else
    // 출력 변수 초기화
    OutColor = float3(0, 0, 0);

    float3 sumDirection = float3(0, 0, 0);
    float totalIntensity = 0;
    
    // 메인 라이트 계산
    Light mainLight = GetMainLight();
    float mainLightIntensity = saturate(dot(NormalWS, mainLight.direction));
    OutColor += mainLight.color * mainLightIntensity;
    
    float mainLightWeight = mainLightIntensity; // 또는 mainLightIntensity * mainLight.color의 밝기
    sumDirection += mainLight.direction * mainLightWeight;
    totalIntensity += mainLightWeight;

    // 추가 라이트 계산
    int lightCount = GetAdditionalLightsCount();
    for (int i = 0; i < lightCount; ++i)
    {
        Light light = GetAdditionalLight(i, PositionWS);
        float3 lightDirection = light.direction; // .position 대신 .direction 사용
        float nDotL = saturate(dot(NormalWS, lightDirection));
        OutColor += light.color * light.distanceAttenuation * nDotL;
        
        float weight = nDotL * light.distanceAttenuation; // 필요 시 color의 세기도 곱하기
        sumDirection += light.direction * weight;
        totalIntensity += weight;
    }
    
    // 합산 벡터 정규화
    AvgDirection = (totalIntensity > 0) ? normalize(sumDirection / totalIntensity) : float3(0, 0, 1);
#endif
}

void GetFirstPointLight_float(float3 PositionWS, float3 NormalWS, out float3 OutDirection)
{
#if defined(SHADERGRAPH_PREVIEW)
    OutDirection = float3(0, 0, 0);
#else
    // 출력 변수 초기화
    OutDirection = float3(0, 0, 0);

    float firstWeight = 0;
    float3 firstDirection = float3(0, 0, 0);
    
    // 추가 라이트 계산
    int lightCount = GetAdditionalLightsCount();
    for (int i = 0; i < lightCount; ++i)
    {
        Light light = GetAdditionalLight(i, PositionWS);
        float nDotL = saturate(dot(NormalWS, light.direction));
        float weight = nDotL * light.distanceAttenuation;
        if (weight > firstWeight)
        {
            firstWeight = weight;
            firstDirection = light.direction;
        }
    }
    
    // Spot Light일 경우
    OutDirection = (firstWeight > 0) ? normalize(firstDirection) : float3(0, 0, 1);
#endif
}
#endif