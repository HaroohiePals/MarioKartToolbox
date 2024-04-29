#version 400 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormalOrColor;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in uint aMtxId;

out vec4 vertexColor; // specify a color output to the fragment shader
out vec2 texCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

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
    uint mtx = aMtxId & 0x1Fu;
    gl_Position = projection * view * model * posMtxStack[mtx] * vec4(aPosition, 1.0);

    float alpha = float((polygonAttr >> 16) & 0x1Fu) / 31.0;

    uint texCoordMode = (texImageParam >> 30) & 3u;

    texCoord = vec2(0);

    if ((aMtxId & (1u << 5)) != 0)
    {
        //not sure if we should multiply by the view matrix here
        vec3 nrm = normalize((view * model * dirMtxStack[mtx] * vec4(aNormalOrColor, 0.0)).xyz);

        vec3 finalColor = emission.rgb;

        for (int l = 0; l < 4; l++)
        {
            if ((polygonAttr & (1u << l)) == 0)
                continue;
            float ld = max(0, dot(-lightVectors[l].xyz, nrm));
            ld = clamp(ld, 0, 1);
            finalColor += (ld * lightColors[l].rgb) * diffuse.rgb;
            finalColor += lightColors[l].rgb * ambient.rgb;

            vec3 h = (lightVectors[l].xyz + vec3(0, 0, -1)) * 0.5;

            float ls = max(0, cos(2 * acos(dot(-h, nrm))));

            ls = clamp(ls, 0, 1);

            //todo: maybe support the specular reflection table
            //if (UsesSpecularReflectionTable)
            //    s[l] = (SpecularReflectionTable[(int)(ls * 127)] / 255f * lightColors[l].rgb) * specular.rgb;
            //else
                finalColor += (ls * lightColors[l].rgb) * specular.rgb;
        }

        finalColor = min(vec3(1), finalColor);
        vertexColor = vec4(finalColor, alpha);

        if (texCoordMode == 2)
            texCoord = vec2(texMtx[0][0] * aNormalOrColor.x + texMtx[1][0] * aNormalOrColor.y + texMtx[2][0] * aNormalOrColor.z + aTexCoord.x,
                            texMtx[0][1] * aNormalOrColor.x + texMtx[1][1] * aNormalOrColor.y + texMtx[2][1] * aNormalOrColor.z + aTexCoord.y);
    }
    else
        vertexColor = vec4(aNormalOrColor.rgb, alpha);

    uint sSize = 8 << ((texImageParam >> 20) & 7u);
    uint tSize = 8 << ((texImageParam >> 23) & 7u);

    if (texCoordMode == 0)
        texCoord = aTexCoord;
    else if (texCoordMode == 1)
        texCoord = vec2(texMtx[0][0] * aTexCoord.x + texMtx[1][0] * aTexCoord.y + texMtx[2][0] + texMtx[3][0],
                        texMtx[0][1] * aTexCoord.x + texMtx[1][1] * aTexCoord.y + texMtx[2][1] + texMtx[3][1]);
    else if (texCoordMode == 3)
        texCoord = vec2(texMtx[0][0] * aPosition.x + texMtx[1][0] * aPosition.y + texMtx[2][0] * aPosition.z + aTexCoord.x,
                        texMtx[0][1] * aPosition.x + texMtx[1][1] * aPosition.y + texMtx[2][1] * aPosition.z + aTexCoord.y);

    texCoord += vec2(0.001, 0.001);

    texCoord /= vec2(sSize, tSize);
}