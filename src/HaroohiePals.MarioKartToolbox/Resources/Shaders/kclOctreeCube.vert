#version 400 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aMinPos;
layout (location = 2) in float aSize;
layout (location = 3) in uint aPickingId;

out vec4 vertexColor;
flat out uint pickingId;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform uint pickingGroupId;
uniform uint hoverId;

uniform vec4 normalColor;
uniform vec4 hoverColor;

void main()
{
    gl_Position = projection * view * model * vec4(aMinPos + aPosition * aSize, 1.0);

    if (aPickingId == hoverId)
        vertexColor = hoverColor;
    else
        vertexColor = normalColor;

    pickingId = aPickingId | (pickingGroupId << 24);
}