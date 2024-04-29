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

const float wireframeThickness = 2;

void main()
{
    vec2 shiftTexCoord = mod(texCoord, 1.0) - vec2(0.5);

    vec2 dist = abs(shiftTexCoord * 2.0);
    float d = pow(max(dist.x, dist.y), 2.0);
    float delta = wireframeThickness * fwidth(d);

    float gradient = step(1.0 - delta, d);

    if (gradient > 0.9)
       FragColor = vec4(vertexColor.rgb, gradient);
    else
       discard;

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