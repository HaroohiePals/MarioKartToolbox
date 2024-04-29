#version 400 core

layout (location = 0) out vec4 FragColor;
layout (location = 1) out uvec4 outPickingId;

in vec4 vertexColor;
flat in uint pickingId;
sample in vec3 barycentric;

uniform float wireframeThickness;
uniform vec4 wireframeColor;


float aastep (float threshold, float dist)
{
    float afwidth = fwidth(dist) * 0.5;
    return smoothstep(threshold - afwidth, threshold + afwidth, dist);
}

// This function is not currently used, but it can be useful
// to achieve a fixed width wireframe regardless of z-depth
float computeScreenSpaceWireframe(vec3 barycentric, float lineWidth)
{
    vec3 dist = fwidth(barycentric);
    vec3 smoothed = smoothstep(dist * ((lineWidth * 0.5) - 0.5), dist * ((lineWidth * 0.5) + 0.5), barycentric);
    return 1.0 - min(min(smoothed.x, smoothed.y), smoothed.z);
}

void main()
{
    if (wireframeThickness > 0)
    {
        float d = min(min(barycentric.x, barycentric.y), barycentric.z);

        float positionAlong = max(barycentric.x, barycentric.y);
        if (barycentric.y < barycentric.x && barycentric.y < barycentric.z)
            positionAlong = 1.0 - positionAlong;

        float computedThickness = computeScreenSpaceWireframe(barycentric, wireframeThickness);

        float edge = 1.0 - aastep(computedThickness, d);

        vec4 outColor = vec4(0.0);

        vec3 mainStroke = mix(vertexColor.rgb, wireframeColor.rgb, edge);
        outColor.a = mix(vertexColor.a, wireframeColor.a, edge);//vertexColor.a;
        outColor.rgb = mainStroke;

        FragColor = outColor;
    }
    else
        FragColor = vertexColor;

    outPickingId.r = pickingId & 0xFFu;
    outPickingId.g = (pickingId >> 8) & 0xFFu;
    outPickingId.b = (pickingId >> 16) & 0xFFu;
    outPickingId.a = (pickingId >> 24) & 0xFFu;
}