#version 400 core

layout (location = 0) out vec4 FragColor;

in float near;
in float far;
in float scale;
in vec3 nearPoint;
in vec3 farPoint;
in mat4 fragView;
in mat4 fragProj;

vec4 grid(vec3 fragPos3D, float scale) {
    vec2 coord = fragPos3D.xz * scale;
    vec2 derivative = fwidth(coord) * 1.5;
    vec2 grid = abs(fract(coord - 0.5) - 0.5) / derivative;
    float line = min(grid.x, grid.y);
    float minimumz = min(derivative.y, 1);
    float minimumx = min(derivative.x, 1);
    vec4 color = vec4(0.2, 0.2, 0.2, 1.0 - min(line, 1.0));
    // z axis
    if (fragPos3D.x > -100.0 * minimumx && fragPos3D.x < 100.0 * minimumx)
        color.z = 1.0;
    // x axis
    if (fragPos3D.z > -100.0 * minimumz && fragPos3D.z < 100.0 * minimumz)
        color.x = 1.0;
    return color;
}

float computeDepth(vec3 pos) {
    vec4 clip_space_pos = fragProj * fragView * vec4(pos.xyz, 1.0);
    float ndcDepth = (clip_space_pos.z / clip_space_pos.w);
    return ((gl_DepthRange.diff * ndcDepth) + gl_DepthRange.near + gl_DepthRange.far) / 2.0;
}

float computeLinearDepth(vec3 pos) {
    vec4 clip_space_pos = fragProj * fragView * vec4(pos.xyz, 1.0);
    float clip_space_depth = (clip_space_pos.z / clip_space_pos.w) * 2.0 - 1.0; // put back between -1 and 1
    float linearDepth = (2.0 * near * far) / (far + near - clip_space_depth * (far - near)); // get linear value between 0.01 and 100
    return linearDepth / far; // normalize
}

void main() {
    float t = -nearPoint.y / (farPoint.y - nearPoint.y);
    vec3 fragPos3D = nearPoint + t * (farPoint - nearPoint);

    gl_FragDepth = computeDepth(fragPos3D);

    float linearDepth = computeLinearDepth(fragPos3D);
    float fading = max(0, (0.5 - linearDepth));

    FragColor = (grid(fragPos3D, 10.0 / scale) + grid(fragPos3D, 1.0 / scale)) * float(t > 0.0); // adding multiple resolution for the grid
    FragColor.a *= fading;
}