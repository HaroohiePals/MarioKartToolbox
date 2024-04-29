#version 400 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormalOrColor;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in uint aMtxId;

out float near;
out float far;
out float scale;
out vec3 nearPoint;
out vec3 farPoint;
out mat4 fragView;
out mat4 fragProj;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform float gridScale;
uniform float gridFar;
uniform float gridNear;

vec3 UnprojectPoint(float x, float y, float z) {
    mat4 viewInv = inverse(view);
    mat4 projInv = inverse(projection);
    vec4 unprojectedPoint =  viewInv * projInv * vec4(x, y, z, 1.0);
    return unprojectedPoint.xyz / unprojectedPoint.w;
}

void main()
{   
    fragView = view;
    fragProj = projection;
    near = gridNear;
    far = gridFar;
    scale = gridScale;

    vec3 p = aPosition.xyz;

    nearPoint = UnprojectPoint(p.x, p.y, 0.0).xyz; // unprojecting on the near plane
    farPoint = UnprojectPoint(p.x, p.y, 1.0).xyz; // unprojecting on the far plane

    gl_Position = vec4(p, 1.0);
}