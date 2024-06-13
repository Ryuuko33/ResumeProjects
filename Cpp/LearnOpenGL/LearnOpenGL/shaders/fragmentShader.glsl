#version 330 core
out vec4 FragColor;

in vec3 VertexPos;
in vec2 TexCoord;

uniform sampler2D texture1;
uniform sampler2D texture2;
uniform float alpha;

void main()
{
//    FragColor = vec4(ourColor, 1.0f);
//    FragColor = vec4(VertexPos, 1.0f);
//    FragColor = texture(ourTexture, TexCoord) * vec4(ourColor, 1.f);
    vec2 _TexCoord_Flip_X = vec2(-TexCoord.x, TexCoord.y);
    FragColor = mix(texture(texture1, TexCoord), texture(texture2, _TexCoord_Flip_X), alpha);
}
