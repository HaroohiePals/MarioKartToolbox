#version 400 core
//https://stackoverflow.com/a/60440937

layout(std140) uniform TVertex
{
   vec4 vertex[1024]; 
};

uniform mat4  u_mvp;
uniform vec2  u_resolution;
uniform float u_thickness;

void main()
{
    int line_i = gl_VertexID / 6;
    int tri_i  = gl_VertexID % 6;

    vec4 va[4];
    for (int i=0; i<4; ++i)
    {
        va[i] = u_mvp * vertex[line_i+i];
        va[i].xy /= va[i].w;
        va[i].xy = (va[i].xy + 1.0) * 0.5 * u_resolution;
    }

    vec2 v_line = va[2].xy - va[1].xy;
    if (length(v_line) > 0.1)
        v_line = normalize(v_line);
    vec2 nv_line = vec2(-v_line.y, v_line.x);
    
    vec4 pos;
    if (tri_i == 0 || tri_i == 1 || tri_i == 3)
    {
        vec2 v_pred = vec2(0);// va[1].xy - va[0].xy;// normalize(va[1].xy - va[0].xy);
        if (length(v_pred) > 0.1)
            v_pred = normalize(v_pred);
        vec2 v_miter = normalize(nv_line + vec2(-v_pred.y, v_pred.x));

        pos = va[1];
        pos.xy += v_miter * u_thickness * (tri_i == 1 ? -0.5 : 0.5) / dot(v_miter, nv_line);
    }
    else
    {
        vec2 v_succ = vec2(0);// va[3].xy - va[2].xy;// normalize(va[3].xy - va[2].xy);
        if (length(v_succ) > 0.1)
            v_succ = normalize(v_succ);
        vec2 v_miter = normalize(nv_line + vec2(-v_succ.y, v_succ.x));

        pos = va[2];
        pos.xy += v_miter * u_thickness * (tri_i == 5 ? 0.5 : -0.5) / dot(v_miter, nv_line);
    }

    pos.xy = pos.xy / u_resolution * 2.0 - 1.0;
    pos.xy *= pos.w;
    gl_Position = pos;
}
