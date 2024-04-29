#version 400 core

layout (location = 0) out vec4 fragColor;
layout (location = 1) out uvec4 outPickingId;
layout (location = 2) out uint outFogBit;

uniform uint uPickingId;
uniform vec4 uColor;

void main()
{
    fragColor = uColor;

    outPickingId.r = uPickingId & 0xFFu;
    outPickingId.g = (uPickingId >> 8) & 0xFFu;
    outPickingId.b = (uPickingId >> 16) & 0xFFu;
    outPickingId.a = (uPickingId >> 24) & 0xFFu;

    outFogBit = 0;
}
