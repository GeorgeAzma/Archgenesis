#ifndef GRAYSCALE_INCLUDED
#define GRAYSCALE_INCLUDED

#define RADIUS 8.0
#define EDGE 0.35

float3 random3(float3 c)
{
    float j = 4096.0 * sin(dot(c, float3(17.0, 59.4, 15.0)));
    float3 r;
    r.z = frac(512.0 * j);
    j *= .125;
    r.x = frac(512.0 * j);
    j *= .125;
    r.y = frac(512.0 * j);
    return r - 0.5;
}

float simplex3d(float3 p)
{
    float3 s = floor(p + dot(p, float3(0.3333333, 0.3333333, 0.3333333)));
    float3 x = p - s + dot(s, float3(0.1666667, 0.1666667, 0.1666667));
    float3 e = step(float3(0, 0, 0), x - x.yzx);
    float3 i1 = e * (1.0 - e.zxy);
    float3 i2 = 1.0 - e.zxy * (1.0 - e);
    float3 x1 = x - i1 + 0.1666667;
    float3 x2 = x - i2 + 2.0 * 0.1666667;
    float3 x3 = x - 1.0 + 3.0 * 0.1666667;
    float4 w, d;
    w.x = dot(x, x);
    w.y = dot(x1, x1);
    w.z = dot(x2, x2);
    w.w = dot(x3, x3);
    w = max(0.6 - w, 0.0);
    d.x = dot(random3(s), x);
    d.y = dot(random3(s + i1), x1);
    d.z = dot(random3(s + i2), x2);
    d.w = dot(random3(s + 1.0), x3);
    w *= w;
    w *= w;
    d *= w;
    return dot(d, float4(52, 52, 52, 52));
}

float speedLines(float2 uv, float time)
{
    time *= 1.;
    float scale = 50.0;
    uv -= 0.5;
    float2 p = float2(0.5, 0.5) + normalize(uv) * min(length(uv), 0.05);
    float3 p3 = scale * 0.25 * float3(p.xy, 0) + float3(0, 0, time * 0.025);
    float noise = simplex3d(p3 * 32.0) * 0.5 + 0.5;
    float dist = abs(clamp(length(uv) / RADIUS, 0.0, 1.0) * noise * 2. - 1.);
    float stepped = smoothstep(EDGE - .5, EDGE + .5, noise * (1.0 - pow(dist, 4.0)));
    float final = smoothstep(EDGE - 0.2, EDGE + 0.2, noise * stepped);
    return final;
}

void Screen_float(float3 col, float2 uv, float time, float speedLinesUniform, out float3 color)
{
    color = col + speedLines(uv, time) * speedLinesUniform * 0.2;
}

#endif // GRAYSCALE_INCLUDED