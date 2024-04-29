#version 400 core

layout (location = 0) out vec4 FragColor;
layout (location = 1) out uvec4 outPickingId;
layout (location = 2) out uint outFogBit;

in vec4 vertexColor; // the input variable from the vertex shader (same name and same type)
in vec2 texCoord;

uniform sampler2D texture0;
uniform uint pickingId;

layout (std140) uniform uboData
{
    uint polygonAttr;
    uint texImageParam;
    uint flags;
    uint padding;
    vec4 diffuse;
    vec4 ambient;
    vec4 specular;
    vec4 emission;
    vec4 lightVectors[4];
    vec4 lightColors[4];
    mat4 posMtxStack[32]; //index 31 is the current matrix
    mat4 dirMtxStack[32]; //index 31 is the current matrix
    mat4 texMtx;
};

void main()
{
    uint mode = (polygonAttr >> 4) & 3u;
    uint texFmt = (texImageParam >> 26) & 7u;

    vec4 tex;
    if (texFmt == 0)
        tex = vec4(1.0);
    else
        tex = texture(texture0, texCoord);

    //todo: support toon and maybe shadow
    if (mode == 0 || texFmt == 0)
        FragColor = tex * vertexColor;
    else if (mode == 1 || mode == 3)
        FragColor = vec4(mix(vertexColor.rgb, tex.rgb, tex.a), vertexColor.a);

    if ((flags & (1u << 1)) == 0 && FragColor.a <= 0.99)
        discard; //opaque pass skips translucent pixels
    else if ((flags & (1u << 1)) != 0 && FragColor.a > 0.99)
        discard; //translucent pass skips opaque pixels

    if (FragColor.a < 0.01)
        discard; //always skip 100% transparent pixels

    outPickingId.r = pickingId & 0xFFu;
    outPickingId.g = (pickingId >> 8) & 0xFFu;
    outPickingId.b = (pickingId >> 16) & 0xFFu;
    outPickingId.a = (pickingId >> 24) & 0xFFu;
    //outPickingId = pickingId;
    outFogBit = (polygonAttr >> 15) & 1u;
}