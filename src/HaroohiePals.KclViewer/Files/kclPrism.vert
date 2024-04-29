#version 400 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec4 aColor;
layout (location = 2) in uint aPickingId;
layout (location = 3) in uint aCornerIdx;

out vec4 vertexColor;
sample out vec3 barycentric;
flat out uint pickingId;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    gl_Position = projection * view * model * vec4(aPosition, 1.0);
    vertexColor = aColor;

    if (aCornerIdx == 0)
        barycentric = vec3(1, 0, 0);
    else if (aCornerIdx == 1)
        barycentric = vec3(0, 1, 0);
    else
        barycentric = vec3(0, 0, 1);

    pickingId = aPickingId;
}