﻿#version 330 core
in vec2 TexCoords;
in vec3 WorldPos;
in vec3 Normal;

out vec4 FragColor;

uniform vec3 camPos;

uniform vec3 albedo;
uniform float metallic;
uniform float roughness;
uniform float ao;

uniform vec3 lightPositions[4];
uniform vec3 lightColors[4];

const float PI = 3.14159265359;

vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1 - F0) * pow(clamp(1.0 - cosTheta, 0.f, 1.f), 5.0);
}

float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a      = roughness*roughness;
    float a2     = a*a;
    float NdotH  = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;

    float num   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return num / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float num   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return num / denom;
}
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2  = GeometrySchlickGGX(NdotV, roughness);
    float ggx1  = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}

void main() {
    vec3 N = normalize(Normal);
    vec3 V = normalize(camPos - WorldPos);
    
    vec3 Lo = vec3(0.f);
    for (int i = 0; i < 4; i++) {
        vec3 L = normalize(lightPositions[i] - WorldPos);
        vec3 H = normalize(V + L);
        
        float distance = length(lightPositions[i] - WorldPos);
        float attenation = 1.0 / (distance * distance);
        vec3 radiance = lightColors[i] * attenation;

        vec3 F0 = vec3(0.04f);
        F0      = mix(F0, albedo, metallic);
        vec3 F  = fresnelSchlick(max(dot(H, V), 0.0), F0);
        float NDF = DistributionGGX(N, H, roughness);
        float G   = GeometrySmith(N, V, L, roughness);

        vec3 numerator = NDF * G * F;
        float denominator = 4.f * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.00001;
        vec3 specular = numerator / denominator;
        
        vec3 kS = F;
        vec3 kD = vec3(1.f) - kS;
        
        kD *= 1.0 - metallic;
        
        float NdotL = max(dot(N, L), 0.f);
        Lo += (kD * albedo / PI + specular) * radiance * NdotL; 
    }
    
    vec3 ambient = vec3(0.03f) * albedo * ao;
    vec3 color   = ambient + Lo;
    
    // tone mapping & gamma correction
    color = color / (color + vec3(1.f));
    color = pow(color, vec3(1.f / 2.2f));
    
    FragColor = vec4(color, 1.f);
}