#version 400 core

uniform sampler2DMS depthBufferTex;
uniform usampler2DMS fogBufferTex;
uniform vec2 viewportSize;
uniform mat4 invProjMtx;
uniform mat4 invViewMtx;
uniform mat4 mkdsProjMtx;
uniform int fogShift;
uniform int fogOffset;
uniform int fogTable[32];
uniform vec4 fogColor;

in vec2 texCoord;

out vec4 outputColor;

void main()
{
    ivec2 intTexCoord = ivec2(int(viewportSize.x * texCoord.x), int(viewportSize.y * texCoord.y));

    vec4 samp = texelFetch(depthBufferTex, intTexCoord, gl_SampleID);

    uvec4 fogFlag = texelFetch(fogBufferTex, intTexCoord, gl_SampleID);
    
    if (fogFlag.r == 0)
        discard;

    float depth = samp.r;

    float z = depth * 2.0 - 1.0;

    vec4 clipSpacePosition = vec4(texCoord * 2.0 - 1.0, z, 1.0);
    vec4 viewSpacePosition = invProjMtx * clipSpacePosition;

    // Perspective division
    viewSpacePosition /= viewSpacePosition.w;

    vec4 worldSpacePosition = invViewMtx * viewSpacePosition;
    worldSpacePosition = vec4(worldSpacePosition.x / 16.0, worldSpacePosition.y / 16.0, worldSpacePosition.z / 16.0, 1.0);

    mat4 newInvView = invViewMtx;

    newInvView[3][0] /= 16.0;
    newInvView[3][1] /= 16.0;
    newInvView[3][2] /= 16.0;

    mat4 newView = inverse(newInvView);

    vec4 newClipSpacePosition = mkdsProjMtx * newView * worldSpacePosition;

    int fogDepth;
    // w mode
    fogDepth = int(newClipSpacePosition.w * 4096.0) >> 9;
    fogDepth = clamp(fogDepth, 0, 0x7FFF);
    fogDepth -= fogOffset;

    int f_ivl = 0x400 >> fogShift;

    int fogDensity;

    if (fogDepth <= f_ivl - 1)
        fogDensity = fogTable[0];
    else if (fogDepth >= f_ivl * 32)
        fogDensity = fogTable[31];
    else
    {
        int tableIdx = fogDepth / f_ivl; 
        fogDensity = fogTable[tableIdx - 1] + ((fogTable[tableIdx] - fogTable[tableIdx - 1]) * (fogDepth - f_ivl * tableIdx)) / f_ivl;
    }

    if (fogDensity >= 127)
        fogDensity = 128;

    outputColor = vec4(fogColor.rgb, fogDensity / 128.0);
}