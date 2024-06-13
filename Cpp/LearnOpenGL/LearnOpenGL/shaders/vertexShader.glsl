#version 330 core

layout (location = 0) in vec3 aPos;
layout (location =1) in vec2 aTexCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 VertexPos;
out vec3 ourColor;
out vec2 TexCoord;

void main()
{
//   vec3 _Pos = vec3(aPos.x + XOffset, -aPos.y, aPos.z);
   VertexPos = aPos;
   gl_Position = projection * view * model * vec4(aPos, 1.0f);
   TexCoord = aTexCoord;
}