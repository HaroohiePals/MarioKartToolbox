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

void main()
{
    vec2 circCoord = 2.0 * texCoord - 1.0;
    
    float d = dot(circCoord, circCoord);
    float delta = fwidth(d);

    float circle = 1.0 - smoothstep(1.0 - delta, 1.0, d);

    FragColor = vec4(vertexColor.rgb, circle);

    if (d <= 1.0 - (delta * 1.6))
        FragColor = clamp(FragColor - vec4(0.0, 0.0, 0.0, 0.5), 0.0, 1.0);

    if (highlight == 1)
    {
         FragColor = clamp(FragColor + vec4(0.5, 0.5, 0.5, 0.0), 0.0, 1.0);
    }
    if (hover == 1)
    {
        FragColor = clamp(FragColor + vec4(0.3, 0.3, 0.3, 0.0), 0.0, 1.0);
    }

    /*
    outPickingId.r = pickingId & 0xFFu;
    outPickingId.g = (pickingId >> 8) & 0xFFu;
    outPickingId.b = (pickingId >> 16) & 0xFFu;
    outPickingId.a = (pickingId >> 24) & 0xFFu;
    */

    outFogBit = 0;
}