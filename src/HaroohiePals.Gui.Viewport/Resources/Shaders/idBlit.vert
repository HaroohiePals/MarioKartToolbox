#version 330 core

uniform mat4 projMtxScreenQuad;
uniform vec2 viewportSize;

layout(location = 0) in vec2 aPos;

out vec2 texCoord;

void main()
{
    gl_Position = projMtxScreenQuad * vec4(aPos * viewportSize, 0, 1);
    texCoord = vec2(aPos.x, 1.0 - aPos.y);
}