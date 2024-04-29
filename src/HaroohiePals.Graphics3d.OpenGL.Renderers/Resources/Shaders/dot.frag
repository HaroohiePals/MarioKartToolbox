#version 400 core

layout (location = 0) out vec4 FragColor;
layout (location = 1) out uvec4 outPickingId;
layout (location = 2) out uint outFogBit;

in vec4 vertexColor; // the input variable from the vertex shader (same name and same type)
flat in uint pickingId;
sample in vec3 boxPos;

flat in uint hover;
flat in uint highlight;
flat in uint hasTexture;
flat in float texCoordAngle;

uniform sampler2D texture0;

vec2 rotateUV(vec2 uv, float rotation)
{
    float mid = 0.5;
    return vec2(
        cos(rotation) * (uv.x - mid) + sin(rotation) * (uv.y - mid) + mid,
        cos(rotation) * (uv.y - mid) - sin(rotation) * (uv.x - mid) + mid
    );
}

void main()
{
    if (hasTexture == 1)
    {
        vec2 texCoord = ((gl_PointCoord - 0.5) * 1.4142) + 0.5;
        
        texCoord = rotateUV(texCoord, texCoordAngle);

        FragColor = texture(texture0, texCoord) * vertexColor;
    }
    else
    {
        vec2 circCoord = 2.0 * gl_PointCoord - 1.0;

        float d = dot(circCoord, circCoord);
        float delta = fwidth(d);

        float circle = 1.0 - smoothstep(1.0 - delta, 1.0, d);

        FragColor = vec4(vertexColor.rgb, circle);
    }

    if (highlight == 1)
    {
        FragColor = clamp(FragColor + vec4(0.5, 0.5, 0.5, 0.0), 0.0, 1.0);
    }
    if (hover == 1)
    {
        FragColor = clamp(FragColor + vec4(0.3, 0.3, 0.3, 0.0), 0.0, 1.0);
    }
    
    if (FragColor.a < 0.001)
        discard; //always skip 100% transparent pixels

    outPickingId.r = pickingId & 0xFFu;
    outPickingId.g = (pickingId >> 8) & 0xFFu;
    outPickingId.b = (pickingId >> 16) & 0xFFu;
    outPickingId.a = (pickingId >> 24) & 0xFFu;

    outFogBit = 0;
}