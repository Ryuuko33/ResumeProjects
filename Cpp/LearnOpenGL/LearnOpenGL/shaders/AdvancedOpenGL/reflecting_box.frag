﻿#version 330 core
out vec4 FragColor;

in vec3 Normal;
in vec3 Position;

uniform vec3 cameraPos;
uniform samplerCube skybox;

void main()
{
    vec3 i = normalize(Position - cameraPos);
    vec3 r = reflect(i, normalize(Normal));
    
    FragColor = vec4(texture(skybox, r).rgb, 1.f);
}