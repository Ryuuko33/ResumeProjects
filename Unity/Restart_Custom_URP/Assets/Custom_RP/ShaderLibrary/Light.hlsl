#ifndef CUSTOM_Light_INCLUDED
#define CUSTOM_Light_INCLUDED

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

struct Light
{
    float3 color;
    float3 direction;
    float3 attenuation;
};

CBUFFER_START(_CustomLight)
    int _DirectionalLightCount;
    float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END


int GetDirectionalLightCount()
{
    return _DirectionalLightCount;
}

// 构造光源的ShadowData
DirectionalShadowData GetDirectionalShadowData(
    int lightIndex,
    ShadowData shadowData
    )
{
    DirectionalShadowData data;
    data.strength = _DirectionalLightShadowData[lightIndex].x * shadowData.strength;
    data.tileIndex =
        _DirectionalLightShadowData[lightIndex].y + shadowData.cascadeIndex;
    data.normalBias = _DirectionalLightShadowData[lightIndex].z; 
    return data;
}

Light GetDirectionalLight(int index, Surface surfaceWS, ShadowData shadowData)
{
    Light light;
    light.color = _DirectionalLightColors[index].rgb;
    light.direction = _DirectionalLightDirections[index].xyz;
    DirectionalShadowData dirShadowData = GetDirectionalShadowData(index, shadowData);
    light.attenuation = GetDirectionalShadowAttenuation(dirShadowData, shadowData, surfaceWS);

    return light;
}


#endif


