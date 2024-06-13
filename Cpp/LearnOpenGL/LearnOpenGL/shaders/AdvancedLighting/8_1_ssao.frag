#version 330 core
out float FragColor;

in vec2 TexCoords;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D texNoise;

uniform mat4 projection;

uniform vec3 samples[64];

const int kernelSize = 64;
const float radius   = 0.5f;
const float bias     = 0.025f;

const vec2 noiseScale = vec2(800.f / 4.f, 600.f / 4.f);

void main()
{
    vec3 fragPos = texture(gPosition, TexCoords).xyz;
    vec3 normal = normalize(texture(gNormal, TexCoords).rgb);
    vec3 randomVec = normalize(texture(texNoise, TexCoords * noiseScale).xyz);
    
    // bulid TBN matrix
    vec3 tangent = normalize(randomVec - normal * dot(randomVec, normal));
    vec3 bitangent = cross(normal, tangent);
    mat3 TBN = mat3(tangent, bitangent, normal);
    
    // iterate every kernel sample that transform to view-space 
    float  occlusion = 0.f;
    for (int i = 0; i < kernelSize; ++i) {
        vec3 samplePos = TBN * samples[i];
        samplePos = fragPos + samplePos * radius;
        
        vec4 offset = vec4(samplePos, 1.f);
        offset = projection * offset;
        offset.xyz /= offset.w;
        offset.xyz = offset.xyz * .5f + .5f;
        
        float sampleDepth = texture(gPosition, offset.xy).z;
        
        // range check: 只有在范围检查内的深度值，才能够对遮蔽因子有增益
        float rangeCheck = smoothstep(0.0, 1.0, radius / abs(fragPos.z - sampleDepth));
        occlusion += (sampleDepth >= samplePos.z + bias ? 1.0 : 0.0) * rangeCheck;
    }
    // normaization
    occlusion = 1.0 - (occlusion / kernelSize);
    FragColor = occlusion;
}