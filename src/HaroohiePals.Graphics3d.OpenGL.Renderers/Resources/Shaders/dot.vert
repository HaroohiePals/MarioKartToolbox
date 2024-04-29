#version 400 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormalOrColor;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in uint aMtxId;

layout (location = 4) in mat4 aTransform;
layout (location = 8) in vec3 aColor;
layout (location = 9) in uint aPickingId;
layout (location = 10) in uint aHasText;
layout (location = 11) in uint aHover;
layout (location = 12) in uint aHighlight;
layout (location = 13) in float aTexCoordAngle;

out vec4 vertexColor; // specify a color output to the fragment shader
flat out uint pickingId;
sample out vec3 boxPos;

flat out uint hover;
flat out uint highlight;
flat out uint hasTexture;
flat out float texCoordAngle;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    boxPos = aPosition;
    vec4 pos = vec4(aPosition * 10.0, 1.0);
    
    gl_Position = projection * view * model * aTransform * pos;
    vertexColor = vec4(aColor, 1.0);

    hover = aHover;
    highlight = aHighlight;
    hasTexture = aHasText;
    texCoordAngle = aTexCoordAngle;

    pickingId = aPickingId;
}