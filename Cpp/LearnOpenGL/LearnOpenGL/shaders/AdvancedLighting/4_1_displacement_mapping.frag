#version 330 core
in VS_OUT{
    vec2 TexCoords;
    vec3 TangentLightPos;
    vec3 TangentViewPos;
    vec3 TangentFragPos;
} fs_in;

uniform sampler2D normalMap;
uniform sampler2D diffuseMap;
uniform sampler2D displacementMap;

out vec4 FragColor;

uniform float height_scale;

//const float numLayers = 10000;
const float maxLayers = 32.f;
const float minLayers = 8.f;

vec2 ParallaxMapping(vec2 texCoords, vec3 viewDir)
{
//    float height = texture(displacementMap, texCoords).r;
//    vec2 p = viewDir.xy / viewDir.z * (height * height_scale);
//    return texCoords - p;
    // 深度层数量
    float numLayers = mix(maxLayers, minLayers, max(dot(vec3(0.0, 0.0, 1.0), viewDir), 0.0));
    // 每层的深度大小
    float layerDepth = 1.0 / numLayers;
    // 初始化当前层深度
    float currentLayerDepth = 0.0;
    // 每层的纹理坐标偏移
    vec2 P = viewDir.xy * height_scale; // 向量总长度
    vec2 deltaTexCoords = P / numLayers; // 均分给每一层
    
    vec2 currentTexCoords = texCoords;
    float currentDepthMapValue = texture(displacementMap, currentTexCoords).r;
    
    while(currentLayerDepth < currentDepthMapValue)
    {
        currentTexCoords -= deltaTexCoords;
        currentDepthMapValue = texture(displacementMap, currentTexCoords).r;
        currentLayerDepth += layerDepth;
    }
    
    vec2 prevTexCoords = currentTexCoords + deltaTexCoords;
    
    float afterDepth = currentDepthMapValue - currentLayerDepth;
    float beforeDepth = texture(displacementMap, prevTexCoords).r - currentLayerDepth + layerDepth;
    
    float weight = afterDepth / (afterDepth - beforeDepth);
    vec2 finalTexCoords = prevTexCoords * weight + currentTexCoords * (1 - weight);
    
    return currentTexCoords;
}

void main() {
    vec3 viewDir = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);
    vec2 texCoords = ParallaxMapping(fs_in.TexCoords, viewDir);
    if(texCoords.x > 1.0 || texCoords.y > 1.0 || texCoords.x < 0.0 || texCoords.y < 0.0)
        discard;
    
    vec3 color = texture(diffuseMap, texCoords).rgb;
    vec3 normal = texture(normalMap, texCoords).rgb;
    normal = normalize(normal * 2.f - 1.f);
    
    vec3 lightColor = vec3(1.f);
    // ambient 
    vec3 ambient = .3f * lightColor;
    // diffuse
    vec3 lightDir = normalize(fs_in.TangentLightPos - fs_in.TangentFragPos);
    float diff = max(dot(lightDir, normal), 0.f);
    vec3 diffuse = diff * lightColor;
    // specular

    vec3 reflectDir = reflect(-lightDir, normal);
    vec3 halfDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfDir), 0.f), 64.f);
    vec3 specular = spec * lightColor;
    
    FragColor = vec4((ambient + diffuse + specular) * color, 1.f);
}