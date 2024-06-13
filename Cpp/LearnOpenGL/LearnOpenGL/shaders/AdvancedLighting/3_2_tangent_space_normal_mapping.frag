#version 330 core
in VS_OUT{
    vec2 TexCoords;
    vec3 TangentLightPos;
    vec3 TangentViewPos;
    vec3 TangentFragPos;
} fs_in;

uniform sampler2D normalMap;
uniform sampler2D diffuseMap;

out vec4 FragColor;

void main() {
    vec3 color = texture(diffuseMap, fs_in.TexCoords).rgb;
    
    vec3 normal = texture(normalMap, fs_in.TexCoords).rgb;
    normal = normalize(normal * 2.f - 1.f);
    
    vec3 lightColor = vec3(5.f);
    
    // ambient 
    vec3 ambient = .3f * lightColor;
    // diffuse
    vec3 lightDir = normalize(fs_in.TangentLightPos - fs_in.TangentFragPos);
    float diff = max(dot(lightDir, normal), 0.f);
    vec3 diffuse = diff * lightColor;
    // specular
    vec3 viewDir = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);
    vec3 reflectDir = reflect(-lightDir, normal);
    vec3 halfDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfDir), 0.f), 64.f);
    vec3 specular = spec * lightColor;
    
    FragColor = vec4((ambient + diffuse + specular) * color, 1.f);
}