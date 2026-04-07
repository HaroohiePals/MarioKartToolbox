#version 400 core

layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 outPickingId;
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
    FragColor = texture(texture0, texCoord);

    if (highlight == 1)
    {
         FragColor = clamp(FragColor + vec4(0.5, 0.5, 0.5, 0.0), 0.0, 1.0);
    }
    if (hover == 1)
    {
        FragColor = clamp(FragColor + vec4(0.3, 0.3, 0.3, 0.0), 0.0, 1.0);
    }

    outPickingId.r = float(pickingId & 0xFFu) / 255.0;
    outPickingId.g = float((pickingId >> 8) & 0xFFu) / 255.0;
    outPickingId.b = float((pickingId >> 16) & 0xFFu) / 255.0;
    outPickingId.a = 1.0;

    outFogBit = 0;
}