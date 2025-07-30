#ifndef ADDITIONALLIGHTS_INCLUDED
#define ADDITIONALLIGHTS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

void GetPointLights_float(float3 PositionWS, float3 NormalWS, out float3 OutColor)
{
    float3 combinedLightColor = float3(0, 0, 0);

    Light mainLight = GetMainLight();
    float mainLightIntensity = saturate(dot(NormalWS, mainLight.direction));
    combinedLightColor += mainLight.color * mainLightIntensity;

    int lightCount = GetAdditionalLightsCount();
    for (int i = 0; i < lightCount; ++i)
    {
        Light light = GetAdditionalLight(i, PositionWS);
        float3 lightDirection = normalize(light.position - PositionWS);
        float nDotL = saturate(dot(NormalWS, lightDirection));

        combinedLightColor += light.color * light.distanceAttenuation * nDotL;
    }

    OutColor = combinedLightColor;
}
#endif