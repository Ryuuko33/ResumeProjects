#version 330 core
layout (location = 0) out vec3 gPosition;
layout (location = 1) out vec3 glNormal;
layout (location = 2) out vec4 glAlbedoSpec;

in vec2 TexCoords;
in vec3 FragPos;
in vec3 Normal;

uniform sampler2D texture_diffuse1;
uniform sampler2D texture_specular1;

void main() {
    gPosition = FragPos;
    glNormal = normalize(Normal);
    glAlbedoSpec.rgb = vec3(.95f);
    glAlbedoSpec.a = texture(texture_specular1, TexCoords).r;
}