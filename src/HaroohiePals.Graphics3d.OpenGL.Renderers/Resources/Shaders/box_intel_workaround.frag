#version 400 core

layout (location = 0) out vec4 FragColor;
layout (location = 1) out uvec4 outPickingId;
layout (location = 2) out uint outFogBit;

in vec4 vertexColor; // the input variable from the vertex shader (same name and same type)
flat in uint pickingId;
sample in vec3 boxPos;

flat in uint hover;
flat in uint highlight;

uniform sampler2D texture0;

sample in vec2 texCoord;

const float lineWidth = 1.25;

vec3 pointOnLine(vec3 start, vec3 end, vec3 p)
{
    vec3 lVec = end - start;
    float t = dot(p - start, lVec) / dot(lVec, lVec);
    t = clamp(t, 0, 1);
    return start + lVec * t;
}

float distanceToLine(vec3 start, vec3 end, vec3 p)
{
    return length(p - pointOnLine(start, end, p));
}

float aastep(float threshold, float dist)
{
    float afwidth = fwidth(dist) * 0.5;
    return smoothstep(threshold - afwidth, threshold + afwidth, dist);
}

void main()
{
    FragColor = vertexColor;

    if (highlight == 1)
    {
        FragColor = clamp(FragColor + vec4(0.5, 0.5, 0.5, 0.0), 0.0, 1.0);
    }
    if (hover == 1)
    {
        FragColor = clamp(FragColor + vec4(0.3, 0.3, 0.3, 0.0), 0.0, 1.0);
    }

    outPickingId.r = pickingId & 0xFFu;
    outPickingId.g = (pickingId >> 8) & 0xFFu;
    outPickingId.b = (pickingId >> 16) & 0xFFu;
    outPickingId.a = (pickingId >> 24) & 0xFFu;

    outFogBit = 0;
}