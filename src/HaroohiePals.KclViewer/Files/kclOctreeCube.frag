#version 400 core

layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 outPickingId;

in vec4 vertexColor;
flat in uint pickingId;

void main()
{
    FragColor = vertexColor;

    outPickingId.r = float(pickingId & 0xFFu) / 255.0;
    outPickingId.g = float((pickingId >> 8) & 0xFFu) / 255.0;
    outPickingId.b = float((pickingId >> 16) & 0xFFu) / 255.0;
    outPickingId.a = 1.0;
}