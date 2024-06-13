#version 330 core
layout (triangles) in;
layout (line_strip, max_vertices = 3) out;

in VS_OUT {
    vec3 normal;
} gs_in[];

const float MAGNITUDE = .4f;

uniform mat4 projection;

void  GenerateLine(int index)
{
    gl_Position = projection * gl_in[index].gl_Position;
    EmitVertex();
    gl_Position = projection * (gl_in[index].gl_Position + vec4(gs_in[index].normal, 0.f) * MAGNITUDE);
    EmitVertex();
    
    EndPrimitive();
}

void main() {
    for (int i = 1; i < 4; i++) {
       GenerateLine(i);
    }
}