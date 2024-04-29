#version 400 core

layout (location = 0) out vec4 FragColor;
layout (location = 1) out uvec4 outPickingId;

in vec4 vertexColor;
flat in uint pickingId;

void main()
{
    FragColor = vertexColor;

    outPickingId.r = pickingId & 0xFFu;
    outPickingId.g = (pickingId >> 8) & 0xFFu;
    outPickingId.b = (pickingId >> 16) & 0xFFu;
    outPickingId.a = (pickingId >> 24) & 0xFFu;
}