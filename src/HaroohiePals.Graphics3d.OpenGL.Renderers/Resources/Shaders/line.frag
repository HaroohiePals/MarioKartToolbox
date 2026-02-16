#version 400 core

layout (location = 0) out vec4 fragColor;
layout (location = 1) out vec4 outPickingId;
layout (location = 2) out uint outFogBit;

uniform uint uPickingId;
uniform vec4 uColor;

void main()
{
    fragColor = uColor;

    outPickingId.r = float(uPickingId & 0xFFu) / 255.0;
    outPickingId.g = float((uPickingId >> 8) & 0xFFu) / 255.0;
    outPickingId.b = float((uPickingId >> 16) & 0xFFu) / 255.0;
    outPickingId.a = 1.0;

    outFogBit = 0;
}
