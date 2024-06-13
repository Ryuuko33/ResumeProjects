#version 330 core
in vec2 TexCoords;

out vec4 FragColor;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gAlbedoSpec;

struct Light{    
    vec3 Position;
    vec3 Color;

    float Linear;
    float Quadratic;
    
    float Radius;
};
  
const int NR_LIGHTS = 32;
uniform Light lights[NR_LIGHTS];
uniform vec3 viewPos;

void main() {
    // retrieve data from G-buffer
    vec3 FragPos = texture(gPosition, TexCoords).rgb;
    vec3 Normal = texture(gNormal, TexCoords).rgb;
    vec3 Albedo = texture(gAlbedoSpec, TexCoords).rgb;
    float Specular = texture(gAlbedoSpec, TexCoords).a;
    
    // calculate lighting as usual
    // ambient
    vec3 lighting = Albedo * .1f;
    // diffuse
    vec3 viewDir = normalize(viewPos - FragPos);
    for (int i = 0; i < NR_LIGHTS; i++) {
        
        float distance = length(lights[i].Position - FragPos);
        if(distance < lights[i].Radius)
        {
            vec3 lightDir = normalize(lights[i].Position - FragPos);
            vec3 diffuse = max(dot(Normal, lightDir), 0.0) * Albedo * lights[i].Color;
            // specular
            vec3 halfwayDir = normalize(lightDir + viewDir);
            float spec = pow(max(dot(Normal, halfwayDir), 0.0), 16.f);
            vec3 specular = lights[i].Color * spec * Specular;
            // attenuation
            float distacne = length(lights[i].Position - FragPos);
            float attenuation = 1.0 / (1.0 + lights[i].Linear * distacne + lights[i].Quadratic * distacne * distacne);
            diffuse *= attenuation;
            specular *= attenuation;
            lighting += diffuse + specular;
        }
    }
    
    FragColor = vec4(lighting, 1.f);
}