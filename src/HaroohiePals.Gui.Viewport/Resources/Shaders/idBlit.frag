#version 400 core

uniform usampler2DMS idBufferTex;
uniform vec2 viewportSize;

in vec2 texCoord;

out uvec4 outPickingId;

void main()
{
    ivec2 intTexCoord = ivec2(int(viewportSize.x * texCoord.x), int(viewportSize.y * texCoord.y));

    outPickingId = texelFetch(idBufferTex, intTexCoord, 0);
}