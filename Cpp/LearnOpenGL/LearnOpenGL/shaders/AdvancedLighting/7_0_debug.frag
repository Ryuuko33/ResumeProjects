#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D outputTexture;

void main() {
         FragColor = vec4(texture(outputTexture, TexCoords).rrr, 1.f);
}